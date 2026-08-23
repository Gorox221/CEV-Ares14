// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Perks.Effects;

/// <summary>
/// Multiplies the chance of positive breakdowns for the target.
/// Positive is marked by the <c>positive</c> flag on the breakdown prototype.
/// </summary>
public sealed partial class PositiveBreakdownChancePerkEffect : PerkEffect<PositiveBreakdownChancePerkEffect>
{
    [DataField]
    public float Multiplier = 1f;
}