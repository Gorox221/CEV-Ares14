// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Perks.Effects;

public sealed partial class SurvivorPerkEffect : PerkEffect<SurvivorPerkEffect>
{
    [DataField]
    public float Multiplier = 0.5f;
}