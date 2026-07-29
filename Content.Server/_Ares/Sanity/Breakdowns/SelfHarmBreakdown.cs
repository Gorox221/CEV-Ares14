// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Prototypes;
using Content.Shared.Damage;
using Content.Shared.Effects;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._Ares.Sanity.Breakdowns;

public static class SelfHarmBreakdown
{
    public static void Execute(EntityUid uid, SanityComponent sanity, SanityBreakdownPrototype proto,
        IGameTiming timing)
    {
        sanity.SelfHarmReturnSanity = proto.SanityReturn;
        sanity.SelfHarmAccumulator = 0f;
        sanity.SelfHarmInterval = proto.SelfHarmInterval;
        sanity.SelfHarmDamage = proto.SelfHarmDamage;

        if (proto.Duration > 0f)
            sanity.SelfHarmEndTime = timing.CurTime + TimeSpan.FromSeconds(proto.Duration);
    }

    public static void PerformAttack(EntityUid uid, SanityComponent sanity,
        SharedMeleeWeaponSystem melee, DamageableSystem damageable,
        SharedAudioSystem audio, SharedColorFlashEffectSystem color,
        IEntityManager entityManager)
    {
        if (melee.TryGetWeapon(uid, out var weaponUid, out var weapon))
        {
            var netUid = entityManager.GetNetEntity(uid);
            var netWeapon = entityManager.GetNetEntity(weaponUid);
            var transform = entityManager.GetComponent<TransformComponent>(uid);
            var coords = entityManager.GetNetCoordinates(transform.Coordinates);
            var ev = new LightAttackEvent(netUid, netWeapon, coords);
            melee.DoLightAttack(uid, ev, weaponUid, weapon, null);

            var angle = transform.LocalRotation;
            var localPos = angle.ToVec() * 0.5f;
            melee.DoLunge(uid, weaponUid, weapon.Angle, localPos, weapon.Animation, weapon.AnimationRotation, weapon.FlipAnimation, predicted: false);

            var attackerFilter = Filter.Entities(uid);
            audio.PlayEntity(weapon.SwingSound, attackerFilter, weaponUid, true, weapon.SwingSound.Params);

            var hitSound = weapon.HitSound ?? weapon.NoDamageSound;
            if (hitSound != null)
                audio.PlayEntity(hitSound, attackerFilter, uid, true, hitSound.Params);

            color.RaiseEffect(Color.Red, new List<EntityUid> { uid }, attackerFilter);
        }
        else if (sanity.SelfHarmDamage != null)
        {
            damageable.TryChangeDamage(uid, sanity.SelfHarmDamage, interruptsDoAfters: false, origin: uid);
        }
    }
}
