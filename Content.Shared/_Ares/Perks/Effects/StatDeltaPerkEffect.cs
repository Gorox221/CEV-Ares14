// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Stats;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Perks.Effects;

/// <summary>
/// Adds deltas to stats on the target when the perk is applied.
/// <c>deltas</c> addresses specific stats, <c>all</c> applies to every stat.
/// </summary>
public sealed partial class StatDeltaPerkEffect : PerkEffect<StatDeltaPerkEffect>
{
    [DataField]
    public Dictionary<ProtoId<StatPrototype>, int> Deltas = new();

    [DataField]
    public int AllDelta;
}