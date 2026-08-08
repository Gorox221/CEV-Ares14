// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Perks.Effects;

namespace Content.Shared._Ares.Perks.Events;

/// <summary>
/// Raised on the target when a perk effect is applied, carrying the typed effect
/// so the effect's own system can handle it.
/// </summary>
[ByRefEvent]
public readonly record struct PerkEffectAppliedEvent<T>(T Effect) where T : PerkEffect<T>;