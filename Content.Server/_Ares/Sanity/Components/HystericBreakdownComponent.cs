// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Ares.Sanity.Components;

[RegisterComponent]
public sealed partial class HystericBreakdownComponent : Component
{
    [DataField]
    public TimeSpan EndTime;

    [DataField]
    public float StaminaAccumulator;

    [DataField]
    public float StaminaInterval = 2f;

    [DataField]
    public float StaminaDamage = 15f;

    [DataField]
    public float ReturnSanity = 50f;
}
