// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Ares.Sanity.Components;

[RegisterComponent]
public sealed partial class SanityAffectorComponent : Component
{
    [DataField]
    public float SanityChange = -5f;

    [DataField]
    public float Range = 10f;

    [DataField]
    public bool RequiresLineOfSight = true;
}
