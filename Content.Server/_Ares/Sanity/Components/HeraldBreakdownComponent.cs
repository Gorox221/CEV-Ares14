// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Ares.Sanity.Components;

[RegisterComponent]
public sealed partial class HeraldBreakdownComponent : Component
{
    [DataField]
    public TimeSpan EndTime;

    [DataField]
    public float QuoteAccumulator;

    [DataField]
    public float QuoteInterval = 10f;

    [DataField]
    public float ReturnSanity = 45f;
}