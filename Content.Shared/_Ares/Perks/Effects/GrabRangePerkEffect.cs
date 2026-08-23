// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Perks.Effects;

/// <summary>
/// Increases grab/pull interaction range for the perk holder.
/// </summary>
public sealed partial class GrabRangePerkEffect : PerkEffect<GrabRangePerkEffect>
{
    [DataField]
    public float RangeExtension = 1f;
}
