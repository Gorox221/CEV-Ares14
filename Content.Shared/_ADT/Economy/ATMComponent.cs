using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Audio;
using Robust.Shared.Serialization;

namespace Content.Shared._ADT.Economy;

[RegisterComponent]
public sealed partial class ATMComponent : Component
{
    [DataField("idCardSlot")]
    public ItemSlot CardSlot = new ();

    [DataField("currencyType")]
    public string CurrencyType = "SpaceCash";

    public string SlotId = "IdCardSlot";

    public string CreditStackPrototype = "Credit";

    [DataField("soundInsertCurrency")]
    public SoundSpecifier SoundInsertCurrency = new SoundPathSpecifier("/Audio/_ADT/Machines/polaroid2.ogg");

    [DataField("soundWithdrawCurrency")]
    public SoundSpecifier SoundWithdrawCurrency = new SoundPathSpecifier("/Audio/_ADT/Machines/polaroid1.ogg");

    [DataField("soundApply")]
    public SoundSpecifier SoundApply = new SoundPathSpecifier("/Audio/_ADT/Machines/chime.ogg");

    [DataField("soundDeny")]
    public SoundSpecifier SoundDeny = new SoundPathSpecifier("/Audio/_ADT/Machines/buzz-sigh.ogg");

    [DataField("errorChance")]
    public float ErrorChance = 0.25f;
}


[Serializable, NetSerializable]
public enum ATMUiKey
{
    Key
}
