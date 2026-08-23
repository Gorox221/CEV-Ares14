// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Perks.Effects;

public sealed partial class SanityAuraPerkEffect : PerkEffect<SanityAuraPerkEffect>
{
    /// <summary>
    /// Sanity change applied to nearby entities per check tick (negative = drain).
    /// </summary>
    [DataField]
    public float Change = 2f;

    /// <summary>
    /// How far from the perk holder the effect applies.
    /// </summary>
    [DataField]
    public float Range = 8f;

    /// <summary>
    /// Whether people behind walls still receive the effect.
    /// </summary>
    [DataField]
    public bool RequiresLineOfSight = true;

    /// <summary>
    /// Only apply while the perk holder is alive.
    /// </summary>
    [DataField]
    public bool RequiresAlive = true;
}
