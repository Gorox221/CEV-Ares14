// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Sanity.Behaviors;

/// <summary>
/// Sanity change per check tick while standing in a blood or vomit puddle.
/// </summary>
public sealed partial class PuddleSanityChangeBehavior : SanityChangeBehavior<PuddleSanityChangeBehavior>
{
    /// <summary>
    /// Sanity change per check tick while in range (negative = lose sanity).
    /// </summary>
    [DataField]
    public float Change = -0.5f;

    /// <summary>
    /// How far the puddle is checked from the perceiver.
    /// </summary>
    [DataField]
    public float Range = 7f;

    /// <summary>
    /// Fraction of blood or vomit in the puddle solution required to count.
    /// </summary>
    [DataField]
    public float MinFraction = 0.4f;
}
