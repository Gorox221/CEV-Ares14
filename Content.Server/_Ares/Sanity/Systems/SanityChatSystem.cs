// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared.Chat;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Ares.Sanity.Systems;

public sealed partial class SanityChatSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<EntitySpokeEvent>(OnEntitySpoke);
    }

    private void OnEntitySpoke(EntitySpokeEvent args)
    {
        if (args.IsWhisper)
            return;

        if (!TryComp<SanityComponent>(args.Source, out var sanity))
            return;

        if (_timing.CurTime < sanity.NextMessageTime)
            return;

        var isLocal = args.Channel == null;
        var isRadio = args.Channel != null;

        if (!isLocal && !isRadio)
            return;

        var oldValue = sanity.CurrentSanity;
        var newValue = Math.Clamp(oldValue + 1f, sanity.MinSanity, sanity.MaxSanity);

        if (MathHelper.CloseTo(oldValue, newValue))
            return;

        sanity.CurrentSanity = newValue;
        sanity.NextMessageTime = _timing.CurTime + TimeSpan.FromSeconds(_random.Next(30, 46));
        Dirty(args.Source, sanity);

        var ev = new SanityChangedEvent(args.Source, oldValue, newValue, newValue - oldValue);
        RaiseLocalEvent(args.Source, ref ev);
    }
}
