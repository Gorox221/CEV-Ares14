// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Perks.Effects;

/// <summary>
/// Applies the specified damage modifier set to the target while the perk is active.
/// </summary>
public sealed partial class DamageResistancePerkEffect : PerkEffect<DamageResistancePerkEffect>
{
    [DataField]
    public ProtoId<DamageModifierSetPrototype> ModifierSetId = string.Empty;
}