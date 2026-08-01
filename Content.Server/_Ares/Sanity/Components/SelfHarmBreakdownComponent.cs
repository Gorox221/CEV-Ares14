// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;

namespace Content.Server._Ares.Sanity.Components;

/// <summary>
/// Runtime state of an ongoing self-harm breakdown, managed by SelfHarmBreakdownSystem.
/// </summary>
[RegisterComponent]
public sealed partial class SelfHarmBreakdownComponent : Component
{
    [DataField]
    public TimeSpan EndTime;

    [DataField]
    public float Accumulator;

    [DataField]
    public float Interval = 2.5f;

    [DataField]
    public DamageSpecifier? Damage;

    [DataField]
    public float ReturnSanity = 25f;
}
