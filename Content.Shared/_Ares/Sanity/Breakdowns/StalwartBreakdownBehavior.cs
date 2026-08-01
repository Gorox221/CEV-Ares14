// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;

namespace Content.Shared._Ares.Sanity.Breakdowns;

/// <summary>
/// Breakdown that heals the target and immediately restores
/// <see cref="SanityBreakdownBehavior.SanityReturn"/> sanity.
/// </summary>
public sealed partial class StalwartBreakdownBehavior : SanityBreakdownBehavior<StalwartBreakdownBehavior>
{
    [DataField]
    public DamageSpecifier? Healing;
}
