using Scripts.Model.Spells;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Scripts.Game.Defined.Spells;
using Scripts.Game.Defined.Serialized.Spells;
using Scripts.Model.Characters;
using Scripts.Model.Pages;
using Scripts.Model.Interfaces;

namespace Scripts.Model.Items {

    /// <summary>
    /// Items that are used up when used.
    /// </summary>
    /// <seealso cref="Scripts.Model.Items.UseableItem" />
    public abstract class ConsumableItem : UseableItem, ICostable {

        /// <summary>
        /// Item spellbook associated with this item.
        /// </summary>
        private SpellBook book;

        /// <summary>
        /// The default sprite if one is not used
        /// </summary>
        private static readonly Sprite DEFAULT_SPRITE = Util.GetSprite("shiny-apple");

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsumableItem"/> class.
        /// </summary>
        /// <param name="sprite">The sprite for this item.</param>
        /// <param name="basePrice">The base price for this item.</param>
        /// <param name="target">The types of characters this item can target.</param>
        /// <param name="name">The name of this item<param>
        /// <param name="description">The description of this item.</param>
        public ConsumableItem(Sprite sprite, int basePrice, TargetType target, string name, string description)
            : base(sprite, basePrice, target, name, description) {
        }

        public ConsumableItem(string spriteLoc, int basePrice, TargetType target, string name, string description)
            : this(Util.GetSprite(spriteLoc), basePrice, target, name, description) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsumableItem"/> class.
        /// </summary>
        /// <param name="basePrice">The base price for this item.</param>
        /// <param name="target">The types of characters this item can target.</param>
        /// <param name="name">The name of this item<param>
        /// <param name="description">The description of this item.</param>
        private ConsumableItem(int basePrice, TargetType target, string name, string description)
            : this(DEFAULT_SPRITE, basePrice, target, name, description) { }

        /// <summary>
        /// Gets the spell book.
        /// </summary>
        /// <returns></returns>
        public sealed override SpellBook GetSpellBook() {
            if (book == null) {
                book = new UseItem(this);
            }
            return book;
        }

        /// <summary>
        /// Gets the spelleffects of this item.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public abstract IList<SpellEffect> GetEffects(Page page, Character caster, Character target);

        /// <summary>
        /// Determines whether having the caster use an item on a target meets particular requirements.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        ///   <c>true</c> if caster can use the item on target; otherwise, <c>false</c>.
        /// </returns>
        protected override bool IsMeetOtherRequirements(Character caster, Character target) {
            return target.Stats.State == State.ALIVE;
        }

        public string GetName() {
            return this.Name;
        }

        public bool CanAfford(int amount, Character characterToCheck) {
            Util.Assert(amount > 0, "Amount must be nonnegative.");
            return characterToCheck.Inventory.HasItem(this, amount);
        }

        public void DeductCostFromCharacter(int amount, Character unitToDeductFrom) {
            Util.Assert(CanAfford(amount, unitToDeductFrom));
            unitToDeductFrom.Inventory.Remove(this, amount);
        }

        protected sealed override string DescriptionHelper {
            get {
                return string.Format("Consumable\n{0}", Flavor);
            }
        }
    }
}