// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared._Ares.Stats;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityEffects.Effects.Solution;
using Content.Shared.Kitchen;
using Content.Shared.Mind;
using Content.Shared.Nutrition;
using Content.Shared.Nutrition.Components;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Systems;
using Content.Shared.Popups;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Ares.Sanity.Systems;

public sealed partial class RestSystem : EntitySystem
{
    [Dependency] private readonly AresStatsSystem _stats = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainers = default!;
    [Dependency] private readonly SharedObjectivesSystem _objectives = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private const float InsightDecayReset = 80f;

    private readonly Dictionary<EntityUid, EntityUid> _desireObjectives = new();
    private readonly Dictionary<string, string> _foodGroupCache = new();

    private static readonly string[] EatGroups =
    [
        "DessertAndPastry",
        "Breakfast",
        "Pasta",
        "PieAndTart",
        "Pizza",
        "Salad",
        "SoupAndStew",
        "Bread",
    ];

    private static readonly Dictionary<string, HashSet<string>> GroupRecipeMapping = new()
    {
        ["DessertAndPastry"] = new() { "Dessert", "Cake", "BarsAndCookies" },
        ["Breakfast"] = new() { "Breakfast" },
        ["Pasta"] = new() { "Pasta" },
        ["PieAndTart"] = new() { "Pie" },
        ["Pizza"] = new() { "Pizza" },
        ["Salad"] = new() { "Salad" },
        ["SoupAndStew"] = new() { "Soup" },
        ["Bread"] = new() { "Breads" },
    };

    public override void Initialize()
    {
        base.Initialize();

        CacheFoodGroups();
        _prototypes.PrototypesReloaded += OnPrototypesReloaded;
        SubscribeLocalEvent<SanityChangedEvent>(OnSanityChanged);
        SubscribeLocalEvent<RestComponent, IngestingEvent>(OnIngested);
        SubscribeNetworkEvent<RestLevelUpRequestEvent>(OnLevelUpRequest);
    }

    public override void Shutdown()
    {
        _prototypes.PrototypesReloaded -= OnPrototypesReloaded;
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs obj)
    {
        CacheFoodGroups();
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<RestComponent>();
        while (query.MoveNext(out var uid, out var rest))
        {
            if (rest.CurrentDesire == "Smoke" && !rest.LevelUpPending)
                CheckSmokeDesire(uid, rest);
        }
    }

    private void CacheFoodGroups()
    {
        _foodGroupCache.Clear();
        foreach (var recipe in _prototypes.EnumeratePrototypes<FoodRecipePrototype>())
        {
            _foodGroupCache.TryAdd(recipe.Result, recipe.Group);
        }
    }

    private void OnSanityChanged(ref SanityChangedEvent args)
    {
        var uid = args.Entity;

        if (!HasComp<InsightComponent>(uid))
            return;

        var rest = EnsureComp<RestComponent>(uid);
        var insight = Comp<InsightComponent>(uid);

        if (rest.CurrentDesire != null || rest.LevelUpPending)
        {
            if (insight.CurrentInsight >= insight.MaxInsight)
            {
                insight.CurrentInsight = InsightDecayReset;
                insight.LevelChange = 0f;
                Dirty(uid, insight);

                RemoveDesireObjective(uid, rest);
                rest.CurrentDesire = null;
                rest.RecipeGroup = null;
                rest.LevelUpPending = false;
                Dirty(uid, rest);

                _popup.PopupEntity(Loc.GetString("rest-insight-faded"), uid, uid);
            }
            return;
        }

        if (insight.CurrentInsight < insight.MaxInsight)
            return;

        insight.CurrentInsight = 0f;
        insight.LevelChange = 0f;
        Dirty(uid, insight);

        var (desire, recipeGroup) = PickDesire();
        rest.CurrentDesire = desire;
        rest.RecipeGroup = recipeGroup;
        rest.RestStartTime = _timing.CurTime;
        Dirty(uid, rest);

        if (_mind.TryGetMind(uid, out var mindId, out var mind))
        {
            var proto = desire switch
            {
                "Eat" => "DesireEatObjective",
                "Drink" => "DesireDrinkObjective",
                "Smoke" => "DesireSmokeObjective",
                _ => null
            };

            if (proto != null)
            {
                var objEnt = _objectives.TryCreateObjective(mindId, mind, proto);
                if (objEnt != null)
                {
                    var meta = MetaData(objEnt.Value);
                    if (TryComp<DesireConditionComponent>(objEnt.Value, out var desireComp))
                    {
                        desireComp.RecipeGroup = recipeGroup;
                        Dirty(objEnt.Value, desireComp);

                        if (recipeGroup != null)
                            _metaData.SetEntityName(objEnt.Value, Loc.GetString("rest-desire-" + desire, ("group", Loc.GetString("rest-desire-group-" + recipeGroup))), meta);
                    }

                    _mind.AddObjective(mindId, mind, objEnt.Value);
                    _desireObjectives[uid] = objEnt.Value;
                }
            }
        }

        _popup.PopupEntity(Loc.GetString("rest-desire-start"), uid, uid);
    }

    private void OnIngested(Entity<RestComponent> ent, ref IngestingEvent args)
    {
        if (ent.Comp.LevelUpPending)
            return;

        if (ent.Comp.CurrentDesire == "Eat")
        {
            if (ent.Comp.RecipeGroup == null)
                return;

            var foodProto = MetaData(args.Food).EntityPrototype?.ID;
            if (foodProto == null)
                return;

            if (!_foodGroupCache.TryGetValue(foodProto, out var foodGroup))
                return;

            if (!GroupRecipeMapping.TryGetValue(ent.Comp.RecipeGroup, out var allowedGroups))
                return;

            if (allowedGroups.Contains(foodGroup))
                CompleteDesire(ent, ent.Comp);

            return;
        }

        if (ent.Comp.CurrentDesire == "Drink" && ContainsAlcohol(args.Split))
            CompleteDesire(ent, ent.Comp);
    }

    private void CheckSmokeDesire(EntityUid uid, RestComponent rest)
    {
        if (!TryComp<BloodstreamComponent>(uid, out var bloodstream))
            return;

        if (!_solutionContainers.ResolveSolution(uid, bloodstream.BloodSolutionName, ref bloodstream.BloodSolution, out var solution))
            return;

        foreach (var reagent in solution.Contents)
        {
            if (reagent.Reagent.Prototype == "Nicotine")
            {
                CompleteDesire((uid, rest), rest);
                return;
            }
        }
    }

    private void CompleteDesire(Entity<RestComponent> ent, RestComponent rest)
    {
        RemoveDesireObjective(ent, rest);
        rest.CurrentDesire = null;
        rest.LevelUpPending = true;
        Dirty(ent, ent.Comp);

        _popup.PopupEntity(Loc.GetString("rest-desire-complete"), ent, ent);
        _popup.PopupEntity(Loc.GetString("rest-levelup-available"), ent, ent);
    }

    private void RemoveDesireObjective(EntityUid uid, RestComponent rest)
    {
        if (!_desireObjectives.TryGetValue(uid, out var objEnt))
            return;

        if (TryComp<DesireConditionComponent>(objEnt, out var desireCond))
        {
            desireCond.Completed = true;
            Dirty(objEnt, desireCond);
        }

        if (_mind.TryGetMind(uid, out var mindId, out var mind))
        {
            var index = mind.Objectives.IndexOf(objEnt);
            if (index >= 0)
                _mind.TryRemoveObjective(mindId, mind, index);
        }

        _desireObjectives.Remove(uid);
    }

    private void OnLevelUpRequest(RestLevelUpRequestEvent args, EntitySessionEventArgs session)
    {
        var uid = GetEntity(args.Player);

        if (!TryComp<RestComponent>(uid, out var rest) || !rest.LevelUpPending)
            return;

        rest.LevelUpPending = false;
        Dirty(uid, rest);

        if (args.Choice == "Internalize")
            DistributeStats(uid);

        _popup.PopupEntity(Loc.GetString("rest-levelup-complete"), uid, uid);
    }

    private void DistributeStats(EntityUid uid)
    {
        var totalPoints = 45;
        var statIds = _prototypes.EnumeratePrototypes<StatPrototype>()
            .Select(s => new ProtoId<StatPrototype>(s.ID))
            .ToList();

        var points = new int[statIds.Count];
        var remaining = totalPoints;

        for (var i = 0; i < points.Length; i++)
        {
            points[i] = _random.Next(0, remaining + 1);
            remaining -= points[i];
        }

        for (var i = 0; i < statIds.Count; i++)
            _stats.ModifyStatLevel(uid, statIds[i], points[i]);
    }

    private bool ContainsAlcohol(Solution solution)
    {
        foreach (var reagent in solution.Contents)
        {
            if (IsAlcoholicReagent(reagent.Reagent.Prototype))
                return true;
        }

        return false;
    }

    private bool IsAlcoholicReagent(string reagentProtoId)
    {
        var reagentProto = _prototypes.Index<ReagentPrototype>(reagentProtoId);

        // Check if any metabolism group produces Ethanol
        if (reagentProto.Metabolisms != null)
        {
            foreach (var entry in reagentProto.Metabolisms.Values)
            {
                foreach (var effect in entry.Effects)
                {
                    if (effect is AdjustReagent adj && adj.Reagent == "Ethanol")
                        return true;
                }
            }
        }

        return false;
    }

    private bool HasAlcohol(EntityUid food)
    {
        if (!TryComp<SolutionComponent>(food, out var sol))
            return false;

        return ContainsAlcohol(sol.Solution);
    }

    private (string desire, string? recipeGroup) PickDesire()
    {
        var desire = new[] { "Eat", "Drink", "Smoke" }[_random.Next(3)];
        var group = desire == "Eat" ? EatGroups[_random.Next(EatGroups.Length)] : null;
        return (desire, group);
    }
}
