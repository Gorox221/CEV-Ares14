// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Behaviors;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared._Ares.Stats;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server._Ares.Sanity.Systems;

public abstract partial class SanityChangeSystem : EntitySystem
{
    [Dependency] private readonly AresStatsSystem _stats = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    protected void ApplyChange(Entity<SanityComponent> ent, float delta)
    {
        if (_mobState.IsIncapacitated(ent))
            return;

        if (delta < 0f && HasComp<AbsoluteConcentrationComponent>(ent))
            return;

        if (MathHelper.CloseTo(delta, 0f))
            return;

        var oldValue = ent.Comp.CurrentSanity;
        var newValue = Math.Clamp(oldValue + delta, ent.Comp.MinSanity, ent.Comp.MaxSanity);

        if (MathHelper.CloseTo(oldValue, newValue))
            return;

        ent.Comp.CurrentSanity = newValue;
        Dirty(ent, ent.Comp);

        var ev = new SanityChangedEvent(ent, oldValue, newValue, newValue - oldValue);
        RaiseLocalEvent(ent, ref ev, true);
    }

    protected float GetVigilanceMultiplier(EntityUid uid)
    {
        var vigPrototype = new ProtoId<StatPrototype>("Vigilance");
        if (!_prototypes.HasIndex(vigPrototype))
            return 1f;

        var vigLevel = _stats.GetStatLevel(uid, vigPrototype);
        var clampedVig = Math.Clamp(vigLevel, 0, 60);
        return (float)(1.2 - clampedVig / 60.0);
    }

    protected bool CanPerceiveSanityEffects(EntityUid uid)
    {
        if (TryComp<MobStateComponent>(uid, out var mobState)
            && mobState.CurrentState is MobState.Dead or MobState.Critical)
            return false;

        if (TryComp<BlindableComponent>(uid, out var blindable) && blindable.IsBlind)
            return false;

        return true;
    }
}
