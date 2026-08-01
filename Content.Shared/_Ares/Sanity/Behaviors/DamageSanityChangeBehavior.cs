// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Sanity.Behaviors;

/// <summary>
/// Sanity lost when the entity takes damage.
/// </summary>
public sealed partial class DamageSanityChangeBehavior : SanityChangeBehavior<DamageSanityChangeBehavior>
{
    /// <summary>
    /// Sanity lost per unit of damage taken.
    /// </summary>
    [DataField]
    public float ChangePerDamage = 0.12f;
}
