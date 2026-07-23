using Robust.Shared.GameStates;

namespace Content.Shared._Ares.LockHack;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class LockHackComponent : Component
{
    [DataField, AutoNetworkedField]
    public int WireCount = 16;

    [DataField, AutoNetworkedField]
    public int RequiredPulses = 8;
}
