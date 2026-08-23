// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Ares.Perks;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GrabRangeComponent : Component
{
    [DataField, AutoNetworkedField]
    public float RangeExtension = 1f;
}
