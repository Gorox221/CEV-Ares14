// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Humanoid;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Stats;

public sealed partial class AresStatsSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<HumanoidAppearanceComponent, MapInitEvent>(OnHumanoidMapInit);
    }

    private void OnHumanoidMapInit(Entity<HumanoidAppearanceComponent> ent, ref MapInitEvent args)
    {
        if (HasComp<StatsComponent>(ent))
            return;

        var statsComp = AddComp<StatsComponent>(ent);

        foreach (var stat in _prototypes.EnumeratePrototypes<StatPrototype>())
        {
            statsComp.Stats[new ProtoId<StatPrototype>(stat.ID)] = 0;
        }

        Dirty(ent, statsComp);
    }

    public int GetStatLevel(EntityUid uid, ProtoId<StatPrototype> statId, StatsComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return 0;

        return component.Stats.GetValueOrDefault(statId, 0);
    }

    public void SetStatLevel(EntityUid uid, ProtoId<StatPrototype> statId, int level, StatsComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var old = component.Stats.GetValueOrDefault(statId, 0);
        var newLevel = Math.Max(0, level);
        component.Stats[statId] = newLevel;
        Dirty(uid, component);
        var ev = new StatLevelChangedEvent(uid, statId, old, newLevel);
        RaiseLocalEvent(uid, ref ev);
    }

    public void ModifyStatLevel(EntityUid uid, ProtoId<StatPrototype> statId, int delta, StatsComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var current = component.Stats.GetValueOrDefault(statId, 0);
        var newLevel = Math.Max(0, current + delta);
        component.Stats[statId] = newLevel;
        Dirty(uid, component);
        var ev2 = new StatLevelChangedEvent(uid, statId, current, newLevel);
        RaiseLocalEvent(uid, ref ev2);
    }

    public void SetAllStatLevels(EntityUid uid, Dictionary<ProtoId<StatPrototype>, int> stats, StatsComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        foreach (var (statId, level) in stats)
        {
            var old = component.Stats.GetValueOrDefault(statId, 0);
            component.Stats[statId] = Math.Max(0, level);
            var ev = new StatLevelChangedEvent(uid, statId, old, Math.Max(0, level));
            RaiseLocalEvent(uid, ref ev);
        }

        Dirty(uid, component);
    }

    public void ClearStats(EntityUid uid, StatsComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.Stats.Clear();
        Dirty(uid, component);
    }
}
