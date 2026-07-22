// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Stats;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class StatsComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<StatPrototype>, int> Stats = new();
}
