// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Ares.Sanity.Behaviors;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared.Chat;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Server._Ares.Sanity.Systems;

/// <summary>
/// Raises sanity when the entity speaks in local or radio chat, on a cooldown.
/// </summary>
public sealed partial class SanityChatSystem : SanityChangeSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EntitySpokeEvent>(OnEntitySpoke);
    }

    private void OnEntitySpoke(EntitySpokeEvent args)
    {
        if (args.IsWhisper)
            return;

        if (!TryComp<SanityComponent>(args.Source, out var sanity))
            return;

        var behavior = sanity.Changes.OfType<ChatSanityChangeBehavior>().FirstOrDefault();
        if (behavior == null)
            return;

        if (_timing.CurTime < sanity.NextMessageTime)
            return;

        var isLocal = args.Channel == null;
        var isRadio = args.Channel != null;

        if (!isLocal && !isRadio)
            return;

        ApplyChange((args.Source, sanity), behavior.Change);
        sanity.NextMessageTime = _timing.CurTime + TimeSpan.FromSeconds(_random.NextFloat(behavior.MinCooldown, behavior.MaxCooldown));
    }
}
