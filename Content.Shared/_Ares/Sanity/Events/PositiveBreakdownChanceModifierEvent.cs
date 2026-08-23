// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Sanity.Events;

/// <summary>
/// Raised on the target while a breakdown is being picked; systems may multiply
/// the weight of positive breakdowns for the roll.
/// </summary>
[ByRefEvent]
public record struct PositiveBreakdownChanceModifierEvent
{
    public float PositiveMultiplier = 1f;

    public PositiveBreakdownChanceModifierEvent()
    {
    }
}