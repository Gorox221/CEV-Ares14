// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Desires;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityEffects.Effects.Solution;
using Robust.Shared.Prototypes;

namespace Content.Server._Ares.Sanity.Systems;

/// <summary>
/// Handles fulfillment of the Drink desire: consuming something alcoholic.
/// </summary>
public sealed partial class DrinkDesireSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly RestSystem _rest = default!;
    [Dependency] private readonly DesireConditionSystem _desireConditions = default!;

    private readonly HashSet<ProtoId<ReagentPrototype>> _alcoholicReagents = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RestDesireIngestedEvent>(OnRestIngested);
        CacheAlcoholicReagents();
        _prototypes.PrototypesReloaded += OnPrototypesReloaded;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _prototypes.PrototypesReloaded -= OnPrototypesReloaded;
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs obj)
    {
        CacheAlcoholicReagents();
    }

    private void OnRestIngested(ref RestDesireIngestedEvent args)
    {
        if (!_rest.TryGetDesireObjective(args.Player, out var objEnt, out var cond))
            return;

        if (cond.Behavior is not DrinkDesireBehavior)
            return;

        if (ReagentDesireBehavior.ContainsReagent(args.Split, _alcoholicReagents))
            _desireConditions.SetCompleted(objEnt, cond);
    }

    private void CacheAlcoholicReagents()
    {
        _alcoholicReagents.Clear();

        foreach (var reagent in _prototypes.EnumeratePrototypes<ReagentPrototype>())
        {
            if (reagent.Metabolisms == null)
                continue;

            var alcoholic = false;
            foreach (var entry in reagent.Metabolisms.Values)
            {
                foreach (var effect in entry.Effects)
                {
                    if (effect is AdjustReagent adj && adj.Reagent == "Ethanol")
                    {
                        alcoholic = true;
                        break;
                    }
                }

                if (alcoholic)
                    break;
            }

            if (alcoholic)
                _alcoholicReagents.Add(new ProtoId<ReagentPrototype>(reagent.ID));
        }
    }
}
