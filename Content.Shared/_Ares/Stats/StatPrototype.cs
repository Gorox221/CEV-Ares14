// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Stats;

[Prototype]
public sealed partial class StatPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId Name { get; private set; } = string.Empty;

    [DataField]
    public LocId? Description { get; private set; }

    [DataField]
    public Color Color { get; private set; } = Color.White;

    [DataField]
    public int MinLevel { get; private set; } = 0;

    [DataField]
    public int MaxLevel { get; private set; } = 100;
}
