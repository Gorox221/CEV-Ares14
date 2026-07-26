// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Ares.Stats;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TempStatModifierComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<StatPrototype>, List<TempStatMod>> Modifiers = new();
}

[Serializable, NetSerializable]
public struct TempStatMod
{
    public int Delta;
    public TimeSpan EndTime;
    public string? Source;
}
