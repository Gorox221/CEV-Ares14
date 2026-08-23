// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Desires;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared.Kitchen;
using Content.Server.Nutrition.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._Ares.Sanity.Systems;

/// <summary>
/// Handles fulfillment of the Eat desire: consuming food from the desired group.
/// </summary>
public sealed partial class EatDesireSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly RestSystem _rest = default!;
    [Dependency] private readonly DesireConditionSystem _desireConditions = default!;

    private readonly Dictionary<string, string> _foodGroupCache = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RestDesireIngestedEvent>(OnRestIngested);
        CacheFoodGroups();
        _prototypes.PrototypesReloaded += OnPrototypesReloaded;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _prototypes.PrototypesReloaded -= OnPrototypesReloaded;
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs obj)
    {
        CacheFoodGroups();
    }

    private void OnRestIngested(ref RestDesireIngestedEvent args)
    {
        if (!_rest.TryGetDesireObjective(args.Player, out var objEnt, out var cond))
            return;

        if (cond.Behavior is not EatDesireBehavior eat || cond.RecipeGroup == null)
            return;

        var foodProto = MetaData(args.Food).EntityPrototype;
        if (foodProto == null || !_foodGroupCache.TryGetValue(foodProto.ID, out var foodGroup))
            return;

        foreach (var group in eat.Groups)
        {
            if (group.Id == cond.RecipeGroup && group.Recipes.Contains(foodGroup))
            {
                _desireConditions.SetCompleted(objEnt, cond);
                return;
            }
        }
    }

    /// <summary>
    /// Maps each food entity prototype produced by a recipe to its recipe group.
    /// Slices of a sliceable dish inherit the group of the dish they were cut from.
    /// </summary>
    private void CacheFoodGroups()
    {
        _foodGroupCache.Clear();
        foreach (var recipe in _prototypes.EnumeratePrototypes<FoodRecipePrototype>())
        {
            _foodGroupCache.TryAdd(recipe.Result, recipe.Group);

            if (_prototypes.TryIndex<EntityPrototype>(recipe.Result, out var foodProto)
                && foodProto.TryGetComponent<SliceableFoodComponent>(out var sliceable)
                && sliceable.Slice != null)
            {
                _foodGroupCache.TryAdd(sliceable.Slice.Value, recipe.Group);
            }
        }
    }
}
