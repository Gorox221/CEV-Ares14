// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Perks.Effects;

/// <summary>
/// Applies one delta to the target's highest stat and another to all the rest.
/// </summary>
public sealed partial class StatRedistributionPerkEffect : PerkEffect<StatRedistributionPerkEffect>
{
    [DataField]
    public int HighestDelta = -10;

    [DataField]
    public int OtherDelta = 4;
}