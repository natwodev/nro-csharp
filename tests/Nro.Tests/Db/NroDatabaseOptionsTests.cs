using Npgsql;
using Nro.Data;

namespace Nro.Tests.Db;

/// <summary>
/// Tests the configuration and the safeguards of the integration-test helper; needs NO database. Proves that integration tests FAIL clearly
/// (never skip silently) when <c>NRO_TEST_DB</c> is missing, when it points at the wrong database, or when the connection cannot be made.
/// </summary>
public class NroDatabaseOptionsTests
{
    [Fact]
    public void Missing_connection_string_cannot_be_constructed()
    {
        Assert.Throws<ArgumentNullException>(() => new NroDatabase(null!));
        Assert.Throws<ArgumentException>(() => new NroDatabase(new NroDatabaseOptions()));
        Assert.Throws<ArgumentException>(() => new NroDatabase(new NroDatabaseOptions { ConnectionString = "   " }));
    }

    [Fact]
    public void Negative_command_timeout_is_invalid()
    {
        var options = new NroDatabaseOptions { ConnectionString = "Host=127.0.0.1;Database=x", CommandTimeoutSeconds = -1 };

        Assert.Throws<ArgumentOutOfRangeException>(() => new NroDatabase(options));
    }

    [Fact]
    public void Option_defaults()
    {
        var options = new NroDatabaseOptions();

        Assert.Equal(string.Empty, options.ConnectionString);   // no hard-coded secret
        Assert.False(options.LogQuery);
        Assert.Equal(30, options.CommandTimeoutSeconds);
    }

    [Fact]
    public async Task Construct_and_dispose_without_a_real_connection()
    {
        // NpgsqlDataSource opens connections lazily: building and closing it does not touch the server
        await using var db = new NroDatabase(new NroDatabaseOptions { ConnectionString = "Host=127.0.0.1;Port=1;Database=x;Username=u" });

        Assert.NotNull(db);
    }

    // ------------------------------------------------------------------ TestDatabase safeguards

    [Fact]
    public void Missing_NRO_TEST_DB_is_a_clear_error_not_a_skip()
    {
        var error = Assert.Throws<InvalidOperationException>(() => TestDatabase.ReadConnectionString(_ => null));

        Assert.Contains(TestDatabase.EnvironmentVariable, error.Message);
        Assert.Throws<InvalidOperationException>(() => TestDatabase.ReadConnectionString(_ => "   "));
    }

    [Fact]
    public void Connection_string_pointing_at_the_wrong_database_is_rejected_and_does_not_leak_the_string()
    {
        const string wrong = "Host=127.0.0.1;Port=55432;Database=nro_dev;Username=nguoi_dung;Password=mat-khau-gia-12345";

        var error = Assert.Throws<InvalidOperationException>(() => TestDatabase.ReadConnectionString(_ => wrong));

        Assert.Contains("nro_dev", error.Message);
        Assert.Contains("nro_test", error.Message);
        Assert.DoesNotContain("mat-khau-gia-12345", error.Message);
        Assert.DoesNotContain("nguoi_dung", error.Message);
    }

    [Fact]
    public void Malformed_connection_string_is_rejected_and_does_not_leak_the_string()
    {
        var error = Assert.Throws<InvalidOperationException>(() => TestDatabase.ReadConnectionString(_ => "khong;phai=chuoi;ket;noi=mat-khau-gia-77"));

        Assert.DoesNotContain("mat-khau-gia-77", error.Message);
    }

    [Fact]
    public void Correct_connection_string_for_nro_test_is_accepted()
    {
        const string good = "Host=127.0.0.1;Port=55432;Database=nro_test;Username=u;Password=p";

        Assert.Equal(good, TestDatabase.ReadConnectionString(_ => good));
    }

    [Fact]
    public async Task Unreachable_database_makes_the_fixture_fail_with_a_clear_message()
    {
        var fixture = new UnreachableFixture();

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.InitializeAsync());

        Assert.Contains("Could not connect", error.Message);
        Assert.Contains("nro_test", error.Message);
        Assert.DoesNotContain("mat-khau-gia-55", error.Message);
        await fixture.DisposeAsync();   // safe even if initialisation failed
    }

    private sealed class UnreachableFixture : TestDatabase
    {
        protected override string ResolveConnectionString()
            => "Host=127.0.0.1;Port=1;Database=nro_test;Username=u;Password=mat-khau-gia-55;Timeout=3;Pooling=false";
    }
}
