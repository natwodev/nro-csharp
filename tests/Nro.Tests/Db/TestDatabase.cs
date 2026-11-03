using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Npgsql;
using Nro.Data;

namespace Nro.Tests.Db;

/// <summary>
/// Helper for integration tests on a REAL PostgreSQL (xUnit <c>IClassFixture</c>): every test CLASS gets a random schema
/// <c>t_&lt;guid&gt;</c> in the <c>nro_test</c> database, dropped when done.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>The connection string is read from the environment variable <c>NRO_TEST_DB</c> (the checker loads it when running <c>test</c>). It is NEVER printed.</item>
/// <item>FAILS clearly, never skips silently, if the variable is missing, the connection fails, or the database is not <c>nro_test</c>.</item>
/// <item>Never touches <c>nro_dev</c>; creates no object outside the test class's schema (the <c>citext</c> extension is available in <c>public</c>,
/// so <c>public</c> comes later in the <c>search_path</c>, but the test's tables always live in the dedicated schema).</item>
/// <item>Also offers a RAW Npgsql data source, independent of the code under test, to verify real effects in the database.</item>
/// </list>
/// </remarks>
public class TestDatabase : IAsyncLifetime
{
    public const string EnvironmentVariable = "NRO_TEST_DB";
    public const string RequiredDatabase = "nro_test";

    private static readonly Regex SchemaPattern = new("^t_[0-9a-f]{32}$", RegexOptions.CultureInvariant);

    private string _connectionString = string.Empty;
    private NpgsqlDataSource? _admin;
    private NpgsqlDataSource? _raw;
    private bool _schemaCreated;

    /// <summary>Random schema name of this test class.</summary>
    public string Schema { get; } = "t_" + Guid.NewGuid().ToString("N");

    /// <summary>Logger that writes to the memory of <see cref="Db"/>.</summary>
    public ListLogger Logger { get; } = new();

    /// <summary>The database under test, pointing at the test class's schema.</summary>
    public NroDatabase Db { get; private set; } = null!;

    /// <summary>
    /// Reads and validates the connection string from the environment variable (parameterised so it can be tested without touching the real environment).
    /// The error message names the variable and the database but does NOT contain the connection string.
    /// </summary>
    public static string ReadConnectionString(Func<string, string?> getEnvironmentVariable)
    {
        var value = getEnvironmentVariable(EnvironmentVariable);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Missing environment variable {EnvironmentVariable} (connection string of database {RequiredDatabase}). Integration tests are NOT skipped: "
                + "run through 'python tools/check_port.py test' (which loads this variable) or load the variable from postgres/.secrets/connection.nro_test.txt without printing it.");
        }

        NpgsqlConnectionStringBuilder builder;
        try
        {
            builder = new NpgsqlConnectionStringBuilder(value);
        }
        catch (ArgumentException)
        {
            throw new InvalidOperationException($"{EnvironmentVariable} is not a valid Npgsql connection string.");
        }

        if (!string.Equals(builder.Database, RequiredDatabase, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"{EnvironmentVariable} must point to the database '{RequiredDatabase}' (currently '{builder.Database}'): integration tests may only run against {RequiredDatabase}.");
        }

        return value;
    }

    public async Task InitializeAsync()
    {
        _connectionString = ResolveConnectionString();
        Assert.Matches(SchemaPattern, Schema);

        var admin = new NpgsqlConnectionStringBuilder(_connectionString) { MaxPoolSize = 2 };
        _admin = new NpgsqlDataSourceBuilder(admin.ConnectionString).Build();

        try
        {
            await using var connection = await _admin.OpenConnectionAsync();
            await using var command = new NpgsqlCommand("select current_database()", connection);
            var current = (string?)await command.ExecuteScalarAsync();
            if (!string.Equals(current, RequiredDatabase, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Connected to database '{current}', not '{RequiredDatabase}': stopping the integration tests.");
            }
        }
        catch (NpgsqlException error)
        {
            throw new InvalidOperationException(
                $"Could not connect to database {RequiredDatabase} ({error.GetType().Name}). Is the nro-postgres container running? "
                + "Check with: python tools/check_port.py postgres");
        }

        var create = "create schema " + QuotedSchema;
        await RunAdminAsync(create);
        _schemaCreated = true;

        var raw = new NpgsqlConnectionStringBuilder(_connectionString) { SearchPath = Schema + ",public", MaxPoolSize = 3 };
        _raw = new NpgsqlDataSourceBuilder(raw.ConnectionString).Build();

        Db = CreateDatabase(logger: Logger);
        await SeedAsync();
    }

    public async Task DisposeAsync()
    {
        if (Db is not null)
        {
            await Db.DisposeAsync();
        }

        if (_raw is not null)
        {
            await _raw.DisposeAsync();
        }

        if (_admin is not null)
        {
            try
            {
                if (_schemaCreated)
                {
                    var drop = "drop schema if exists " + QuotedSchema + " cascade";
                    await RunAdminAsync(drop);
                }
            }
            finally
            {
                await _admin.DisposeAsync();
            }
        }
    }

    /// <summary>Source of the connection string. Defaults to the environment variable; a subclass overrides it to exercise failure paths (e.g. a port nobody listens on).</summary>
    protected virtual string ResolveConnectionString() => ReadConnectionString(Environment.GetEnvironmentVariable);

    /// <summary>Creates the initial tables/data of the test class (runs in its own schema, through the raw connection).</summary>
    protected virtual Task SeedAsync() => Task.CompletedTask;

    /// <summary>
    /// Builds one more <see cref="NroDatabase"/> pointing at the test class's schema, with customised options or connection string
    /// (e.g. <c>Maximum Pool Size=1</c>). The caller must <c>await using</c> it.
    /// </summary>
    public NroDatabase CreateDatabase(
        Action<NroDatabaseOptions>? configure = null,
        Action<NpgsqlConnectionStringBuilder>? configureConnection = null,
        ILogger<NroDatabase>? logger = null)
    {
        var builder = new NpgsqlConnectionStringBuilder(_connectionString) { SearchPath = Schema + ",public", MaxPoolSize = 25 };
        configureConnection?.Invoke(builder);
        var options = new NroDatabaseOptions { ConnectionString = builder.ConnectionString };
        configure?.Invoke(options);
        return new NroDatabase(options, logger ?? new ListLogger());
    }

    /// <summary>A separate RAW Npgsql data source (customised connection string) pointing at the test class's schema. The caller must <c>await using</c> it.</summary>
    public NpgsqlDataSource CreateRawDataSource(Action<NpgsqlConnectionStringBuilder> configureConnection)
    {
        var builder = new NpgsqlConnectionStringBuilder(_connectionString) { SearchPath = Schema + ",public" };
        configureConnection(builder);
        return new NpgsqlDataSourceBuilder(builder.ConnectionString).Build();
    }

    /// <summary>Runs a raw DDL/DML statement (no parameters) in the test class's schema, WITHOUT going through the code under test.</summary>
    public async Task RunAsync(string sql)
    {
        await using var connection = await _raw!.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>Runs a raw query that returns one value, WITHOUT going through the code under test, to verify real effects in the database.</summary>
    public async Task<T> ScalarAsync<T>(string sql, params object[] parameters)
    {
        await using var connection = await _raw!.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        foreach (var parameter in parameters)
        {
            command.Parameters.Add(new NpgsqlParameter { Value = parameter });
        }

        var result = await command.ExecuteScalarAsync();
        return (T)Convert.ChangeType(result!, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>Lists the <c>t_*</c> schemas still existing in nro_test (used to prove cleanup).</summary>
    public async Task<IReadOnlyList<string>> ListTestSchemasAsync()
    {
        var names = new List<string>();
        await using var connection = await _admin!.OpenConnectionAsync();
        await using var command = new NpgsqlCommand("select nspname from pg_namespace where nspname ~ '^t_[0-9a-f]{32}$' order by 1", connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            names.Add(reader.GetString(0));
        }

        return names;
    }

    private string QuotedSchema
    {
        get
        {
            // DDL cannot parameterise a schema name; the name is generated by this code itself (t_ + 32 hex digits) and checked with a regex before use.
            Assert.Matches(SchemaPattern, Schema);
            return "\"" + Schema + "\"";
        }
    }

    private async Task RunAdminAsync(string sql)
    {
        await using var connection = await _admin!.OpenConnectionAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}
