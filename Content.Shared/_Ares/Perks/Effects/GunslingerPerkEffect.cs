// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Perks.Effects;

/// <summary>
/// Multiplies the fire rate of guns held and fired one-handed
/// </summary>
public sealed partial class GunslingerPerkEffect : PerkEffect<GunslingerPerkEffect>
{
    [DataField]
    public float FireRateMultiplier = 1.33f;
}