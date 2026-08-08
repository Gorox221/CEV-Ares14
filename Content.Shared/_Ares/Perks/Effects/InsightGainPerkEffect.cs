// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Perks.Effects;

/// <summary>
/// Multiplies insight gained by the target.
/// </summary>
public sealed partial class InsightGainPerkEffect : PerkEffect<InsightGainPerkEffect>
{
    [DataField]
    public float Multiplier = 1.5f;
}