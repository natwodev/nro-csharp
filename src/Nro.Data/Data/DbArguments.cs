using Npgsql;
using NpgsqlTypes;

namespace Nro.Data;

/// <summary>Converts parameter values and values read back between the types the game code uses and the Npgsql types.</summary>
internal static class DbArguments
{
    /// <summary>
    /// Parameter value to <see cref="NpgsqlParameter"/> (unnamed, by position <c>$n</c>).
    /// <list type="bullet">
    /// <item><c>null</c> becomes <see cref="DBNull.Value"/> (Java <c>setObject(i, null)</c>).</item>
    /// <item><b>Strings are sent as "unknown" (untyped)</b>, so PostgreSQL infers the type from context. By default Npgsql sends
    /// <c>text</c>, which changes the meaning SILENTLY compared with MySQL: (1) <c>citext_column = 'admin'</c> becomes a case-sensitive comparison
    /// (the <c>text = text</c> operator), defeating <c>citext</c>, which replaces MySQL's <c>_ci</c> collation, e.g. for login names;
    /// (2) <c>numeric_column = '5'</c>, <c>boolean_column = 'true'</c>, and writing a string into a <c>jsonb</c>/<c>timestamp</c> column fail with
    /// "operator does not exist" or "is of type ... but expression is of type text" even though MySQL coerces the type. The "unknown" form matches how
    /// MySQL treats strings; a string that cannot be coerced (<c>'abc'</c> into a numeric column) still fails loudly in PostgreSQL.
    /// <para><b>Known cost</b>: when there is NO type context, PostgreSQL raises <c>could not determine data type of
    /// parameter</c> (42P18) or <c>could not determine polymorphic type because input has type unknown</c> (42804) (tried on PG 18) for <c>? is null</c>, <c>pg_typeof(?)</c>, <c>to_jsonb(?)</c>, <c>jsonb_build_object('a', ?)</c>.
    /// DAOs must cast explicitly (<c>?::text is null</c>, <c>?::jsonb</c>); see <c>README.md</c> in the same directory.</para></item>
    /// <item>Every other type is passed through unchanged: Npgsql already supports <c>sbyte</c> (Java <c>byte</c>), <c>short</c>, <c>int</c>, <c>long</c>,
    /// <c>float</c>, <c>double</c>, <c>decimal</c>, <c>bool</c>, <c>DateTime</c>, <c>byte[]</c>, <c>Guid</c>.</item>
    /// </list>
    /// </summary>
    public static NpgsqlParameter[] ToParameters(object?[] args)
    {
        var parameters = new NpgsqlParameter[args.Length];
        for (var i = 0; i < args.Length; i++)
        {
            parameters[i] = args[i] switch
            {
                null => new NpgsqlParameter { Value = DBNull.Value },
                string text => new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Unknown, Value = text },

                // params object?[] swallows everything, including a CancellationToken passed in the wrong position: fail early with a clear message.
                CancellationToken => throw new ArgumentException(
                    $"SQL parameter #{i + 1} is a CancellationToken: pass the token through the (sql, args, cancellationToken) overload.",
                    nameof(args)),
                var value => new NpgsqlParameter { Value = value },
            };
        }

        return parameters;
    }

    /// <summary>
    /// Value read by Npgsql to the value stored in <see cref="ResultSnapshot"/>.
    /// <list type="bullet">
    /// <item><see cref="DBNull"/> to <c>null</c> (Java <c>getObject</c> returns <c>null</c> for SQL NULL).</item>
    /// <item><c>short</c> (smallint) to <c>int</c>: JDBC (both MySQL and PostgreSQL) returns <c>Integer</c> for TINYINT/SMALLINT/INT, and the Java code
    /// reads those columns with <c>getByte</c>/<c>getShort</c> (which need an <c>int</c>). If <c>short</c> were kept, those ~110 call sites would throw
    /// <see cref="InvalidCastException"/>.</item>
    /// </list>
    /// Other types are passed through unchanged (int, long, float, double, decimal, bool, string, DateTime, ...).
    /// </summary>
    public static object? FromDriverValue(object value) => value switch
    {
        DBNull => null,
        short number => (int)number,
        _ => value,
    };
}
