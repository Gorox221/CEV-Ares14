// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Ares.Sanity.Components;
using Content.Server.Chat.Systems;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Speech.Components;
using Content.Shared.Speech.Muting;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Ares.Sanity.Systems;

public sealed partial class SanityEmoteSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<SanityEmoteComponent>();
        while (query.MoveNext(out var uid, out var emote))
        {
            if (_timing.CurTime < emote.NextEmoteTime)
                continue;

            emote.NextEmoteTime = _timing.CurTime + TimeSpan.FromSeconds(emote.Interval);

            if (emote.Emotes.Count == 0 || !_random.Prob(emote.Chance))
                continue;

            var emoteId = _random.Pick(emote.Emotes);
            _chat.TryEmoteWithChat(uid, emoteId, ignoreActionBlocker: true, forceEmote: true);

            if (HasComp<MutedComponent>(uid))
                PlayEmoteSound(uid, emoteId);
        }
    }

    private void PlayEmoteSound(EntityUid uid, ProtoId<EmotePrototype> emoteId)
    {
        if (!TryComp<VocalComponent>(uid, out var vocal) || vocal.EmoteSounds is not { } soundsId)
            return;

        if (!_proto.TryIndex(emoteId, out EmotePrototype? emote)
            || !_proto.TryIndex(soundsId, out EmoteSoundsPrototype? sounds))
            return;

        _chat.TryPlayEmoteSound(uid, sounds, emote);
    }

    public void StartEmoting(EntityUid uid, List<ProtoId<EmotePrototype>> emotes, float interval, float chance)
    {
        var comp = EnsureComp<SanityEmoteComponent>(uid);
        comp.Emotes = emotes;
        comp.Interval = interval;
        comp.Chance = chance;
        comp.NextEmoteTime = _timing.CurTime + TimeSpan.FromSeconds(interval);
    }

    public void StopEmoting(EntityUid uid)
    {
        RemComp<SanityEmoteComponent>(uid);
    }
}
