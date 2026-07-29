// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Prototypes;
using Content.Shared.Damage;

namespace Content.Server._Ares.Sanity.Breakdowns;

public static class StalwartBreakdown
{
    public static void Execute(EntityUid uid, SanityComponent sanity, SanityBreakdownPrototype proto,
        DamageableSystem damageable)
    {
        if (proto.Healing != null)
            damageable.TryChangeDamage(uid, proto.Healing, ignoreResistances: true);

        sanity.CurrentSanity = proto.SanityReturn;
    }
}
