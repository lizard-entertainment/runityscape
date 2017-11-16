using Scripts.Game.Defined.Spells;
using Scripts.Model.Characters;
using Scripts.Model.Spells;
using System.Collections.Generic;
using Scripts.View.Portraits;
using System.Collections;
using Scripts.Game.Defined.SFXs;
using Scripts.Model.Pages;
using System;

namespace Scripts.Model.Items {

    /// <summary>
    /// Equips the item
    /// </summary>
    /// <seealso cref="Scripts.Model.Spells.ItemSpellBook" />
    public class CastEquipItem : ItemSpellBook {
        private EquippableItem equip;

        /// <summary>
        /// Initializes a new instance of the <see cref="CastEquipItem"/> class.
        /// </summary>
        /// <param name="itemToEquip">The item to equip.</param>
        public CastEquipItem(EquippableItem itemToEquip) : base(itemToEquip, itemToEquip.Target, itemToEquip.Name, "Equip") {
            this.equip = itemToEquip;
            this.flags.Add(Spells.Flag.USABLE_OUT_OF_COMBAT);
        }

        /// <summary>
        /// Can this item be equipped on target?
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        ///   <c>true</c> if [is meet other cast requirements2] [the specified caster]; otherwise, <c>false</c>.
        /// </returns>
        protected override bool IsMeetItemCastRequirements(Character caster, Character target) {
            return caster.Inventory.HasItem(equip) && (!target.Equipment.Contains(equip.Type) || caster.Inventory.IsAddable(target.Equipment.PeekItem(equip.Type)));
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                    new EquipItemEffect(new EquipParams(caster.Inventory, target.Equipment, equip), new Buffs.BuffParams(target.Stats, target.Id))
                };
        }
    }

    /// <summary>
    /// Unequip item spell.
    /// </summary>
    /// <seealso cref="Scripts.Model.Spells.ItemSpellBook" />
    public class CastUnequipItem : ItemSpellBook {
        private Inventory caster;
        private Equipment targetEquipment;
        private new EquippableItem item;

        /// <summary>
        /// Initializes a new instance of the <see cref="CastUnequipItem"/> class.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="targetEquipment">The target's equipment.</param>
        /// <param name="item">The item.</param>
        public CastUnequipItem(Inventory caster, Equipment targetEquipment, EquippableItem item) : base(item, item.Target, item.Name, "Unequip") {
            this.caster = caster;
            this.targetEquipment = targetEquipment;
            this.item = item;
            this.flags.Add(Spells.Flag.USABLE_OUT_OF_COMBAT);
        }

        /// <summary>
        /// Can we unequip this item?
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        ///   <c>true</c> if [is meet other cast requirements2] [the specified caster]; otherwise, <c>false</c>.
        /// </returns>
        protected override bool IsMeetItemCastRequirements(Character caster, Character target) {
            return target.Equipment.Contains(item.Type) && caster.Inventory.IsAddable(item);
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                    new UnequipItemEffect(new EquipParams(caster.Inventory, target.Equipment, item))
                };
        }
    }

    /// <summary>
    /// Dummy class for unusable items eg basic items
    /// </summary>
    /// <seealso cref="Scripts.Model.Spells.ItemSpellBook" />
    public class Dummy : ItemSpellBook {

        public Dummy(BasicItem basic) : base(basic, basic.Target, string.Empty, string.Empty) {
        }

        protected override string CreateDescriptionHelper() {
            return string.Format("{0}", item.Description);
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            return new SpellEffect[0];
        }
    }

    /// <summary>
    /// Consumable item spell
    /// </summary>
    /// <seealso cref="Scripts.Model.Spells.ItemSpellBook" />
    public class UseItem : ItemSpellBook {
        private readonly ConsumableItem consume;

        public UseItem(ConsumableItem consume) : base(consume, consume.Target, consume.Name, "Use") {
            this.consume = consume;
            if (consume.HasFlag(Flag.USABLE_OUT_OF_COMBAT)) {
                this.flags.Add(Spells.Flag.USABLE_OUT_OF_COMBAT);
            } else {
                this.flags.Remove(Spells.Flag.USABLE_OUT_OF_COMBAT);
            }
            AddCost(consume, 1);
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            return consume.GetEffects(page, caster, target);
        }

        protected override IList<IEnumerator> GetHitSFX(Character caster, Character target) {
            return new IEnumerator[] { SFX.PlaySound("healspell1") };
        }
    }

    /// <summary>
    /// Throw the item away.
    /// </summary>
    /// <seealso cref="Scripts.Model.Spells.ItemSpellBook" />
    public class TossItem : ItemSpellBook {
        private readonly Inventory inventory;

        public TossItem(Item item, Inventory inventory) : base(item, TargetType.SELF, item.Name, "Dispose") {
            flags.Add(Spells.Flag.USABLE_OUT_OF_COMBAT);
            this.inventory = inventory;
        }

        protected override IList<SpellEffect> GetHitEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                new RemoveItemEffect(item, target.Inventory)
            };
        }

        protected override bool IsMeetItemCastRequirements(Character caster, Character target) {
            return caster.Inventory.HasItem(item);
        }

        protected sealed override bool IsOverride() {
            return true;
        }
    }
}