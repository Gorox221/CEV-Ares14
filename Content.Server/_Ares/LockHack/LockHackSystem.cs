using System.Linq;
using Content.Server.Electrocution;
using Content.Server.Hands.Systems;
using Content.Shared._Ares.LockHack;
using Content.Shared._Ares.Stats;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction;
using Content.Shared.Lock;
using Content.Shared.Popups;
using Content.Shared.Tools;
using Content.Shared.Tools.Components;
using Content.Shared.Tools.Systems;
using Content.Shared.Wires;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Ares.LockHack;

public sealed class LockHackSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly LockSystem _lockSystem = default!;
    [Dependency] private readonly AresStatsSystem _stats = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly SharedToolSystem _toolSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly ElectrocutionSystem _electrocution = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;

    private static readonly ProtoId<ToolQualityPrototype> PulsingQuality = "Pulsing";

    private readonly Dictionary<EntityUid, LockHackData> _activeHacks = new();

    private static readonly SoundSpecifier PulseSound = new SoundPathSpecifier("/Audio/Effects/multitool_pulse.ogg");

    public override void Initialize()
    {
        SubscribeLocalEvent<LockHackComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<LockHackComponent, LockHackPulseMessage>(OnPulseMessage);
        SubscribeLocalEvent<LockHackComponent, BoundUIClosedEvent>(OnUiClosed);
        SubscribeLocalEvent<LockHackComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnInteractUsing(Entity<LockHackComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!_toolSystem.HasQuality(args.Used, PulsingQuality))
            return;

        if (!TryComp<LockComponent>(ent, out var lockComp) || !lockComp.Locked)
            return;

        if (!TryComp<ActorComponent>(args.User, out var actor))
            return;

        args.Handled = true;

        _adminLogger.Add(LogType.Action, LogImpact.Medium,
            $"{ToPrettyString(args.User):user} began hacking {ToPrettyString(ent.Owner):target} at {Transform(ent.Owner).Coordinates:targetlocation}");

        InitializeHack(ent, args.User);
        _uiSystem.OpenUi(ent.Owner, LockHackUiKey.Key, actor.PlayerSession);
    }

    private void InitializeHack(Entity<LockHackComponent> ent, EntityUid user)
    {
        var component = ent.Comp;
        var wireCount = component.WireCount;
        var required = component.RequiredPulses;

        var correctIndices = new HashSet<int>();
        while (correctIndices.Count < required)
        {
            correctIndices.Add(_random.Next(wireCount));
        }

        var colors = (WireColor[])Enum.GetValues(typeof(WireColor));
        var letters = (WireLetter[])Enum.GetValues(typeof(WireLetter));

        var wires = new LockHackWireData[wireCount];
        for (var i = 0; i < wireCount; i++)
        {
            wires[i] = new LockHackWireData
            {
                Id = i,
                Color = colors[i % colors.Length],
                Letter = letters[i % letters.Length],
            };
        }

        var hackData = new LockHackData
        {
            CorrectWires = correctIndices,
            Wires = wires,
            User = user,
        };
        _activeHacks[ent.Owner] = hackData;

        UpdateUi(ent);
    }

    private void OnPulseMessage(Entity<LockHackComponent> ent, ref LockHackPulseMessage args)
    {
        if (!_activeHacks.TryGetValue(ent.Owner, out var hackData))
            return;

        var user = hackData.User;
        if (user is not { Valid: true })
            return;

        var userVal = user.Value;

        if (!TryComp<HandsComponent>(userVal, out var hands))
        {
            _popup.PopupEntity(Loc.GetString("ares-lockhack-no-hands"), ent.Owner, userVal);
            return;
        }

        if (!_interaction.InRangeUnobstructed(userVal, ent.Owner))
        {
            _popup.PopupEntity(Loc.GetString("ares-lockhack-cannot-reach"), ent.Owner, userVal);
            return;
        }

        if (!_hands.TryGetActiveItem((userVal, hands), out var heldEntity) || heldEntity is not { } held || !_toolSystem.HasQuality(held, PulsingQuality))
        {
            _popup.PopupEntity(Loc.GetString("ares-lockhack-need-multitool"), ent.Owner, userVal);
            return;
        }

        var wireId = args.WireId;

        if (wireId < 0 || wireId >= hackData.Wires.Length)
            return;

        if (hackData.PulsedWires.Contains(wireId))
            return;

        _audio.PlayPvs(PulseSound, ent.Owner);

        if (!hackData.CorrectWires.Contains(wireId))
        {
            TryShockUser(ent, userVal, wireId);
            return;
        }

        hackData.PulsedWires.Add(wireId);
        hackData.Wires[wireId].Pulsed = true;
        hackData.PulsedCount++;
        _adminLogger.Add(LogType.Action, LogImpact.Low,
            $"{ToPrettyString(userVal):user} pulsed correct wire #{wireId} on {ToPrettyString(ent.Owner):target} ({hackData.PulsedCount}/{ent.Comp.RequiredPulses})");

        TryInstantUnlock(ent, userVal);

        if (hackData.PulsedCount >= ent.Comp.RequiredPulses)
        {
            FinishHack(ent);
            return;
        }

        UpdateUi(ent);
    }

    private void TryShockUser(Entity<LockHackComponent> ent, EntityUid user, int wireId)
    {
        _electrocution.TryDoElectrocution(user, ent.Owner, 10, TimeSpan.FromSeconds(2), false);
        _adminLogger.Add(LogType.Action, LogImpact.Low,
            $"{ToPrettyString(user):user} pulsed WRONG wire #{wireId} on {ToPrettyString(ent.Owner):target} and got shocked");
    }

    private void TryInstantUnlock(Entity<LockHackComponent> ent, EntityUid user)
    {
        var conditionProto = new ProtoId<StatPrototype>("Cognition");
        if (!_prototypes.HasIndex(conditionProto))
            return;

        var cognitionLevel = _stats.GetStatLevel(user, conditionProto);
        if (cognitionLevel <= 0)
            return;

        if (_random.Prob(cognitionLevel / 100f))
        {
            FinishHack(ent, true);
        }
    }

    private void FinishHack(Entity<LockHackComponent> ent, bool instantUnlock = false)
    {
        if (!_activeHacks.TryGetValue(ent.Owner, out var hackData))
            return;

        var user = hackData.User;

        if (user is not { Valid: true })
        {
            _activeHacks.Remove(ent.Owner);
            _uiSystem.CloseUi(ent.Owner, LockHackUiKey.Key);
            return;
        }

        var userVal = user.Value;

        if (instantUnlock)
            _adminLogger.Add(LogType.Action, LogImpact.Medium,
                $"{ToPrettyString(userVal):user} instantly unlocked {ToPrettyString(ent.Owner):target} via Cognition check");
        else
            _adminLogger.Add(LogType.Action, LogImpact.Medium,
                $"{ToPrettyString(userVal):user} finished hacking {ToPrettyString(ent.Owner):target}");

        if (TryComp<LockComponent>(ent, out var lockComp) && lockComp.Locked)
            _lockSystem.Unlock(ent.Owner, userVal);

        foreach (var wire in hackData.Wires)
            wire.Pulsed = true;

        _popup.PopupEntity(Loc.GetString("ares-lockhack-success"), ent.Owner, userVal);

        UpdateUi(ent);
        _activeHacks.Remove(ent.Owner);
        _uiSystem.CloseUi(ent.Owner, LockHackUiKey.Key);
    }

    private void UpdateUi(Entity<LockHackComponent> ent)
    {
        if (!_activeHacks.TryGetValue(ent.Owner, out var hackData))
            return;

        var finished = hackData.PulsedCount >= ent.Comp.RequiredPulses
            || (TryComp<LockComponent>(ent, out var lockComp) && !lockComp.Locked);

        var state = new LockHackBoundUserInterfaceState(
            hackData.Wires,
            hackData.PulsedCount,
            ent.Comp.RequiredPulses,
            finished);

        _uiSystem.SetUiState(ent.Owner, LockHackUiKey.Key, state);
    }

    private void OnUiClosed(Entity<LockHackComponent> ent, ref BoundUIClosedEvent args)
    {
        if (args.UiKey is not LockHackUiKey.Key)
            return;

        if (!_activeHacks.Remove(ent.Owner, out var hackData))
            return;

        var user = hackData.User;
        if (user is { Valid: true })
        {
            _adminLogger.Add(LogType.Action, LogImpact.Low,
                $"{ToPrettyString(user.Value):user} cancelled hacking {ToPrettyString(ent.Owner):target}");
            _popup.PopupEntity(Loc.GetString("ares-lockhack-cancelled"), ent.Owner, user.Value);
        }
    }

    private void OnShutdown(Entity<LockHackComponent> ent, ref ComponentShutdown args)
    {
        _activeHacks.Remove(ent.Owner);
    }

    private sealed class LockHackData
    {
        public HashSet<int> CorrectWires = new();
        public HashSet<int> PulsedWires = new();
        public LockHackWireData[] Wires = Array.Empty<LockHackWireData>();
        public EntityUid? User;
        public int PulsedCount;
    }
}
