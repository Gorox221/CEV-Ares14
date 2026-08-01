// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Sanity.Prototypes;

[Prototype]
public sealed partial class DesirePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Objective entity prototype granted while the desire is active.
    /// The objective's <c>DesireCondition</c> carries the behavior data.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Objective { get; private set; } = default!;

    [DataField]
    public float Weight { get; private set; } = 1f;
}
