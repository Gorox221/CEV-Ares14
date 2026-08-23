// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;

namespace Content.Shared._Ares.Sanity.Breakdowns;

/// <summary>
/// Breakdown during which the target periodically attacks itself until
/// <see cref="Duration"/> elapses, then regains <see cref="SanityBreakdownBehavior.SanityReturn"/> sanity.
/// </summary>
public sealed partial class SelfHarmBreakdownBehavior : SanityBreakdownBehavior<SelfHarmBreakdownBehavior>
{
    [DataField]
    public float Duration;

    [DataField]
    public float Interval = 2.5f;

    [DataField]
    public DamageSpecifier? Damage;
}
