namespace Nro.Core.Templates;

/// <summary>
/// Port of <c>nro/template/TileSetInfoData.java</c>. Java has only one two-parameter constructor (no default constructor)
/// and keeps the <c>data</c> array reference passed in as is (no copy).
/// </summary>
public class TileSetInfoData
{
    public int Type { get; set; }

    public int[] Data { get; set; }

    public TileSetInfoData(int type, int[] data)
    {
        Type = type;
        Data = data;
    }
}
