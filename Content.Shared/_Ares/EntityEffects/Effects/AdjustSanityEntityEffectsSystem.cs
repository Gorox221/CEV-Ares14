// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._Ares.EntityEffects.Effects;

public sealed partial class AdjustSanity : EntityEffectBase<AdjustSanity>
{
    [DataField(required: true)]
    public float Amount;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        var amountColor = Amount >= 0 ? "#00cc44" : "#cc2222";
        var sign = Amount >= 0 ? "+" : "";

        return Loc.GetString("ares-effect-guidebook-adjust-sanity",
            ("chance", Probability),
            ("amount", $"{sign}{Amount}"),
            ("amountColor", amountColor),
            ("sanityWord", Loc.GetString("ares-sanity-guidebook-word")));
    }
}

public sealed partial class AdjustSanityEntityEffectSystem : EntityEffectSystem<SanityComponent, AdjustSanity>
{
    protected override void Effect(Entity<SanityComponent> ent, ref EntityEffectEvent<AdjustSanity> args)
    {
        if (MathHelper.CloseTo(args.Effect.Amount, 0f))
            return;

        // During a breakdown sanity can never be restored, only reduced.
        if (args.Effect.Amount > 0f && ent.Comp.CurrentBreakdown != null)
            return;

        var oldValue = ent.Comp.CurrentSanity;
        var newValue = Math.Clamp(oldValue + args.Effect.Amount * args.Scale, ent.Comp.MinSanity, ent.Comp.MaxSanity);

        if (MathHelper.CloseTo(oldValue, newValue))
            return;

        ent.Comp.CurrentSanity = newValue;
        Dirty(ent, ent.Comp);

        var ev = new SanityChangedEvent(ent.Owner, oldValue, newValue, newValue - oldValue);
        RaiseLocalEvent(ent.Owner, ref ev, true);
    }
}