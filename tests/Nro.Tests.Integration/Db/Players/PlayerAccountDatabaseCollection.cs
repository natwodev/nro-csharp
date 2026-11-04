namespace Nro.Tests.Integration.Db.Players;

/// <summary>
/// Serializes tests that clear the shared <c>player</c> and <c>account</c> tables.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class PlayerAccountDatabaseCollection
{
    public const string Name = "player/account database";
}
