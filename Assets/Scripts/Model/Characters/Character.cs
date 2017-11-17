using Scripts.Game.Defined.Serialized.Brains;
using Scripts.Game.Defined.SFXs;
using Scripts.Model.Interfaces;
using Scripts.Model.SaveLoad;
using Scripts.Model.SaveLoad.SaveObjects;
using Scripts.Model.TextBoxes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Model.Characters {

    /// <summary>
    /// Characters are special entities with
    /// Resources and Attributes, as well as numerous SpellFactories.
    ///
    /// They can participate in battles.
    /// </summary>
    public class Character : ISaveable<CharacterSave>, IIdNumberable, IAvatarable, IPortraitable {
        public const int UNKNOWN_ID = -1;

        /// <summary>
        /// Character's statistics.
        /// </summary>
        public readonly Stats Stats;

        /// <summary>
        /// Buffs currently on the character
        /// </summary>
        public readonly Buffs Buffs;

        /// <summary>
        /// Character's appearance
        /// </summary>
        public Look Look;

        /// <summary>
        /// Character's spells
        /// </summary>
        public readonly SpellBooks Spells;

        /// <summary>
        /// Character's user control / AI
        /// </summary>
        public Brain Brain;

        /// <summary>
        /// Character's inventory, typically shared reference
        /// </summary>
        public Inventory Inventory;

        /// <summary>
        /// Character's equipment.
        /// </summary>
        public readonly Equipment Equipment;

        /// <summary>
        /// The get icon position function
        /// </summary>
        public Func<RectTransform> GetIconRectFunc {
            set {
                getIconRectFunc = value;
            }
        }

        private Func<RectTransform> getIconRectFunc;

        /// <summary>
        /// The parent to effects function
        /// </summary>
        public Action<GameObject> ParentToEffectsFunc {
            set {
                this.parentToEffectsFunc = value;
            }
        }

        private Action<GameObject> parentToEffectsFunc;

        public Func<bool> IsPortraitAvailableFunc {
            set {
                this.isPortraitAvailableFunc = value;
            }
        }

        private Func<bool> isPortraitAvailableFunc;

        /// <summary>
        /// Character specific flags
        /// </summary>
        protected readonly HashSet<Flag> flags;

        /// <summary>
        /// Keeps track of how many characters have been created
        /// </summary>
        private static int idCounter;

        /// <summary>
        /// Unique id for this character.
        /// </summary>
        private int id;

        /// <summary>
        /// The effects queue. Save SFX until the portrait shows up.
        /// </summary>
        private Queue<GameObject> effectsQueue;

        private Vector2 originalPosition;
        private bool isOriginalPositionSet;

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="stats">Stats to use for this character</param>
        /// <param name="look">Look to use for this character</param>
        /// <param name="brain">Brain to use for this character</param>
        /// <param name="spells">Spells to use for this character</param>
        /// <param name="inventory">Inventory to use for this character</param>
        /// <param name="equipment">Equipment to use for this character</param>
        public Character(Stats stats, Look look, Brain brain, SpellBooks spells, Inventory inventory, Equipment equipment) {
            this.Stats = stats;
            this.Buffs = new Buffs(stats);
            this.Brain = brain;
            this.Look = look;
            this.Spells = spells;
            this.Inventory = inventory;
            this.Equipment = equipment;
            this.flags = new HashSet<Flag>();

            Brain.Spells = this.Spells;
            Stats.Update(this);
            Equipment.AddBuff = b => Buffs.AddBuff(b);
            Equipment.RemoveBuff = b => Buffs.RemoveEquipmentBuff(b);
            Stats.InitializeResources();
            Stats.GetFlatEquipmentBonus = f => Equipment.GetFlatStatBonus(f);
            stats.GetMultiplicativeBuffBonus = m => Buffs.GetMultiplicativeStatBonus(m);
            this.id = idCounter++;

            this.effectsQueue = new Queue<GameObject>();
            this.parentToEffectsFunc = (go) => effectsQueue.Enqueue(go);
            this.isPortraitAvailableFunc = () => false;
        }

        /// <summary>
        /// Inventoryless, equipmentless constructor
        /// </summary>
        /// <param name="stats">Stats to use for this character</param>
        /// <param name="look">Look to use for this character</param>
        /// <param name="brain">Brain to use for this character</param>
        /// <param name="spells">Spells to use for this character</param>
        public Character(Stats stats, Look look, Brain brain, SpellBooks spells) : this(stats, look, brain, spells, new Inventory(), new Equipment()) { }

        /// <summary>
        /// Spellless constructor
        /// </summary>
        /// <param name="stats">Stats to use for this character</param>
        /// <param name="look">Look to use for this character</param>
        /// <param name="brain">Brain to use for this character</param>
        public Character(Stats stats, Look look, Brain brain) : this(stats, look, brain, new SpellBooks(), new Inventory(), new Equipment()) { }

        /// <summary>
        /// Used in serialization for parties
        /// </summary>
        /// <param name="inventory">Reference to the shared inventory</param>
        public Character(Inventory inventory) : this(new Stats(), new Look(), new Player(), new SpellBooks(), inventory, new Equipment()) { }

        /// <summary>
        /// Get the unique id
        /// </summary>
        public int Id {
            get {
                return id;
            }
        }

        public Queue<GameObject> Effects {
            get {
                return effectsQueue;
            }
        }

        public Sprite Sprite {
            get {
                return Look.Sprite;
            }
        }

        public Color TextColor {
            get {
                return Look.TextColor;
            }
        }

        public RectTransform RectTransform {
            get {
                // Can't return the Func because it has to be a rectTransform
                if (getIconRectFunc == null) {
                    return null;
                }
                return getIconRectFunc();
            }
        }

        public bool IsPortraitAvailable {
            get {
                return isPortraitAvailableFunc();
            }
        }

        public bool CanAffordCost(ICostable cost, int amount) {
            return cost.CanAfford(amount, this);
        }

        public void ConsumeCosts(ICostable cost, int amount) {
            Util.Assert(CanAffordCost(cost, amount));
            cost.DeductCostFromCharacter(amount, this);
        }

        /// <summary>
        /// Stats are updated each tick to ensure dependent stats
        /// are updated
        /// </summary>
        public void Update() {
            Stats.Update(this);
        }

        /// <summary>
        /// Check all fields for equality. (excluding ID)
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <returns>True if equal</returns>
        public override bool Equals(object obj) {
            var item = obj as Character;

            if (item == null) {
                return false;
            }

            return this.Stats.Equals(item.Stats)
                && this.Buffs.Equals(item.Buffs)
                && this.Look.Equals(item.Look)
                && this.Spells.Equals(item.Spells)
                && this.Brain.Equals(item.Brain)
                && this.Inventory.Equals(item.Inventory)
                && this.flags.SetEquals(item.flags)
                && this.Equipment.Equals(item.Equipment);
        }

        /// <summary>
        /// Add a flag
        /// </summary>
        /// <param name="flagToAdd">Flag to add</param>
        public void AddFlag(Flag flagToAdd) {
            flags.Add(flagToAdd);
        }

        public void RemoveFlag(Flag flagToRemove) {
            flags.Remove(flagToRemove);
        }

        /// <summary>
        /// Determines whether the character has the specified flag.
        /// </summary>
        /// <param name="flagToCheck">The flag to check.</param>
        /// <returns>
        ///   <c>true</c> if the specified f has flag; otherwise, <c>false</c>.
        /// </returns>
        public bool HasFlag(Flag flagToCheck) {
            return this.flags.Contains(flagToCheck);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() {
            return 0;
        }

        /// <summary>
        /// Gets the save object.
        /// </summary>
        /// <returns>A serializable character save object</returns>
        public CharacterSave GetSaveObject() {
            return new CharacterSave(
                this.id,
                flags.ToList(),
                Stats.GetSaveObject(),
                Buffs.GetSaveObject(),
                Look.GetSaveObject(),
                Spells.GetSaveObject(),
                Brain.GetSaveObject(),
                Equipment.GetSaveObject()
                );
        }

        /// <summary>
        /// Initializes from save object.
        /// </summary>
        /// <param name="saveObject">The save object to initialize from.</param>
        public void InitFromSaveObject(CharacterSave saveObject) {
            // No special classes - set fields
            foreach (Flag flag in saveObject.Flags) {
                flags.Add(flag);
            }
            this.Stats.InitFromSaveObject(saveObject.Stats);

            this.Look.InitFromSaveObject(saveObject.Look);
            this.Spells.InitFromSaveObject(saveObject.Spells);
            // No equipment setup here!

            // Must replace class
            Brain brain = saveObject.Brain.CreateObjectFromID();
            this.Brain = brain;

            // Buffs and inventory must be setup in Party!
        }

        public override string ToString() {
            return Look.DisplayName;
        }

        public void ParentToEffects(GameObject go) {
            parentToEffectsFunc(go);
        }
    }
}