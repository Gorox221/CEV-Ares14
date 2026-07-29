// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server._Ares.Sanity.Breakdowns;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared._Ares.Sanity.Prototypes;
using Content.Shared.Damage;
using Content.Shared.Effects;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Ares.Sanity.Systems;

public sealed partial class SanityBreakdownSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SanityComponent, SanityChangedEvent>(OnSanityChanged);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<SanityComponent>();
        while (query.MoveNext(out var uid, out var sanity))
        {
            if (TryComp<MobStateComponent>(uid, out var mobState)
                && mobState.CurrentState is MobState.Dead or MobState.Critical)
            {
                sanity.SelfHarmEndTime = TimeSpan.Zero;
                continue;
            }

            if (sanity.SelfHarmEndTime == TimeSpan.Zero)
                continue;

            if (_timing.CurTime >= sanity.SelfHarmEndTime)
            {
                sanity.SelfHarmEndTime = TimeSpan.Zero;
                _popup.PopupEntity(Loc.GetString("sanity-breakdown-selfharm-end-popup"), uid, uid);
                continue;
            }

            sanity.SelfHarmAccumulator += frameTime;
            if (sanity.SelfHarmAccumulator < sanity.SelfHarmInterval)
                continue;

            sanity.SelfHarmAccumulator = 0f;

            SelfHarmBreakdown.PerformAttack(uid, sanity, _melee, _damageable, _audio, _color, EntityManager);
        }
    }

    private void OnSanityChanged(Entity<SanityComponent> ent, ref SanityChangedEvent args)
    {
        if (args.NewValue > 0 || _timing.CurTime < ent.Comp.NextBreakdownTime)
            return;

        var breakdown = PickBreakdown();
        if (breakdown == null)
            return;

        ent.Comp.NextBreakdownTime = _timing.CurTime + TimeSpan.FromMinutes(7);
        Dirty(ent, ent.Comp);

        _popup.PopupEntity(Loc.GetString(breakdown.Popup), ent, ent);

        if (breakdown.Healing != null)
            StalwartBreakdown.Execute(ent, ent.Comp, breakdown, _damageable);
        else
            SelfHarmBreakdown.Execute(ent, ent.Comp, breakdown, _timing);

        Dirty(ent, ent.Comp);

        var ev = new SanityChangedEvent(ent, 0f, ent.Comp.CurrentSanity, ent.Comp.CurrentSanity);
        RaiseLocalEvent(ent, ref ev);
    }

    private SanityBreakdownPrototype? PickBreakdown()
    {
        var all = _prototypes.EnumeratePrototypes<SanityBreakdownPrototype>().ToList();
        if (all.Count == 0)
            return null;

        var totalWeight = all.Sum(p => p.Weight);
        var roll = (float)_random.NextDouble() * totalWeight;

        foreach (var proto in all)
        {
            roll -= proto.Weight;
            if (roll <= 0)
                return proto;
        }

        return all[^1];
    }
}
