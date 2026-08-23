// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Perks.Effects;

public sealed partial class TerribleFatePerkEffect : PerkEffect<TerribleFatePerkEffect>
{
    /// <summary>
    /// Base chance (at Vigilance 0) of the effect triggering on a witness.
    /// </summary>
    [DataField]
    public float Chance = 1f;
}