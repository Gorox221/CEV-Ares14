// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Wires;
using Robust.Shared.Serialization;

namespace Content.Shared._Ares.LockHack;

[Serializable, NetSerializable]
public enum LockHackUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class LockHackBoundUserInterfaceState : BoundUserInterfaceState
{
    public LockHackWireData[] Wires { get; }
    public int CorrectPulses { get; }
    public int RequiredPulses { get; }
    public bool Finished { get; }

    public LockHackBoundUserInterfaceState(LockHackWireData[] wires, int correctPulses, int requiredPulses, bool finished)
    {
        Wires = wires;
        CorrectPulses = correctPulses;
        RequiredPulses = requiredPulses;
        Finished = finished;
    }
}

[Serializable, NetSerializable]
public sealed class LockHackPulseMessage : BoundUserInterfaceMessage
{
    public int WireId { get; }

    public LockHackPulseMessage(int wireId)
    {
        WireId = wireId;
    }
}

[Serializable, NetSerializable]
public sealed class LockHackWireData
{
    public int Id;
    public WireColor Color;
    public WireLetter Letter;
    public bool Pulsed;
}
