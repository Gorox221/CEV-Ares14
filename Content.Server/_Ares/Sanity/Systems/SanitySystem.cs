// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Robust.Shared.Timing;

namespace Content.Server._Ares.Sanity.Systems;

public sealed partial class SanitySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SanityComponent, SanityChangedEvent>(OnSanityChanged);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<SanityComponent>();
        while (query.MoveNext(out var uid, out var sanity))
        {
            sanity.Accumulator += frameTime;
            if (sanity.Accumulator < sanity.CheckInterval)
                continue;

            sanity.Accumulator = 0f;

            var ev = new SanityCheckEvent(uid);
            RaiseLocalEvent(ref ev);
        }
    }

    private void OnSanityChanged(EntityUid uid, SanityComponent sanity, ref SanityChangedEvent args)
    {
        if (args.NewValue < args.OldValue)
            sanity.LastDamageTime = _timing.CurTime;
    }
}
