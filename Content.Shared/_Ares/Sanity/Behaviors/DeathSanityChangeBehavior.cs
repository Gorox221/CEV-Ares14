// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Sanity.Behaviors;

/// <summary>
/// Sanity lost when the entity sees a humanoid die nearby.
/// </summary>
public sealed partial class DeathSanityChangeBehavior : SanityChangeBehavior<DeathSanityChangeBehavior>
{
    /// <summary>
    /// Sanity lost per witnessed death.
    /// </summary>
    [DataField]
    public float Change = -10f;

    /// <summary>
    /// How far a death is perceived from the viewer.
    /// </summary>
    [DataField]
    public float Range = 8f;
}
