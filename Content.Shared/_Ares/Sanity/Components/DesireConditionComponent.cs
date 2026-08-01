// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Ares.Sanity.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DesireConditionComponent : Component
{
    [DataField]
    public string? DesireType;

    [DataField, AutoNetworkedField]
    public string? RecipeGroup;

    [DataField, AutoNetworkedField]
    public bool Completed;
}
