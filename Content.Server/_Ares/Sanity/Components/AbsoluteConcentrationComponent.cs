// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Ares.Sanity.Components;

/// <summary>
/// Runtime state of an ongoing absolute concentration breakdown, managed by AbsoluteConcentrationBreakdownSystem.
/// </summary>
[RegisterComponent]
public sealed partial class AbsoluteConcentrationComponent : Component
{
    [DataField]
    public TimeSpan EndTime;
}
