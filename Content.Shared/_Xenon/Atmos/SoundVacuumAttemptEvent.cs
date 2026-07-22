namespace Content.Shared._Xenon.Atmos;

public sealed class SoundVacuumAttemptEvent : CancellableEntityEventArgs
{
    public EntityUid Source;
    public EntityUid? User;

    public SoundVacuumAttemptEvent(EntityUid source, EntityUid? user)
    {
        Source = source;
        User = user;
    }
}
