// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Ares.ToolQuality;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ToolQualityComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Quality = 25;
}
