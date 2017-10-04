using Scripts.Model.SaveLoad;
using Scripts.Model.SaveLoad.SaveObjects;
using Scripts.Model.Spells;
using System.Collections.Generic;
using UnityEngine;
using System;
using Scripts.Model.Interfaces;
using Scripts.Model.Characters;

namespace Scripts.Model.Items {

    /// <summary>
    /// Abstract item class.
    /// </summary>
    /// <seealso cref="Scripts.Model.SaveLoad.ISaveable{Scripts.Model.SaveLoad.SaveObjects.ItemSave}" />
    /// <seealso cref="Scripts.Model.Interfaces.ISpellable" />
    public abstract class Item : ISaveable<ItemSave>, ISpellable {

        /// <summary>
        /// The item name
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The item icon
        /// </summary>
        public readonly Sprite Icon;

        /// <summary>
        /// The item's flavor text
        /// </summary>
        public readonly string Flavor;

        /// <summary>
        /// The targets this item can target
        /// </summary>
        public readonly TargetType Target;

        /// <summary>
        /// The base price of this item
        /// </summary>
        public readonly int BasePrice;

        /// <summary>
        /// The flags of this item.
        /// </summary>
        protected readonly HashSet<Flag> flags;

        /// <summary>
        /// Initializes a new instance of the <see cref="Item"/> class.
        /// </summary>
        /// <param name="icon">The item's icon.</param>
        /// <param name="basePrice">The item's base price.</param>
        /// <param name="target">The targets available to this item.</param>
        /// <param name="name">The name of this item.</param>
        /// <param name="description">The item's description.</param>
        public Item(Sprite icon, int basePrice, TargetType target, string name, string description) {
            this.flags = new HashSet<Flag>();
            this.BasePrice = basePrice;
            this.Target = target;
            this.Name = name;
            this.Icon = icon;
            this.Flavor = description;
        }

        /// <summary>
        /// Determines whether the specified flag exists for this item.
        /// </summary>
        /// <param name="flagToCheck">The flag to check.</param>
        /// <returns>
        ///   <c>true</c> if the flag exists; otherwise, <c>false</c>.
        /// </returns>
        public bool HasFlag(Flag flagToCheck) {
            return flags.Contains(flagToCheck);
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description {
            get {
                return DescriptionHelper;
            }
        }

        protected abstract string DescriptionHelper {
            get;
        }

        /// <summary>
        /// Determines whether this item is usable by the caster on target.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        ///   <c>true</c> if the item is usable; otherwise, <c>false</c>.
        /// </returns>
        public bool IsUsable(Character caster, Character target) {
            return caster.Stats.State == Characters.State.ALIVE && IsMeetOtherRequirements(caster, target);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) {
            Item item = obj as Item;
            if (item == null) {
                return false;
            }
            return GetType().Equals(item.GetType());
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() {
            return Name.GetHashCode() ^ Description.GetHashCode();
        }

        /// <summary>
        /// Determines whether other requirements for caster using this item on target are met.
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        ///   <c>true</c> if other requirements for casting using this item on target are met.; otherwise, <c>false</c>.
        /// </returns>
        protected abstract bool IsMeetOtherRequirements(Character caster, Character target);

        /// <summary>
        /// Gets the save object. A save object contains the neccessary
        /// information to initialize a clean class to its saved state.
        /// A save object is also serializable.
        /// </summary>
        /// <returns></returns>
        public ItemSave GetSaveObject() {
            return new ItemSave(GetType());
        }

        /// <summary>
        /// Initializes from save object.
        /// </summary>
        /// <param name="saveObject">The save object.</param>
        public void InitFromSaveObject(ItemSave saveObject) {
            // Nothing
        }

        /// <summary>
        /// Gets the spell book.
        /// </summary>
        /// <returns></returns>
        public abstract SpellBook GetSpellBook();
    }
}