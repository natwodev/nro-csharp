using Nro.Data.Db;
using System.Globalization;
using System.Text;

namespace Nro.Data.Db;

// Đổi dấu ? tham số kiểu JDBC sang $n của Npgsql, chỉ thay dấu ngoài chuỗi, định danh, chú thích và chuỗi dollar-quote của PostgreSQL.
// Nguồn Java: jbcd/ConnectDB.java (đổi ? kiểu JDBC sang $n của Npgsql; toán tử jsonb ?, ?|, ?& không dùng được qua lớp này và chuỗi/chú thích chưa đóng được xem đến cuối lệnh).
public static class SqlPlaceholders
{
    // Đếm dấu ? là tham số thực, không tính dấu trong chuỗi, định danh hoặc chú thích.
    public static int Count(string sql)
    {
        ArgumentNullException.ThrowIfNull(sql);
        return Scan(sql, null);
    }

    // Đổi mỗi ? tham số thực theo thứ tự thành $1..$n; lệch số đối số ném SqlPlaceholderMismatchException như JDBC.
    public static string Translate(string sql, int argumentCount)
    {
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentOutOfRangeException.ThrowIfNegative(argumentCount);

        var output = new StringBuilder(sql.Length + 8);
        var found = Scan(sql, output);
        if (found != argumentCount)
        {
            throw new SqlPlaceholderMismatchException(found, argumentCount);
        }

        return output.ToString();
    }

    // Giữ quirk ConnectDB.executeUpdate: SQL bắt đầu đúng "insert" và kết thúc đúng "()" thay mọi "()" bằng danh sách ?; với 0 đối số, C# giữ SQL để phân biệt bất khả với overload Java không đối số (Java: executeUpdate).
    public static string ExpandInsertValues(string sql, int argumentCount)
    {
        ArgumentNullException.ThrowIfNull(sql);
        if (argumentCount <= 0)
        {
            return sql;
        }

        if (!sql.StartsWith("insert", StringComparison.Ordinal) || !sql.EndsWith("()", StringComparison.Ordinal))
        {
            return sql;
        }

        var list = new StringBuilder("(");
        for (var i = 0; i < argumentCount; i++)
        {
            list.Append('?');
            list.Append(i < argumentCount - 1 ? ',' : ')');
        }

        return sql.Replace("()", list.ToString(), StringComparison.Ordinal);
    }

    // Bộ quét SQL.

    // Quét SQL, ghi bản đã đổi khi output khác null và trả số tham số.
    private static int Scan(string sql, StringBuilder? output)
    {
        var count = 0;
        var i = 0;
        var length = sql.Length;
        while (i < length)
        {
            var c = sql[i];
            int end;
            switch (c)
            {
                case '\'':
                    end = SkipString(sql, i);
                    break;
                case '"':
                    end = SkipQuotedIdentifier(sql, i);
                    break;
                case '-' when i + 1 < length && sql[i + 1] == '-':
                    end = SkipLineComment(sql, i);
                    break;
                case '/' when i + 1 < length && sql[i + 1] == '*':
                    end = SkipBlockComment(sql, i);
                    break;
                case '$':
                    end = SkipDollarQuote(sql, i);
                    break;
                case '?':
                    count++;
                    output?.Append('$').Append(count.ToString(CultureInfo.InvariantCulture));
                    if (output is not null && i + 1 < length && IsIdentifierPart(sql[i + 1]))
                    {
                        // MySQL chấp nhận "?and", còn PostgreSQL coi "$1and" là ký tự thừa sau tham số.
                        output.Append(' ');
                    }

                    i++;
                    continue;
                default:
                    output?.Append(c);
                    i++;
                    continue;
            }

            output?.Append(sql, i, end - i);
            i = end;
        }

        return count;
    }

    // Bỏ qua chuỗi '...' từ start, trả vị trí sau dấu nháy đóng hoặc cuối văn bản.
    private static int SkipString(string sql, int start)
    {
        // E'...' hoặc e'...' cho phép \ thoát ký tự sau; E phải đứng sát dấu nháy và không thuộc định danh khác.
        var escapeString = start > 0
            && (sql[start - 1] == 'E' || sql[start - 1] == 'e')
            && (start == 1 || !IsIdentifierPart(sql[start - 2]));

        var i = start + 1;
        while (i < sql.Length)
        {
            var c = sql[i];
            if (escapeString && c == '\\')
            {
                i += 2;
            }
            else if (c == '\'')
            {
                if (i + 1 < sql.Length && sql[i + 1] == '\'')
                {
                    i += 2;
                }
                else
                {
                    return i + 1;
                }
            }
            else
            {
                i++;
            }
        }

        return sql.Length;
    }

    // Bỏ qua định danh "..."; "" là dấu nháy đã thoát.
    private static int SkipQuotedIdentifier(string sql, int start)
    {
        var i = start + 1;
        while (i < sql.Length)
        {
            if (sql[i] == '"')
            {
                if (i + 1 < sql.Length && sql[i + 1] == '"')
                {
                    i += 2;
                }
                else
                {
                    return i + 1;
                }
            }
            else
            {
                i++;
            }
        }

        return sql.Length;
    }

    // Bỏ qua chú thích -- đến trước ký tự xuống dòng.
    private static int SkipLineComment(string sql, int start)
    {
        var i = start + 2;
        while (i < sql.Length && sql[i] != '\n' && sql[i] != '\r')
        {
            i++;
        }

        return i;
    }

    // Bỏ qua chú thích khối /* ... */ lồng nhau theo PostgreSQL.
    private static int SkipBlockComment(string sql, int start)
    {
        var depth = 1;
        var i = start + 2;
        while (i < sql.Length)
        {
            if (sql[i] == '/' && i + 1 < sql.Length && sql[i + 1] == '*')
            {
                depth++;
                i += 2;
            }
            else if (sql[i] == '*' && i + 1 < sql.Length && sql[i + 1] == '/')
            {
                depth--;
                i += 2;
                if (depth == 0)
                {
                    return i;
                }
            }
            else
            {
                i++;
            }
        }

        return sql.Length;
    }

    // Bỏ qua $$...$$ hoặc $tag$...$tag$; $ không mở chuỗi trả start + 1 để chỉ sao chép chính ký tự đó.
    private static int SkipDollarQuote(string sql, int start)
    {
        if (start > 0 && IsIdentifierPart(sql[start - 1]))
        {
            return start + 1;
        }

        var j = start + 1;
        if (j >= sql.Length)
        {
            return start + 1;
        }

        if (sql[j] != '$')
        {
            if (!IsTagStart(sql[j]))
            {
                return start + 1;
            }

            j++;
            while (j < sql.Length && IsTagPart(sql[j]))
            {
                j++;
            }

            if (j >= sql.Length || sql[j] != '$')
            {
                return start + 1;
            }
        }

        var delimiter = sql.Substring(start, j - start + 1);
        var close = sql.IndexOf(delimiter, j + 1, StringComparison.Ordinal);
        return close < 0 ? sql.Length : close + delimiter.Length;
    }

    private static bool IsIdentifierPart(char c) => char.IsLetterOrDigit(c) || c == '_' || c == '$' || c > 127;

    private static bool IsTagStart(char c) => char.IsLetter(c) || c == '_' || c > 127;

    private static bool IsTagPart(char c) => char.IsLetterOrDigit(c) || c == '_' || c > 127;
}
