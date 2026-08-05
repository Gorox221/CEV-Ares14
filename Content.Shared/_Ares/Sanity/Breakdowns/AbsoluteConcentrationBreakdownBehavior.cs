// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Sanity.Breakdowns;

/// <summary>
/// Breakdown during which the target instantly regains full sanity and becomes
/// immune to sanity loss until <see cref="Duration"/> elapses.
/// </summary>
public sealed partial class AbsoluteConcentrationBreakdownBehavior : SanityBreakdownBehavior<AbsoluteConcentrationBreakdownBehavior>
{
    [DataField]
    public float Duration;
}
