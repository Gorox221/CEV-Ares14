// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Sanity.Behaviors;

/// <summary>
/// Per-check sanity change applied by world entities near the perceiver.
/// Stored on <c>SanityAffectorComponent</c> of the affector entity.
/// </summary>
public sealed partial class AuraSanityChangeBehavior : SanityChangeBehavior<AuraSanityChangeBehavior>
{
    /// <summary>
    /// Sanity change per check tick while in range (negative = lose sanity).
    /// </summary>
    [DataField]
    public float Change = -5f;

    /// <summary>
    /// How far from the affector the effect applies.
    /// </summary>
    [DataField]
    public float Range = 10f;

    [DataField]
    public bool RequiresLineOfSight = true;
}
