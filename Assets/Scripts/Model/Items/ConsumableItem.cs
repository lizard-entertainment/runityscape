using Scripts.Model.Spells;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Scripts.Game.Defined.Spells;
using Scripts.Game.Defined.Serialized.Spells;
using Scripts.Model.Characters;

namespace Scripts.Model.Items {

    /// <summary>
    /// Items that are used up when used.
    /// </summary>
    /// <seealso cref="Scripts.Model.Items.UseableItem" />
    public abstract class ConsumableItem : UseableItem {

        /// <summary>
        /// Item spellbook associated with this item.
        /// </summary>
        private readonly SpellBook book;

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
            this.book = new UseItem(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsumableItem"/> class.
        /// </summary>
        /// <param name="basePrice">The base price for this item.</param>
        /// <param name="target">The types of characters this item can target.</param>
        /// <param name="name">The name of this item<param>
        /// <param name="description">The description of this item.</param>
        public ConsumableItem(int basePrice, TargetType target, string name, string description)
            : this(DEFAULT_SPRITE, basePrice, target, name, description) { }

        /// <summary>
        /// Gets the spell book.
        /// </summary>
        /// <returns></returns>
        public sealed override SpellBook GetSpellBook() {
            return book;
        }

        /// <summary>
        /// Gets the spelleffects of this item.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public abstract IList<SpellEffect> GetEffects(Character caster, Character target);

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

        protected sealed override string DescriptionHelper {
            get {
                return string.Format("Consumable\n{0}", Flavor);
            }
        }
    }
}