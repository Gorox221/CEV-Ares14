// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Perks;

/// <summary>
/// Tracks perk prototypes currently active on an entity.
/// A single perk is picked in the character editor, but more can be
/// acquired during the round, hence the list.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PerksComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<ProtoId<PerkPrototype>> Perks = new();
}