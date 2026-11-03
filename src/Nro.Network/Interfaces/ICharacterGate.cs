namespace Nro.Network.Interfaces;

/// <summary>
/// Tells the dispatcher whether a session already has a character. Most commands are only valid then: the Java <c>Controller</c> tests
/// <c>player != null</c> and silently drops the packet otherwise. The game layer implements this (the network layer knows nothing about players).
/// </summary>
public interface ICharacterGate
{
    bool HasCharacter(ISession session);
}
