// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Ares.Stats;

[RegisterComponent, NetworkedComponent]
public sealed partial class CritThresholdModifierComponent : Component
{
    [DataField]
    public float BaseThreshold = 100f;
}