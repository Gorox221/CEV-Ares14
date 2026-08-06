// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Sanity.Breakdowns;

public sealed partial class DownwardSpiralBreakdownBehavior : SanityBreakdownBehavior<DownwardSpiralBreakdownBehavior>
{
    /// <summary>
    /// Permanent reduction applied to the target's maximum sanity. The maximum can never
    /// go below zero (the mind is fully broken).
    /// </summary>
    [DataField]
    public float MaxSanityPenalty = 20f;
}
