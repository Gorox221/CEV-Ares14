using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Components;
using Content.Shared._Xenon.Atmos;
using Content.Shared.Speech;

namespace Content.Server._Xenon.Atmos;

public sealed class VacuumPropagationSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SoundVacuumAttemptEvent>(OnSoundVacuumAttempt);
        SubscribeLocalEvent<SpeakAttemptEvent>(OnSpeakAttempt);
    }

    private void OnSoundVacuumAttempt(SoundVacuumAttemptEvent ev)
    {
        if (ev.User != null)
            return;

        if (IsInVacuum(ev.Source))
            ev.Cancel();
    }

    private void OnSpeakAttempt(SpeakAttemptEvent ev)
    {
        if (!TryComp<RespiratorComponent>(ev.Uid, out var respirator))
            return;

        if (respirator.Saturation <= respirator.SuffocationThreshold)
            ev.Cancel();
    }

    public bool IsInVacuum(EntityUid uid)
    {
        var mixture = _atmosphere.GetContainingMixture(uid, ignoreExposed: true);

        if (mixture == null)
            return true;

        if (mixture.Immutable && mixture.TotalMoles == 0f)
            return true;

        if (mixture.Pressure < 10f)
            return true;

        return false;
    }
}
