// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Perks.Effects;

public sealed partial class UnfinishedDeliveryPerkEffect : PerkEffect<UnfinishedDeliveryPerkEffect>
{
    /// <summary>
    /// Chance of the perk triggering when the target dies.
    /// </summary>
    [DataField]
    public float Chance = 0.33f;

    [DataField]
    public float BruteHeal = 20f;

    [DataField]
    public float BurnHeal = 20f;

    /// <summary>
    /// Amount of asphyxiation damage removed.
    /// </summary>
    [DataField]
    public float OxyHeal = 100f;

    /// <summary>
    /// Minimum sleep duration in ticks.
    /// </summary>
    [DataField]
    public int MinSleepTicks = 20;

    /// <summary>
    /// Maximum sleep duration in ticks.
    /// </summary>
    [DataField]
    public int MaxSleepTicks = 30;
}