// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Stats;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Origins;

[Prototype]
public sealed partial class OriginPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Origin name.
    /// </summary>
    [DataField(required: true)]
    public LocId Name { get; private set; } = string.Empty;

    /// <summary>
    /// Origin description.
    /// </summary>
    [DataField]
    public LocId? Description { get; private set; }

    /// <summary>
    /// Origin stats.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<StatPrototype>, int> Stats { get; private set; } = new();
}
