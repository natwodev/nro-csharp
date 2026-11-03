namespace Nro.Core.Templates;

/// <summary>
/// Port of <c>nro/template/WaitSuperRank.java</c> (a pair of players waiting for a Super Rank match). Java has only one
/// two-parameter constructor (no default constructor).
/// </summary>
public class WaitSuperRank
{
    public long PlayerId { get; set; }

    public long RivalId { get; set; }

    public WaitSuperRank(long playerId, long rivalId)
    {
        PlayerId = playerId;
        RivalId = rivalId;
    }
}
