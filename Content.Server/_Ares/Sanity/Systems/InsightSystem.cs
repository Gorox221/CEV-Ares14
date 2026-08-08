// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;

namespace Content.Server._Ares.Sanity.Systems;

public sealed partial class InsightSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SanityChangedEvent>(OnSanityChanged);
    }

    private void OnSanityChanged(ref SanityChangedEvent args)
    {
        var uid = args.Entity;

        if (!HasComp<SanityComponent>(uid))
            return;

        var insight = EnsureComp<InsightComponent>(uid);
        var absDelta = Math.Abs(args.Delta);

        insight.LevelChange += absDelta;
        var gain = 0.05f + insight.LevelChange / 15f;

        var gainEvent = new InsightGainModifierEvent() { Entity = uid };
        RaiseLocalEvent(uid, ref gainEvent, true);
        gain *= gainEvent.Multiplier;

        insight.CurrentInsight = Math.Min(insight.CurrentInsight + gain, insight.MaxInsight);
        insight.LevelChange = 0f;

        Dirty(uid, insight);
    }
}
