// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Sanity.Desires;

/// <summary>
/// Desire fulfilled by eating food from one of the desired groups.
/// </summary>
public sealed partial class EatDesireBehavior : DesireBehavior
{
    [DataField(required: true)]
    public List<DesireFoodGroup> Groups = new();
}
