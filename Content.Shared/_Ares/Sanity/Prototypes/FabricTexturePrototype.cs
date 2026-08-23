// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Sanity.Prototypes;

[Prototype]
public sealed partial class FabricTexturePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Unrooted RSI path, e.g. <c>_Goobstation/Mobs/Demons/spookyghost/ghost1.rsi</c>.
    /// </summary>
    [DataField(required: true)]
    public string RsiPath = string.Empty;

    /// <summary>
    /// RSI state to show.
    /// </summary>
    [DataField(required: true)]
    public string RsiState = string.Empty;
}