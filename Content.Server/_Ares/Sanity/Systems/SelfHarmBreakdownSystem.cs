// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Breakdowns;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared.Damage;
using Content.Shared.Effects;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._Ares.Sanity.Systems;

/// <summary>
/// Handles the self-harm breakdown: periodically attacks the target while active,
/// then restores sanity once the duration elapses.
/// </summary>
public sealed partial class SelfHarmBreakdownSystem : SanityBreakdownEffectSystem<SelfHarmBreakdownBehavior>
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SelfHarmBreakdownComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<SelfHarmBreakdownComponent, SanityComponent>();
        while (query.MoveNext(out var uid, out var selfHarm, out var sanity))
        {
            if (_timing.CurTime >= selfHarm.EndTime)
            {
                EndBreakdown(uid, selfHarm, sanity);
                continue;
            }

            selfHarm.Accumulator += frameTime;
            if (selfHarm.Accumulator < selfHarm.Interval)
                continue;

            selfHarm.Accumulator = 0f;
            PerformAttack(uid, selfHarm);
        }
    }

    protected override void OnBreakdownTriggered(Entity<SanityComponent> ent, ref SanityBreakdownTriggeredEvent<SelfHarmBreakdownBehavior> args)
    {
        if (args.Behavior.Duration <= 0f)
            return;

        var selfHarm = EnsureComp<SelfHarmBreakdownComponent>(ent);
        selfHarm.EndTime = _timing.CurTime + TimeSpan.FromSeconds(args.Behavior.Duration);
        selfHarm.Accumulator = 0f;
        selfHarm.Interval = args.Behavior.Interval;
        selfHarm.Damage = args.Behavior.Damage;
        selfHarm.ReturnSanity = args.Behavior.SanityReturn;
    }

    private void EndBreakdown(EntityUid uid, SelfHarmBreakdownComponent selfHarm, SanityComponent sanity)
    {
        var oldValue = sanity.CurrentSanity;
        var newValue = Math.Clamp(selfHarm.ReturnSanity, sanity.MinSanity, sanity.MaxSanity);

        RemComp<SelfHarmBreakdownComponent>(uid);
        sanity.CurrentSanity = newValue;
        Dirty(uid, sanity);

        _popup.PopupEntity(Loc.GetString("sanity-breakdown-selfharm-end-popup"), uid, uid);

        var ev = new SanityChangedEvent(uid, oldValue, newValue, newValue - oldValue);
        RaiseLocalEvent(uid, ref ev, true);
    }

    private void OnMobStateChanged(Entity<SelfHarmBreakdownComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState is MobState.Dead or MobState.Critical)
            RemComp<SelfHarmBreakdownComponent>(ent);
    }

    private void PerformAttack(EntityUid uid, SelfHarmBreakdownComponent selfHarm)
    {
        if (_melee.TryGetWeapon(uid, out var weaponUid, out var weapon))
        {
            var netUid = GetNetEntity(uid);
            var netWeapon = GetNetEntity(weaponUid);
            var coords = GetNetCoordinates(Transform(uid).Coordinates);
            var ev = new LightAttackEvent(netUid, netWeapon, coords);
            _melee.DoLightAttack(uid, ev, weaponUid, weapon, null);

            var angle = Transform(uid).LocalRotation;
            var localPos = angle.ToVec() * 0.5f;
            _melee.DoLunge(uid, weaponUid, weapon.Angle, localPos, weapon.Animation, weapon.AnimationRotation, weapon.FlipAnimation, predicted: false);

            var attackerFilter = Filter.Entities(uid);
            _audio.PlayEntity(weapon.SwingSound, attackerFilter, weaponUid, true, weapon.SwingSound.Params);

            var hitSound = weapon.HitSound ?? weapon.NoDamageSound;
            if (hitSound != null)
                _audio.PlayEntity(hitSound, attackerFilter, uid, true, hitSound.Params);

            _color.RaiseEffect(Color.Red, new List<EntityUid> { uid }, attackerFilter);
        }
        else if (selfHarm.Damage != null)
        {
            _damageable.TryChangeDamage(uid, selfHarm.Damage, interruptsDoAfters: false, origin: uid);
        }
    }
}
