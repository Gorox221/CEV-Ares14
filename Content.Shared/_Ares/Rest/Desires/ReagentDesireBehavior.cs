// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Sanity.Desires;

/// <summary>
/// Base for desires fulfilled by the presence of reagents in a solution.
/// </summary>
public abstract partial class ReagentDesireBehavior : DesireBehavior
{
    [DataField]
    public List<ProtoId<ReagentPrototype>> Reagents = new();

    public bool ContainsReagent(Solution solution)
        => ContainsReagent(solution, Reagents);

    /// <summary>
    /// True if the solution contains any of the given reagents.
    /// </summary>
    public static bool ContainsReagent(Solution solution, ICollection<ProtoId<ReagentPrototype>> reagents)
    {
        foreach (var content in solution.Contents)
        {
            if (reagents.Contains(content.Reagent.Prototype))
                return true;
        }

        return false;
    }
}
