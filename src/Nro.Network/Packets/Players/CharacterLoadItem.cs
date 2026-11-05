namespace Nro.Network.Packets.Players;

// Vật phẩm của ME_LOAD_ALL; TemplateId=-1 biểu thị ô trống. (Java: Service.player)
// Nguồn Java: nro/services/Service.java (dòng 860–958 trong bảng I-02; bản SRC hiện có 479–583).
public sealed record CharacterLoadItem(short TemplateId, int Quantity, string Info, string Content, IReadOnlyList<CharacterLoadItemOption> Options);
