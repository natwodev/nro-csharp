namespace Nro.Game.Effects;

// Biểu diễn bản chụp tọa độ x, y của người chơi hoặc quái tại một thời điểm.
// Nguồn Java: nro/player/Location.java (DTO phụ trợ cho EffectSkin).
// Tọa độ X của bản chụp (Java: x).
// Tọa độ Y của bản chụp (Java: y).
public readonly record struct EffectLocation(int X, int Y);
