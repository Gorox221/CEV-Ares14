// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Perks.Effects;

public sealed partial class MoralistPerkEffect : PerkEffect<MoralistPerkEffect>
{
    /// <summary>
    /// Sanity at or above which a nearby person counts as sane.
    /// </summary>
    [DataField]
    public float SaneSanityThreshold = 60f;

    /// <summary>
    /// Insight and regeneration multiplier added per sane person nearby.
    /// </summary>
    [DataField]
    public float MultiplierPerSane = 0.02f;

    /// <summary>
    /// Sanity below which a nearby person counts as sick.
    /// </summary>
    [DataField]
    public float SickSanityThreshold = 30f;

    /// <summary>
    /// Health percentage below which a nearby person counts as sick.
    /// </summary>
    [DataField]
    public float SickHealthPercent = 50f;

    /// <summary>
    /// Base sanity drain per sick person per sanity check tick.
    /// </summary>
    [DataField]
    public float DamagePerSick = 0.1f;

    /// <summary>
    /// Overall drain multiplier applied before the vigilance multiplier.
    /// </summary>
    [DataField]
    public float DamageMultiplier = 1.2f;

    /// <summary>
    /// Whether people behind walls still count as sane or sick.
    /// </summary>
    [DataField]
    public bool RequiresLineOfSight = true;
}