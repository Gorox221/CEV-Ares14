// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Perks.Effects;

/// <summary>
/// Changes maximum sanity on the target when the perk is applied.
/// </summary>
public sealed partial class MaxSanityPerkEffect : PerkEffect<MaxSanityPerkEffect>
{
    [DataField]
    public int Delta = -20;
}