// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Stats;
using Content.Shared.Construction.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.Construction;

public sealed partial class ConstructionSystem
{
    private static readonly string[] TargetCategories =
    [
        "construction-category-weapons",
        "construction-category-tools",
        "construction-category-clothing",
        "construction-category-misc"
    ];

    private Dictionary<string, string> _graphCategoryCache = new();

    private string? GetCategoryForGraph(string graphId)
    {
        if (_graphCategoryCache.TryGetValue(graphId, out var cached))
            return cached;

        foreach (var proto in PrototypeManager.EnumeratePrototypes<ConstructionPrototype>())
        {
            if (proto.Graph == graphId)
            {
                _graphCategoryCache[graphId] = proto.Category;
                return proto.Category;
            }
        }

        _graphCategoryCache[graphId] = null!;
        return null;
    }

    private bool IsCraftingCategory(string? category)
    {
        if (category == null)
            return false;

        foreach (var target in TargetCategories)
        {
            if (category == target)
                return true;
        }

        return false;
    }

    private float GetCognitionCraftTime(EntityUid user, float baseTime)
    {
        if (baseTime <= 0 || !HasComp<StatsComponent>(user))
            return baseTime;

        var cogLevel = _statsManager.GetStatLevel(user, new ProtoId<StatPrototype>("Cognition"));
        if (cogLevel <= 0)
            return baseTime;

        var masteryFactor = Math.Min(cogLevel / 60f, 1f) * 0.66f;
        var timeReduction = Math.Max(0, 1f - masteryFactor);

        return baseTime * timeReduction;
    }
}
