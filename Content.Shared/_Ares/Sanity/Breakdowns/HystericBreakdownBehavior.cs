// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Sanity.Breakdowns;

public sealed partial class HystericBreakdownBehavior : SanityBreakdownBehavior<HystericBreakdownBehavior>
{
    [DataField]
    public float Duration;

    /// <summary>
    /// Length of the knockdown applied on start, in game ticks.
    /// </summary>
    [DataField]
    public int KnockdownTicks = 4;

    /// <summary>
    /// Seconds between stamina damage ticks.
    /// </summary>
    [DataField]
    public float StaminaInterval = 2f;

    /// <summary>
    /// Stamina damage dealt every seconds.
    /// </summary>
    [DataField]
    public float StaminaDamage = 15f;
}
