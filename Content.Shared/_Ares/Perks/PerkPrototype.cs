// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Perks.Effects;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Perks;

[Prototype]
public sealed partial class PerkPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Perk name.
    /// </summary>
    [DataField(required: true)]
    public LocId Name { get; private set; } = string.Empty;

    /// <summary>
    /// Perk description.
    /// </summary>
    [DataField]
    public LocId? Description { get; private set; }

    /// <summary>
    /// Whether the perk can be picked in the character editor at round start.
    /// Perks with this set to false are only obtainable by other means.
    /// </summary>
    [DataField]
    public bool RoundStartSelectable { get; private set; } = false;

    /// <summary>
    /// Effects applied or queried while the perk is active on an entity.
    /// </summary>
    [DataField]
    public List<PerkEffect> Effects { get; private set; } = new();
}