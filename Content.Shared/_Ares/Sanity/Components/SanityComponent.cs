// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Ares.Sanity.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SanityComponent : Component
{
    [DataField, AutoNetworkedField]
    public float CurrentSanity = 100f;

    [DataField]
    public float MaxSanity = 100f;

    [DataField]
    public float MinSanity = 0f;

    [DataField]
    public float Range = 8f;

    [DataField]
    public float CheckInterval = 2f;

    [DataField]
    public float Accumulator;
}
