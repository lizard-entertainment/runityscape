using Scripts.Model.Items;
using System;
using System.Collections.Generic;
using UnityEngine;
using Scripts.Model.Characters;

namespace Scripts.Model.Spells {

    /// <summary>
    /// Unserialized Spellbooks associated with item usage.
    /// </summary>
    /// <seealso cref="Scripts.Model.Spells.SpellBook" />
    public abstract class ItemSpellBook : SpellBook {
        protected Item item;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemSpellBook"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="verb">The verb. Caster [Verb] ItemName on Target!</param>
        public ItemSpellBook(Item item, string verb) : this(item, item.Name, verb) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemSpellBook"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="name">The item name.</param>
        /// <param name="verb">The verb. Caster [Verb] ItemName on Target!</param>
        public ItemSpellBook(Item item, string name, string verb) : base(name, item.Icon, item.Target, SpellType.ITEM, 0, 0, verb) {
            this.item = item;
            flags.Remove(Flag.CASTER_REQUIRES_SPELL);
        }

        /// <summary>
        /// Gets a detailed name.
        /// Detailed names show the count of the item, and are colored differently based on castability.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <returns></returns>
        public override string GetDetailedName(Character caster) {
            int count = caster.Inventory.GetCount(item);

            Color color = Color.white;
            if (!IsMeetPreTargetRequirements(caster.Stats)) {
                color = Color.grey;
            }

            return caster.Inventory.CountedItemName(item);
        }

        protected override string CreateDescriptionHelper() {
            return item.Description;
        }

        /// <summary>
        /// Determines whether is meet other cast requirements
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        ///   <c>true</c> if [is meet other cast requirements] [the specified caster]; otherwise, <c>false</c>.
        /// </returns>
        protected override bool IsMeetOtherCastRequirements(Character caster, Character target) {
            return item.IsUsable(caster, target) && IsMeetItemCastRequirements(caster, target);
        }

        /// <summary>
        /// Determines whether is meet other cast requirements, other requirements.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        ///   <c>true</c> if [is meet other cast requirements2] [the specified caster]; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool IsMeetItemCastRequirements(Character caster, Character target) {
            return true;
        }
    }
}