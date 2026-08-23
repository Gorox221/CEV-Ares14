// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Stats;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.ToolQuality;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ToolActionFailComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<StatPrototype> Stat = "Cognition";

    [DataField, AutoNetworkedField]
    public int BaseFailChance = 25;
}
