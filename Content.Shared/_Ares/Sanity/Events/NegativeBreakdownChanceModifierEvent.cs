// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Sanity.Events;

[ByRefEvent]
public record struct NegativeBreakdownChanceModifierEvent
{
    public float Multiplier = 1f;

    public NegativeBreakdownChanceModifierEvent()
    {
    }
}
