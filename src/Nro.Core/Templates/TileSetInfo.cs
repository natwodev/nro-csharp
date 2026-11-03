namespace Nro.Core.Templates;

/// <summary>
/// Port of <c>nro/template/TileSetInfo.java</c>. The <c>TileSetInfoData</c> property has the same name as the class
/// <see cref="Nro.Core.Templates.TileSetInfoData"/> (because Java names the field <c>tileSetInfoData</c> and the rule is to capitalize only
/// the first letter); this is still valid because the type <c>List&lt;TileSetInfoData&gt;</c> differs from the type <c>TileSetInfoData</c>.
/// Java has only one single-parameter constructor (no default constructor).
/// </summary>
public class TileSetInfo
{
    public List<TileSetInfoData> TileSetInfoData { get; set; }

    public TileSetInfo(List<TileSetInfoData> tileSetInfoData)
    {
        TileSetInfoData = tileSetInfoData;
    }
}
