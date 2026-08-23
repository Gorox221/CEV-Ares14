// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Ares.Sanity.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class InsightComponent : Component
{
    [DataField, AutoNetworkedField]
    public float CurrentInsight;

    [DataField, AutoNetworkedField]
    public float MaxInsight = 100f;

    [DataField]
    public float LevelChange;
}
