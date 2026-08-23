// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Perks.Effects;

public sealed partial class NegativeBreakdownChancePerkEffect : PerkEffect<NegativeBreakdownChancePerkEffect>
{
    [DataField]
    public float Multiplier = 1f;
}
