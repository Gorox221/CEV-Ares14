// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Sanity.Breakdowns;

public sealed partial class HeraldBreakdownBehavior : SanityBreakdownBehavior<HeraldBreakdownBehavior>
{
    /// <summary>
    /// Length of the breakdown in seconds. Sanity is restored to <see cref="SanityReturn"/>(base) on end.
    /// </summary>
    [DataField]
    public float Duration;

    /// <summary>
    /// Seconds between insane quotes the target speaks aloud.
    /// </summary>
    [DataField]
    public float QuoteInterval = 10f;
}