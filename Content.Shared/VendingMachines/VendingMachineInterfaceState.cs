// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared.VendingMachines
{
    [Serializable, NetSerializable]
    public sealed class VendingMachineEjectMessage : BoundUserInterfaceMessage
    {
        public readonly InventoryType Type;
        public readonly string ID;
        public VendingMachineEjectMessage(InventoryType type, string id)
        {
            Type = type;
            ID = id;
        }
    }

    [Serializable, NetSerializable]
    public sealed class VendingMachineEjectCountMessage : BoundUserInterfaceMessage
    {
        public readonly VendingMachineInventoryEntry Entry;
        public readonly int Count;

        public VendingMachineEjectCountMessage(VendingMachineInventoryEntry entry, int count)
        {
            Entry = entry;
            Count = count;
        }
    }

    [Serializable, NetSerializable]
    public sealed class VendingMachineWithdrawMessage : BoundUserInterfaceMessage
    {
    }

    [Serializable, NetSerializable]
    public sealed class VendingMachineUpdateState : BoundUserInterfaceState
    {
        public List<VendingMachineInventoryEntry> Inventory;
        public double PriceMultiplier;
        public int Credits;
        public int PlayerBalance;
        public bool AllForFree;

        public VendingMachineUpdateState(
            List<VendingMachineInventoryEntry> inventory,
            double priceMultiplier,
            int credits,
            bool allForFree,
            int playerBalance = 0)
        {
            Inventory = inventory;
            PriceMultiplier = priceMultiplier;
            Credits = credits;
            AllForFree = allForFree;
            PlayerBalance = playerBalance;
        }
    }

    [Serializable, NetSerializable]
    public sealed class VendingMachineRequestUpdateMessage : BoundUserInterfaceMessage
    {
    }

    [Serializable, NetSerializable]
    public enum VendingMachineUiKey
    {
        Key,
    }
}