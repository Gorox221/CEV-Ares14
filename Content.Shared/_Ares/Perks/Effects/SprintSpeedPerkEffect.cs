// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Perks.Effects;

/// <summary>
/// Multiplies the target's sprint speed.
/// </summary>
public sealed partial class SprintSpeedPerkEffect : PerkEffect<SprintSpeedPerkEffect>
{
    [DataField]
    public float SprintSpeedMultiplier = 1.33f;
}
