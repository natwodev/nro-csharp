namespace Nro.Network.Packets.Players;

// Tùy chọn vật phẩm của ME_LOAD_ALL. (Java: ItemOption)
// Nguồn Java: nro/services/Service.java (dòng 860–958 trong bảng I-02; bản SRC hiện có 479–583).
public sealed record CharacterLoadItemOption(int OptionId, int Parameter);
