// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Managers;
using Content.Shared._Ares.Sanity.Components;
using Content.Shared._Ares.Sanity.Events;
using Content.Shared.Chat;
using Robust.Server.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Text;

namespace Content.Server._Ares.Sanity.Systems;

/// <summary>
/// Sends hallucinatory chat messages to players with low sanity.
/// Messages are styled with the TV-static (camera_static) shader
/// but with a normal font size.
/// </summary>
public sealed partial class SanityQuoteSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPlayerManager _playerMan = default!;
    [Dependency] private readonly IChatManager _chatMan = default!;

    private const string BaseMessageLocId = "sanity-chat-base";
    private const int TextFontSize = 12;

    [DataField]
    public float MildMinInterval = 30f;

    [DataField]
    public float MildMaxInterval = 60f;

    [DataField]
    public float SevereMinInterval = 15f;

    [DataField]
    public float SevereMaxInterval = 30f;

    private static readonly string[] MildMessages =
    {
        "sanity-chat-mild-1",
        "sanity-chat-mild-2",
        "sanity-chat-mild-3",
        "sanity-chat-mild-4",
        "sanity-chat-mild-5",
        "sanity-chat-mild-6",
        "sanity-chat-mild-7",
        "sanity-chat-mild-8",
        "sanity-chat-mild-9",
        "sanity-chat-mild-10",
        "sanity-chat-mild-11",
        "sanity-chat-mild-12",
        "sanity-chat-mild-13",
        "sanity-chat-mild-14",
        "sanity-chat-mild-15",
        "sanity-chat-mild-16",
        "sanity-chat-mild-17",
        "sanity-chat-mild-18",
        "sanity-chat-mild-19",
        "sanity-chat-mild-20",
        "sanity-chat-mild-21",
        "sanity-chat-mild-22",
        "sanity-chat-mild-23",
        "sanity-chat-mild-24",
        "sanity-chat-mild-25",
        "sanity-chat-mild-26",
        "sanity-chat-mild-27",
        "sanity-chat-mild-28",
        "sanity-chat-mild-29",
        "sanity-chat-mild-30",
        "sanity-chat-mild-31",
        "sanity-chat-mild-32",
        "sanity-chat-mild-33",
        "sanity-chat-mild-34",
        "sanity-chat-mild-35",
        "sanity-chat-mild-36",
        "sanity-chat-mild-37",
        "sanity-chat-mild-38",
        "sanity-chat-mild-39",
        "sanity-chat-mild-40",
        "sanity-chat-mild-41",
        "sanity-chat-mild-42",
        "sanity-chat-mild-43",
        "sanity-chat-mild-44",
        "sanity-chat-mild-45",
        "sanity-chat-mild-46",
        "sanity-chat-mild-47",
        "sanity-chat-mild-48",
        "sanity-chat-mild-49",
        "sanity-chat-mild-50",
        "sanity-chat-mild-51",
        "sanity-chat-mild-52",
        "sanity-chat-mild-53",
        "sanity-chat-mild-54",
        "sanity-chat-mild-55",
        "sanity-chat-mild-56",
        "sanity-chat-mild-57",
        "sanity-chat-mild-58",
        "sanity-chat-mild-59",
        "sanity-chat-mild-60",
        "sanity-chat-mild-61",
        "sanity-chat-mild-62",
        "sanity-chat-mild-63",
        "sanity-chat-mild-64",
        "sanity-chat-mild-65",
        "sanity-chat-mild-66",
        "sanity-chat-mild-67",
        "sanity-chat-mild-68",
        "sanity-chat-mild-69",
        "sanity-chat-mild-70",
        "sanity-chat-mild-71",
        "sanity-chat-mild-72",
        "sanity-chat-mild-73",
        "sanity-chat-mild-74",
        "sanity-chat-mild-75",
        "sanity-chat-mild-76",
        "sanity-chat-mild-77",
        "sanity-chat-mild-78",
        "sanity-chat-mild-79",
        "sanity-chat-mild-80",
        "sanity-chat-mild-81",
        "sanity-chat-mild-82",
        "sanity-chat-mild-83",
        "sanity-chat-mild-84",
        "sanity-chat-mild-85",
        "sanity-chat-mild-86",
        "sanity-chat-mild-87",
    };

    private static readonly string[] SevereMessages =
    {
        "sanity-chat-severe-1",
        "sanity-chat-severe-2",
        "sanity-chat-severe-3",
        "sanity-chat-severe-4",
        "sanity-chat-severe-5",
        "sanity-chat-severe-6",
        "sanity-chat-severe-7",
        "sanity-chat-severe-8",
        "sanity-chat-severe-9",
        "sanity-chat-severe-10",
        "sanity-chat-severe-11",
        "sanity-chat-severe-12",
        "sanity-chat-severe-13",
        "sanity-chat-severe-14",
        "sanity-chat-severe-15",
        "sanity-chat-severe-16",
        "sanity-chat-severe-17",
        "sanity-chat-severe-18",
        "sanity-chat-severe-19",
        "sanity-chat-severe-20",
        "sanity-chat-severe-21",
        "sanity-chat-severe-22",
        "sanity-chat-severe-23",
        "sanity-chat-severe-24",
        "sanity-chat-severe-25",
        "sanity-chat-severe-26",
        "sanity-chat-severe-27",
        "sanity-chat-severe-28",
        "sanity-chat-severe-29",
        "sanity-chat-severe-30",
        "sanity-chat-severe-31",
        "sanity-chat-severe-32",
        "sanity-chat-severe-33",
        "sanity-chat-severe-34",
        "sanity-chat-severe-35",
        "sanity-chat-severe-36",
        "sanity-chat-severe-37",
        "sanity-chat-severe-38",
        "sanity-chat-severe-39",
        "sanity-chat-severe-40",
        "sanity-chat-severe-41",
        "sanity-chat-severe-42",
        "sanity-chat-severe-43",
        "sanity-chat-severe-44",
        "sanity-chat-severe-45",
        "sanity-chat-severe-46",
        "sanity-chat-severe-47",
        "sanity-chat-severe-48",
        "sanity-chat-severe-49",
        "sanity-chat-severe-50",
        "sanity-chat-severe-51",
        "sanity-chat-severe-52",
        "sanity-chat-severe-53",
        "sanity-chat-severe-54",
        "sanity-chat-severe-55",
        "sanity-chat-severe-56",
        "sanity-chat-severe-57",
        "sanity-chat-severe-58",
        "sanity-chat-severe-59",
        "sanity-chat-severe-60",
        "sanity-chat-severe-61",
        "sanity-chat-severe-62",
        "sanity-chat-severe-63",
        "sanity-chat-severe-64",
        "sanity-chat-severe-65",
        "sanity-chat-severe-66",
        "sanity-chat-severe-67",
        "sanity-chat-severe-68",
        "sanity-chat-severe-69",
        "sanity-chat-severe-70",
        "sanity-chat-severe-71",
        "sanity-chat-severe-72",
        "sanity-chat-severe-73",
        "sanity-chat-severe-74",
        "sanity-chat-severe-75",
        "sanity-chat-severe-76",
        "sanity-chat-severe-77",
        "sanity-chat-severe-78",
        "sanity-chat-severe-79",
        "sanity-chat-severe-80",
        "sanity-chat-severe-81",
        "sanity-chat-severe-82",
        "sanity-chat-severe-83",
        "sanity-chat-severe-84",
        "sanity-chat-severe-85",
        "sanity-chat-severe-86",
        "sanity-chat-severe-87",
        "sanity-chat-severe-88",
        "sanity-chat-severe-89",
        "sanity-chat-severe-90",
        "sanity-chat-severe-91",
        "sanity-chat-severe-92",
        "sanity-chat-severe-93",
        "sanity-chat-severe-94",
        "sanity-chat-severe-95",
        "sanity-chat-severe-96",
        "sanity-chat-severe-97",
        "sanity-chat-severe-98",
        "sanity-chat-severe-99",
        "sanity-chat-severe-100",
    };

    public override void Initialize()
    {
        SubscribeLocalEvent<SanityCheckEvent>(OnSanityCheck);
    }

    private void OnSanityCheck(ref SanityCheckEvent args)
    {
        if (!TryComp<SanityComponent>(args.Entity, out var sanity))
            return;

        if (_timing.CurTime < sanity.NextHallucinationTime)
            return;

        if (sanity.CurrentSanity > 40)
            return;

        var severe = sanity.CurrentSanity < 20;
        var messages = severe ? SevereMessages : MildMessages;
        sanity.NextHallucinationTime = _timing.CurTime + TimeSpan.FromSeconds(
            _random.NextFloat(severe ? SevereMinInterval : MildMinInterval,
                severe ? SevereMaxInterval : MildMaxInterval));

        if (!_playerMan.TryGetSessionByEntity(args.Entity, out var session))
            return;

        var message = Loc.GetString(_random.Pick(messages));
        message = WrapMessage(message);
        var loc = Loc.GetString(BaseMessageLocId, ("size", TextFontSize), ("text", message));
        _chatMan.ChatMessageToOne(ChatChannel.Server,
            message,
            loc,
            default,
            false,
            session.Channel,
            canCoalesce: false);
    }

    /// <summary>
    /// The chat renders the TV-static-styled text as a single non-wrapping label,
    /// so long messages have to be wrapped into lines manually.
    /// </summary>
    private static string WrapMessage(string text, int maxLineLength = 50)
    {
        if (text.Length <= maxLineLength)
            return text;

        var result = new StringBuilder();
        var line = new StringBuilder();
        foreach (var word in text.Split(' '))
        {
            if (line.Length == 0)
            {
                line.Append(word);
                continue;
            }

            if (line.Length + 1 + word.Length <= maxLineLength)
            {
                line.Append(' ').Append(word);
                continue;
            }

            result.Append(line).Append('\n');
            line.Clear();
            line.Append(word);
        }

        result.Append(line);
        return result.ToString();
    }
}
