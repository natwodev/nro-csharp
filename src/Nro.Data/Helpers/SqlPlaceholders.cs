using System.Globalization;
using System.Text;

namespace Nro.Data;

/// <summary>
/// Translates the JDBC-style positional parameters (<c>?</c>) of the Java code into Npgsql positional parameters (<c>$1..$n</c>).
/// </summary>
/// <remarks>
/// <para>
/// The Java version (<c>ConnectDB</c>) passes SQL containing <c>?</c> to a <c>PreparedStatement</c>; Npgsql/PostgreSQL does not understand <c>?</c>.
/// The translator scans the SQL using PostgreSQL's lexical syntax and ONLY replaces a <c>?</c> that is outside:
/// </para>
/// <list type="bullet">
/// <item>strings <c>'...'</c> (<c>''</c> is an escaped quote; <c>E'...'</c> in addition lets <c>\</c> escape the next character),</item>
/// <item>identifiers <c>"..."</c> (<c>""</c> is an escaped quote),</item>
/// <item>line comments <c>-- ...</c> (to the end of the line),</item>
/// <item>block comments <c>/* ... */</c> (PostgreSQL allows them to nest, so the translator also tracks the depth),</item>
/// <item>dollar-quoted strings <c>$$...$$</c> and <c>$tag$...$tag$</c>.</item>
/// </list>
/// <para>
/// Known limitation: the jsonb operators <c>?</c>, <c>?|</c>, <c>?&amp;</c> cannot be used in SQL that goes through this translator
/// (a <c>?</c> is always a parameter); use the functions <c>jsonb_exists</c>, <c>jsonb_exists_any</c>, <c>jsonb_exists_all</c> instead.
/// An unterminated string or comment is treated as running to the end of the statement (PostgreSQL will report a syntax error).
/// </para>
/// </remarks>
public static class SqlPlaceholders
{
    /// <summary>Counts the <c>?</c> that are real parameters (not those inside strings, identifiers or comments).</summary>
    public static int Count(string sql)
    {
        ArgumentNullException.ThrowIfNull(sql);
        return Scan(sql, null);
    }

    /// <summary>
    /// Replaces every <c>?</c> that is a real parameter with <c>$1..$n</c> in order of appearance.
    /// The number of <c>?</c> must equal <paramref name="argumentCount"/>; if they differ it throws <see cref="SqlPlaceholderMismatchException"/>
    /// (JDBC also fails in both directions of mismatch: a missing value or an index out of range).
    /// </summary>
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

    /// <summary>
    /// QUIRK of <c>ConnectDB.executeUpdate(String, Object...)</c> (Java), preserved:
    /// if <paramref name="sql"/> starts with exactly <c>insert</c> (lower case, no leading whitespace)
    /// and ends with exactly <c>()</c> (no trailing whitespace or semicolon),
    /// then EVERY <c>()</c> pair in the string is replaced by a list <c>(?,?,...)</c> sized by the number of arguments.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Consequences of the Java version that this one keeps: the comparison is case-sensitive (<c>INSERT</c> is not handled);
    /// <c>startsWith("insert")</c> has no word boundary (<c>insertion ...()</c> matches too); it replaces ALL the <c>()</c>,
    /// including <c>insert into t() values()</c> (both pairs are replaced).
    /// </para>
    /// <para>
    /// Deliberate difference from the Java version: when <paramref name="argumentCount"/> is 0 the SQL is returned unchanged (not rewritten).
    /// Java picks the <c>executeUpdate(String)</c> overload, which has no quirk, when called with no arguments; only when an explicitly empty
    /// array is passed to the <c>Object...</c> overload does Java produce broken SQL ending in <c>(</c>.
    /// C# cannot tell the two calls apart, so it takes the safe choice.
    /// </para>
    /// <para>This method is only for the <c>ExecuteUpdateAsync</c> path; <c>ExecuteQueryAsync</c> has no quirk (exactly as in Java).</para>
    /// </remarks>
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

    // ------------------------------------------------------------------ scanner

    /// <summary>Scans the SQL; if <paramref name="output"/> is not null, writes the translated text into it. Returns the number of parameters.</summary>
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
                        // MySQL accepts "?and", while PostgreSQL treats "$1and" as a "trailing junk after parameter" error.
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

    /// <summary>A string <c>'...'</c> starting at <paramref name="start"/>; returns the position just after the closing quote (or the end of the text).</summary>
    private static int SkipString(string sql, int start)
    {
        // E'...' (or e'...'): adds the rule that \ escapes the next character. The E must come immediately before the quote and not be part of another identifier.
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

    /// <summary>An identifier <c>"..."</c>; <c>""</c> is an escaped quote.</summary>
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

    /// <summary>A comment <c>-- ...</c> to the end of the line; returns the position of the newline character (does not consume it).</summary>
    private static int SkipLineComment(string sql, int start)
    {
        var i = start + 2;
        while (i < sql.Length && sql[i] != '\n' && sql[i] != '\r')
        {
            i++;
        }

        return i;
    }

    /// <summary>A block comment <c>/* ... */</c>, nested like in PostgreSQL.</summary>
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

    /// <summary>
    /// <c>$$...$$</c> or <c>$tag$...$tag$</c>. If the <c>$</c> at <paramref name="start"/> does not open a dollar-quoted string
    /// (e.g. <c>$1</c>, or in the middle of an identifier such as <c>a$b</c>) it returns <c>start + 1</c> so that only that character is copied.
    /// </summary>
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
