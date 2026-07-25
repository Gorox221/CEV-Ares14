// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.Stats;

public sealed class StatCritSystem : EntitySystem
{
    [Dependency] private readonly AresStatsSystem _stats = default!;
    [Dependency] private readonly MobThresholdSystem _thresholds = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<StatsComponent, ComponentInit>(OnStatsComponentInit);
        SubscribeLocalEvent<StatsComponent, StatLevelChangedEvent>(OnStatLevelChanged);
    }

    private void OnStatsComponentInit(Entity<StatsComponent> ent, ref ComponentInit args)
    {
        if (!HasComp<MobThresholdsComponent>(ent))
            return;

        var modifier = EnsureComp<CritThresholdModifierComponent>(ent);

        if (!_prototypes.HasIndex(new ProtoId<StatPrototype>("Toughness")))
            return;

        if (_thresholds.TryGetThresholdForState(ent, MobState.Critical, out var baseThreshold))
            modifier.BaseThreshold = baseThreshold.Value.Float();

        RefreshCritThreshold((ent, modifier));
    }

    private void OnStatLevelChanged(Entity<StatsComponent> ent, ref StatLevelChangedEvent args)
    {
        if (args.StatId.Id != "Toughness")
            return;

        if (!TryComp<CritThresholdModifierComponent>(ent, out var modifier))
            return;

        RefreshCritThreshold((ent, modifier));
    }

    private void RefreshCritThreshold(Entity<CritThresholdModifierComponent> ent)
    {
        if (!_prototypes.HasIndex(new ProtoId<StatPrototype>("Toughness")))
            return;

        var thg = _stats.GetStatLevel(ent, new ProtoId<StatPrototype>("Toughness"));
        var bonus = thg * 0.5f;
        var newThreshold = FixedPoint2.New(ent.Comp.BaseThreshold + bonus);

        _thresholds.SetMobStateThreshold(ent, newThreshold, MobState.Critical);
    }
}