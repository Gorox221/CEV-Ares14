// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Sanity.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TheFabricBreakdownComponent : Component
{
    [DataField]
    public TimeSpan EndTime;

    [DataField]
    public float Accumulator;

    [DataField]
    public float UpdateInterval = 1f;

    [DataField]
    public float Range = 12f;

    /// <summary>
    /// Humanoids whose appearance is replaced with one of <see cref="Textures"/>.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<NetEntity> Targets = new();

    /// <summary>
    /// Candidate texture prototypes a hallucinated target may be replaced with. One is picked at
    /// random per target on the client, so adding entries here just works - no server-side hardcoding.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<ProtoId<FabricTexturePrototype>> Textures = new();
}
