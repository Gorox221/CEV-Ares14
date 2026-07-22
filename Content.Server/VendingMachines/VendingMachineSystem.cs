// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.Server._ADT.Economy;
using Content.Server.Access.Systems;
using Content.Server.Cargo.Systems;
using Content.Server.Power.Components;
using Content.Shared._ADT.Economy;
using Content.Shared.Access.Systems;
using Content.Shared.Cargo;
using Content.Server.Vocalization.Systems;
using Content.Shared.Damage;
using Content.Shared.Emp;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.Throwing;
using Content.Shared.VendingMachines;
using Content.Shared.Wall;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.VendingMachines
{
    public sealed class VendingMachineSystem : SharedVendingMachineSystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly PricingSystem _pricing = default!;
        [Dependency] private readonly ThrowingSystem _throwingSystem = default!;
        [Dependency] private readonly BankCardSystem _bankCard = default!;
        [Dependency] private readonly SharedIdCardSystem _idCardSystem = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;

        private const float WallVendEjectDistanceFromWall = 1f;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<VendingMachineComponent, PowerChangedEvent>(OnPowerChanged);
            SubscribeLocalEvent<VendingMachineComponent, DamageChangedEvent>(OnDamageChanged);
            SubscribeLocalEvent<VendingMachineComponent, PriceCalculationEvent>(OnVendingPrice);
            SubscribeLocalEvent<VendingMachineComponent, TryVocalizeEvent>(OnTryVocalize);

            SubscribeLocalEvent<VendingMachineComponent, VendingMachineSelfDispenseEvent>(OnSelfDispense);

            SubscribeLocalEvent<VendingMachineRestockComponent, PriceCalculationEvent>(OnPriceCalculation);

            Subs.BuiEvents<VendingMachineComponent>(VendingMachineUiKey.Key, subs =>
            {
                subs.Event<VendingMachineRequestUpdateMessage>(OnVendingRequestUpdateMessage);
            });
        }

        private void OnVendingRequestUpdateMessage(Entity<VendingMachineComponent> entity, ref VendingMachineRequestUpdateMessage args)
        {
            if (args.Actor is not { Valid: true } actor)
                return;

            var inventory = GetAllInventory(entity.Owner, entity.Comp);
            var playerBalance = GetPlayerBalance(actor);
            var state = new VendingMachineUpdateState(inventory, entity.Comp.PriceMultiplier, entity.Comp.Credits, entity.Comp.AllForFree, playerBalance: playerBalance);
            UISystem.SetUiState(entity.Owner, VendingMachineUiKey.Key, state);
        }

        private int GetPlayerBalance(EntityUid sender)
        {
            if (!_idCardSystem.TryFindIdCard(sender, out var idCard))
                return 0;
            if (!TryComp<BankCardComponent>(idCard.Owner, out var bankCard) || bankCard.AccountId == null)
                return 0;
            return _bankCard.GetBalance(bankCard.AccountId.Value);
        }

        private void OnVendingPrice(EntityUid uid, VendingMachineComponent component, ref PriceCalculationEvent args)
        {
            var price = 0.0;

            foreach (var entry in component.Inventory.Values)
            {
                if (!PrototypeManager.TryIndex<EntityPrototype>(entry.ID, out var proto))
                {
                    Log.Error($"Unable to find entity prototype {entry.ID} on {ToPrettyString(uid)} vending.");
                    continue;
                }

                price += entry.Amount * _pricing.GetEstimatedPrice(proto);
            }

            args.Price += price;
        }

        private void CalculatePrices(EntityUid uid, VendingMachineComponent component)
        {
            foreach (var entry in component.Inventory.Values)
            {
                if (PrototypeManager.TryIndex<EntityPrototype>(entry.ID, out var proto))
                {
                    entry.Price = (int)_pricing.GetEstimatedPrice(proto);
                }
            }

            foreach (var entry in component.EmaggedInventory.Values)
            {
                if (PrototypeManager.TryIndex<EntityPrototype>(entry.ID, out var proto))
                {
                    entry.Price = (int)_pricing.GetEstimatedPrice(proto);
                }
            }

            foreach (var entry in component.ContrabandInventory.Values)
            {
                if (PrototypeManager.TryIndex<EntityPrototype>(entry.ID, out var proto))
                {
                    entry.Price = (int)_pricing.GetEstimatedPrice(proto);
                }
            }

            Dirty(uid, component);
        }

        protected override void OnMapInit(EntityUid uid, VendingMachineComponent component, MapInitEvent args)
        {
            base.OnMapInit(uid, component, args);

            CalculatePrices(uid, component);

            if (HasComp<ApcPowerReceiverComponent>(uid))
            {
                TryUpdateVisualState((uid, component));
            }
        }

        private void OnPowerChanged(EntityUid uid, VendingMachineComponent component, ref PowerChangedEvent args)
        {
            TryUpdateVisualState((uid, component));
        }

        private void OnDamageChanged(EntityUid uid, VendingMachineComponent component, DamageChangedEvent args)
        {
            if (!args.DamageIncreased && component.Broken)
            {
                component.Broken = false;
                Dirty(uid, component);
                TryUpdateVisualState((uid, component));
                return;
            }

            if (component.Broken || component.DispenseOnHitCoolingDown ||
                component.DispenseOnHitChance == null || args.DamageDelta == null)
                return;

            if (args.DamageIncreased && args.DamageDelta.GetTotal() >= component.DispenseOnHitThreshold &&
                _random.Prob(component.DispenseOnHitChance.Value))
            {
                if (component.DispenseOnHitCooldown != null)
                {
                    component.DispenseOnHitEnd = Timing.CurTime + component.DispenseOnHitCooldown.Value;
                }

                EjectRandom(uid, throwItem: true, forceEject: true, component);
            }
        }

        private void OnSelfDispense(EntityUid uid, VendingMachineComponent component, VendingMachineSelfDispenseEvent args)
        {
            if (args.Handled)
                return;

            args.Handled = true;
            EjectRandom(uid, throwItem: true, forceEject: false, component);
        }

        /// <summary>
        /// Sets the <see cref="VendingMachineComponent.CanShoot"/> property of the vending machine.
        /// </summary>
        public void SetShooting(EntityUid uid, bool canShoot, VendingMachineComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return;

            component.CanShoot = canShoot;
        }

        /// <summary>
        /// Sets the <see cref="VendingMachineComponent.Contraband"/> property of the vending machine.
        /// </summary>
        public void SetContraband(EntityUid uid, bool contraband, VendingMachineComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return;

            component.Contraband = contraband;
            Dirty(uid, component);
        }

        /// <summary>
        /// Ejects a random item from the available stock. Will do nothing if the vending machine is empty.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="throwItem">Whether to throw the item in a random direction after dispensing it.</param>
        /// <param name="forceEject">Whether to skip the regular ejection checks and immediately dispense the item without animation.</param>
        /// <param name="vendComponent"></param>
        public void EjectRandom(EntityUid uid, bool throwItem, bool forceEject = false, VendingMachineComponent? vendComponent = null)
        {
            if (!Resolve(uid, ref vendComponent))
                return;

            var availableItems = GetAvailableInventory(uid, vendComponent);
            if (availableItems.Count <= 0)
                return;

            var item = _random.Pick(availableItems);

            if (forceEject)
            {
                vendComponent.NextItemToEject = item.ID;
                vendComponent.ThrowNextItem = throwItem;
                var entry = GetEntry(uid, item.ID, item.Type, vendComponent);
                if (entry != null)
                    entry.Amount--;
                EjectItem(uid, vendComponent, forceEject);
            }
            else
            {
                TryEjectVendorItem(uid, item.Type, item.ID, throwItem, user: null, vendComponent: vendComponent);
            }
        }

        protected override void EjectItem(EntityUid uid, VendingMachineComponent? vendComponent = null, bool forceEject = false)
        {
            if (!Resolve(uid, ref vendComponent))
                return;

            // No need to update the visual state because we never changed it during a forced eject
            if (!forceEject)
                TryUpdateVisualState((uid, vendComponent));

            if (string.IsNullOrEmpty(vendComponent.NextItemToEject))
            {
                vendComponent.ThrowNextItem = false;
                return;
            }

            // Default spawn coordinates
            var xform = Transform(uid);
            var spawnCoordinates = xform.Coordinates;

            //Make sure the wallvends spawn outside of the wall.
            if (TryComp<WallMountComponent>(uid, out var wallMountComponent))
            {
                var offset = (wallMountComponent.Direction + xform.LocalRotation - Math.PI / 2).ToVec() * WallVendEjectDistanceFromWall;
                spawnCoordinates = spawnCoordinates.Offset(offset);
            }

            var ent = Spawn(vendComponent.NextItemToEject, spawnCoordinates);

            if (vendComponent.ThrowNextItem)
            {
                var range = vendComponent.NonLimitedEjectRange;
                var direction = new Vector2(_random.NextFloat(-range, range), _random.NextFloat(-range, range));
                _throwingSystem.TryThrow(ent, direction, vendComponent.NonLimitedEjectForce);
            }

            vendComponent.NextItemToEject = null;
            vendComponent.ThrowNextItem = false;
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var disabled = EntityQueryEnumerator<EmpDisabledComponent, VendingMachineComponent>();
            while (disabled.MoveNext(out var uid, out _, out var comp))
            {
                if (comp.NextEmpEject < Timing.CurTime)
                {
                    EjectRandom(uid, true, false, comp);
                    comp.NextEmpEject += (5 * comp.EjectDelay);
                }
            }
        }

        private void OnPriceCalculation(EntityUid uid, VendingMachineRestockComponent component, ref PriceCalculationEvent args)
        {
            List<double> priceSets = new();

            // Find the most expensive inventory and use that as the highest price.
            foreach (var vendingInventory in component.CanRestock)
            {
                double total = 0;

                if (PrototypeManager.TryIndex(vendingInventory, out VendingMachineInventoryPrototype? inventoryPrototype))
                {
                    foreach (var (item, amount) in inventoryPrototype.StartingInventory)
                    {
                        if (PrototypeManager.TryIndex(item, out EntityPrototype? entity))
                            total += _pricing.GetEstimatedPrice(entity) * amount;
                    }
                }

                priceSets.Add(total);
            }

            args.Price += priceSets.Max();
        }

        private void OnTryVocalize(Entity<VendingMachineComponent> ent, ref TryVocalizeEvent args)
        {
            args.Cancelled |= ent.Comp.Broken;
        }

        // ADT-Economy: Send UI state updates to the client
        protected override void UpdateUI(Entity<VendingMachineComponent?> entity)
        {
            if (!Resolve(entity, ref entity.Comp))
                return;

            var inventory = GetAllInventory(entity.Owner, entity.Comp);
            var state = new VendingMachineUpdateState(inventory, entity.Comp.PriceMultiplier, entity.Comp.Credits, entity.Comp.AllForFree);
            UISystem.SetUiState(entity.Owner, VendingMachineUiKey.Key, state);
        }

        private void UpdateUIWithBalance(Entity<VendingMachineComponent?> entity, EntityUid user)
        {
            if (!Resolve(entity, ref entity.Comp))
                return;

            var inventory = GetAllInventory(entity.Owner, entity.Comp);
            var playerBalance = GetPlayerBalance(user);
            var state = new VendingMachineUpdateState(inventory, entity.Comp.PriceMultiplier, entity.Comp.Credits, entity.Comp.AllForFree, playerBalance: playerBalance);
            UISystem.SetUiState(entity.Owner, VendingMachineUiKey.Key, state);
        }

        // ADT-Economy: Override to handle bank payment
        protected override void TryChargeAndVend(EntityUid uid, EntityUid sender, VendingMachineInventoryEntry entry, int count, VendingMachineComponent component)
        {
            // Calculate total price
            var totalPrice = (int)(entry.Price * component.PriceMultiplier * count);

            if (totalPrice <= 0)
            {
                // Free item, just vend
                for (int i = 0; i < count; i++)
                {
                    AuthorizedVend(uid, sender, entry.Type, entry.ID, component);
                }
                return;
            }

            // Play sound for paid items from server only (no client prediction)
            var emitSound = component.SoundVend;

            // Find the player's bank account via their ID card
            if (!_idCardSystem.TryFindIdCard(sender, out var idCard))
            {
                _popup.PopupEntity(Loc.GetString("vending-machine-no-id-card"), uid, sender, PopupType.Medium);
                Deny((uid, component));
                return;
            }

            if (!TryComp<BankCardComponent>(idCard.Owner, out var bankCard))
            {
                _popup.PopupEntity(Loc.GetString("vending-machine-no-bank-card"), uid, sender, PopupType.Medium);
                Deny((uid, component));
                return;
            }

            if (bankCard.AccountId == null)
            {
                _popup.PopupEntity(Loc.GetString("vending-machine-no-bank-account"), uid, sender, PopupType.Medium);
                Deny((uid, component));
                return;
            }

            // Try to charge the player's bank account
            if (!_bankCard.TryChangeBalance(bankCard.AccountId.Value, -totalPrice))
            {
                _popup.PopupEntity(Loc.GetString("vending-machine-insufficient-funds"), uid, sender, PopupType.Medium);
                Deny((uid, component));
                return;
            }

            // Payment successful, add credits to the machine
            component.Credits += totalPrice;
            Dirty(uid, component);

            // Vend the item(s) - suppress predicted sound since there's no client prediction
            for (int i = 0; i < count; i++)
            {
                AuthorizedVend(uid, sender, entry.Type, entry.ID, component, playSound: false);
            }

            // Play sound for all players (including the user who didn't predict)
            if (emitSound != null)
                Audio.PlayPvs(emitSound, uid);

            // Update UI with the player's new balance
            UpdateUIWithBalance((uid, component), sender);
        }
    }
}
