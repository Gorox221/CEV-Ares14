// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Sanity.Events;

/// <summary>
/// Raised on the target before insight gain is applied; systems may multiply
/// the gained amount.
/// </summary>
[ByRefEvent]
public record struct InsightGainModifierEvent
{
    public float Multiplier = 1f;

    public InsightGainModifierEvent()
    {
    }
}