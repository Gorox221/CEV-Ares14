// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Sanity.Behaviors;

/// <summary>
/// Regenerates sanity over time when the entity has not taken sanity damage recently.
/// </summary>
public sealed partial class RegenSanityChangeBehavior : SanityChangeBehavior<RegenSanityChangeBehavior>
{
    /// <summary>
    /// How long after the last sanity damage regeneration starts.
    /// </summary>
    [DataField]
    public float Delay = 30f;

    /// <summary>
    /// Sanity gained per check tick while regenerating.
    /// </summary>
    [DataField]
    public float Amount = 0.2f;
}
