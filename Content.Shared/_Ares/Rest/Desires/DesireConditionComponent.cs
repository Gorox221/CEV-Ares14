// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Desires;
using Robust.Shared.GameStates;

namespace Content.Shared._Ares.Sanity.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DesireConditionComponent : Component
{
    [DataField]
    public string? DesireType;

    /// <summary>
    /// Fulfillment logic of the desire. Checked by the desire's own system
    /// to detect when the player has fulfilled it.
    /// </summary>
    [DataField]
    public DesireBehavior? Behavior;

    [DataField, AutoNetworkedField]
    public string? RecipeGroup;

    [DataField, AutoNetworkedField]
    public bool Completed;
}
