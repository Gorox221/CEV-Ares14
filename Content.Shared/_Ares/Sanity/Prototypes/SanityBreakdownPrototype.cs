// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Breakdowns;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Sanity.Prototypes;

[Prototype]
public sealed partial class SanityBreakdownPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId Popup { get; private set; } = string.Empty;

    [DataField]
    public float Weight { get; private set; } = 100f;

    /// <summary>
    /// Behavior of the breakdown, dispatched to its own system when triggered.
    /// </summary>
    [DataField(required: true)]
    public SanityBreakdownBehavior Behavior { get; private set; } = default!;
}
