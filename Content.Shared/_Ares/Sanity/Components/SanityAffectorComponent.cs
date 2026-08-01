// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Behaviors;

namespace Content.Shared._Ares.Sanity.Components;

/// <summary>
/// World entity that lowers or raises sanity of perceivers in range.
/// The effect parameters are data-driven via the behavior.
/// </summary>
[RegisterComponent]
public sealed partial class SanityAffectorComponent : Component
{
    [DataField(required: true)]
    public AuraSanityChangeBehavior Behavior = default!;
}
