// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Sanity.Breakdowns;

public sealed partial class TheFabricBreakdownBehavior : SanityBreakdownBehavior<TheFabricBreakdownBehavior>
{
    /// <summary>
    /// Length of the breakdown in seconds. Sanity is not restored when it ends.
    /// </summary>
    [DataField]
    public float Duration;

    /// <summary>
    /// Radius (in meters) around the affected person whose humanoids appear as ghosts.
    /// </summary>
    [DataField]
    public float Range = 12f;

    /// <summary>
    /// Seconds between refreshes of the hallucinated entity list.
    /// </summary>
    [DataField]
    public float UpdateInterval = 1f;

    /// <summary>
    /// IDs of <see cref="FabricTexturePrototype"/> nearby humanoids may randomly be replaced
    /// with. A single ID is picked at random per target, so adding entries here just works.
    /// </summary>
    [DataField]
    public List<ProtoId<FabricTexturePrototype>> Textures = new();
}