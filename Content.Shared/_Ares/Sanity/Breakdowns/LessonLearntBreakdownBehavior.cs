// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Sanity.Breakdowns;

public sealed partial class LessonLearntBreakdownBehavior : SanityBreakdownBehavior<LessonLearntBreakdownBehavior>
{
    /// <summary>
    /// Minimum random amount added to each stat (inclusive).
    /// </summary>
    [DataField]
    public int StatMinIncrease = 5;

    /// <summary>
    /// Maximum random amount added to each stat (inclusive).
    /// </summary>
    [DataField]
    public int StatMaxIncrease = 10;
}
