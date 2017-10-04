using Scripts.Model.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Scripts.Model.Stats;
using Scripts.Model.Buffs;
using Scripts.Model.Spells;
using Scripts.Presenter;
using Scripts.Model.Interfaces;
using Scripts.Model.SaveLoad;
using Scripts.Model.SaveLoad.SaveObjects;

namespace Scripts.Model.Characters {

    /// <summary>
    /// Represents the equipment equipped on a character.
    /// </summary>
    /// <seealso cref="System.Collections.Generic.IEnumerable{Scripts.Model.Items.Item}" />
    /// <seealso cref="System.Collections.Generic.IEnumerable{Scripts.Model.Items.EquippableItem}" />
    /// <seealso cref="System.Collections.Generic.IEnumerable{Scripts.Model.Interfaces.ISpellable}" />
    /// <seealso cref="System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{Scripts.Model.Items.EquipType, Scripts.Model.Items.EquippableItem}}" />
    /// <seealso cref="System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{Scripts.Model.Items.EquipType, Scripts.Model.Buffs.Buff}}" />
    /// <seealso cref="System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{Scripts.Model.Stats.StatType, System.Int32}}" />
    /// <seealso cref="Scripts.Model.SaveLoad.ISaveable{Scripts.Model.SaveLoad.SaveObjects.EquipmentSave}" />
    public class Equipment
        :
            IEnumerable<Item>,
            IEnumerable<EquippableItem>,
            IEnumerable<ISpellable>,
            IEnumerable<KeyValuePair<EquipType, EquippableItem>>,
            IEnumerable<KeyValuePair<EquipType, Buff>>,
            IEnumerable<KeyValuePair<StatType, int>>,
            ISaveable<EquipmentSave> {
        public Action<Buff> AddBuff;
        public Action<Buff> RemoveBuff;
        public Action<SplatDetails> AddSplat;

        /// <summary>
        /// Equipment internal dictionary
        /// </summary>
        private readonly IDictionary<EquipType, EquippableItem> equipped;

        /// <summary>
        /// Buffs associated with equipment
        /// </summary>
        private readonly IDictionary<EquipType, Buff> itemBuffs;

        /// <summary>
        /// The stat bonuses from the equipment.
        /// </summary>
        private readonly IDictionary<StatType, int> statBonuses;

        // Temporary serialization fields
        private bool IsSetupDone;

        private List<Character> partyMembers;

        /// <summary>
        /// Initializes a new instance of the <see cref="Equipment"/> class.
        /// </summary>
        public Equipment() {
            this.equipped = new Dictionary<EquipType, EquippableItem>();
            this.statBonuses = new Dictionary<StatType, int>();
            this.AddBuff = ((a) => { });
            this.RemoveBuff = ((a) => { });
            foreach (StatType s in StatType.ASSIGNABLES) {
                statBonuses.Add(s, 0);
            }
            this.itemBuffs = new Dictionary<EquipType, Buff>();
            this.AddSplat = ((p) => { });
        }

        /// <summary>
        /// Adds a piece of equipment to this.
        /// </summary>
        /// <param name="inventory">The inventory to take the item from.</param>
        /// <param name="caster">The one equipping the equip onto this equipment.</param>
        /// <param name="equippableItem">The equippable item to equip.</param>
        public void AddEquip(Inventory inventory, BuffParams caster, EquippableItem equippableItem) {
            inventory.Remove(equippableItem);
            if (Contains(equippableItem.Type)) {
                RemoveEquip(inventory, equippableItem.Type);
            }

            Buff buff = equippableItem.CreateBuff();
            if (buff != null) {
                buff.Caster = caster;
                itemBuffs.Add(equippableItem.Type, buff);
                AddBuff(buff);
            }

            equipped.Add(equippableItem.Type, equippableItem);
            foreach (KeyValuePair<StatType, int> pair in equippableItem.StatBonuses) {
                Util.Assert(StatType.ASSIGNABLES.Contains(pair.Key), "Invalid stat type on equipment.");
                this.statBonuses[pair.Key] += pair.Value;
                AddSplat(new SplatDetails(pair.Key, pair.Value));
            }
        }

        /// <summary>
        /// Removes a piece of equipment.
        /// </summary>
        /// <param name="inventory">The inventory to place the removed item into.</param>
        /// <param name="type">The type of equipment to remove.</param>
        public void RemoveEquip(Inventory inventory, EquipType type) {
            Util.Assert(equipped.ContainsKey(type), "No equipment in slot.");
            EquippableItem itemToRemove = equipped[type];

            if (itemBuffs.ContainsKey(itemToRemove.Type)) {
                Buff buffToRemove = itemBuffs[itemToRemove.Type];
                itemBuffs.Remove(itemToRemove.Type);
                RemoveBuff(buffToRemove);
            }

            inventory.Add(itemToRemove);
            equipped.Remove(itemToRemove.Type);

            foreach (KeyValuePair<StatType, int> pair in itemToRemove.StatBonuses) {
                Util.Assert(StatType.ASSIGNABLES.Contains(pair.Key), "Invalid stat type on equipment.");
                int amountToRestore = -pair.Value;
                this.statBonuses[pair.Key] += amountToRestore;
                AddSplat(new SplatDetails(pair.Key, amountToRestore));
            }
        }

        /// <summary>
        /// Peeks at the item in a particular slot
        /// </summary>
        /// <param name="type">The type of equipment to peek.</param>
        /// <returns>The equippable item, if any, in that slot</returns>
        public EquippableItem PeekItem(EquipType type) {
            return equipped[type];
        }

        /// <summary>
        /// Gets the current equipment bonus.
        /// </summary>
        /// <param name="type">The type of stat to get the bonus for.</param>
        /// <returns>The equipment bonus for type</returns>
        public int GetFlatStatBonus(StatType type) {
            return statBonuses.ContainsKey(type) ? statBonuses[type] : 0;
        }

        /// <summary>
        /// Determines whether a slot is occupied.
        /// </summary>
        /// <param name="type">The type whose slot to check.</param>
        /// <returns>
        ///   <c>true</c> if the slot is occupied; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(EquipType type) {
            return equipped.ContainsKey(type);
        }

        /// <summary>
        /// Check equipment, buffs, and stat bonuses.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) {
            var item = obj as Equipment;

            if (item == null) {
                return false;
            }

            return Util.IsDictionariesEqual(this.equipped, item.equipped)
                && Util.IsDictionariesEqual(this.itemBuffs, item.itemBuffs)
                && Util.IsDictionariesEqual(this.statBonuses, item.statBonuses);
        }

        /// <summary>
        /// No hashable content
        /// </summary>
        /// <returns>
        /// 0
        /// </returns>
        public override int GetHashCode() {
            return 0;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return equipped.Values.GetEnumerator();
        }

        IEnumerator<EquippableItem> IEnumerable<EquippableItem>.GetEnumerator() {
            return equipped.Values.GetEnumerator();
        }

        IEnumerator<ISpellable> IEnumerable<ISpellable>.GetEnumerator() {
            return equipped.Values.Select(e => e.GetSpellBook()).Cast<ISpellable>().ToList().GetEnumerator();
        }

        /// <summary>
        /// Gets the save object.
        /// </summary>
        /// <returns></returns>
        public EquipmentSave GetSaveObject() {
            List<EquipItemSave> equips = new List<EquipItemSave>();
            foreach (ISaveable<EquipItemSave> eq in equipped.Values) {
                equips.Add(eq.GetSaveObject());
            }

            List<EquipmentSave.EquipBonus> bonuses = new List<EquipmentSave.EquipBonus>();
            foreach (KeyValuePair<StatType, int> pair in statBonuses) {
                bonuses.Add(new EquipmentSave.EquipBonus() { Stat = new StatTypeSave(pair.Key), Bonus = pair.Value });
            }

            List<EquipmentSave.EquipBuff> buffs = new List<EquipmentSave.EquipBuff>();
            foreach (KeyValuePair<EquipType, Buff> pair in itemBuffs) {
                buffs.Add(new EquipmentSave.EquipBuff() {
                    EquipType = new EquipTypeSave(pair.Key),
                    Buff = new BuffSave(
                        pair.Value.TurnsRemaining,
                        pair.Value.BuffCaster.GetSaveObject(),
                        pair.Value.CasterId, pair.Value.GetType())
                });
            }
            return new EquipmentSave(equips, buffs, bonuses);
        }

        /// <summary>
        /// Initializes from save object.
        /// </summary>
        /// <param name="saveObject">The save object.</param>
        public void InitFromSaveObject(EquipmentSave saveObject) {
            foreach (EquipItemSave save in saveObject.Equipped) {
                EquippableItem eq = save.CreateObjectFromID();
                eq.InitFromSaveObject(save);
                equipped.Add(save.EquipTypeSave.Restore(), eq);
            }

            statBonuses.Clear();
            foreach (EquipmentSave.EquipBonus bonus in saveObject.Bonuses) {
                StatType type = bonus.Stat.Restore();
                int count = bonus.Bonus;
                this.statBonuses.Add(type, count);
            }

            foreach (EquipmentSave.EquipBuff eb in saveObject.Buffs) {
                BuffSave bs = eb.Buff;
                EquipType et = eb.EquipType.Restore();
                Buff buff = CharacterBuffsSave.SetupBuffCasterFromSave(bs, partyMembers);
                itemBuffs.Add(et, buff);
            }

            partyMembers = null;
        }

        /// <summary>
        /// Set up the temporary save fields.
        /// </summary>
        /// <param name="partyMembers">The party members.</param>
        public void SetupTemporarySaveFields(List<Character> partyMembers) {
            Util.Assert(!IsSetupDone, "Only callable when restoring Equipment in Party.");
            this.partyMembers = partyMembers;
            IsSetupDone = true;
        }

        IEnumerator<KeyValuePair<EquipType, EquippableItem>> IEnumerable<KeyValuePair<EquipType, EquippableItem>>.GetEnumerator() {
            return equipped.GetEnumerator();
        }

        IEnumerator<KeyValuePair<EquipType, Buff>> IEnumerable<KeyValuePair<EquipType, Buff>>.GetEnumerator() {
            return itemBuffs.GetEnumerator();
        }

        IEnumerator<KeyValuePair<StatType, int>> IEnumerable<KeyValuePair<StatType, int>>.GetEnumerator() {
            return statBonuses.GetEnumerator();
        }

        IEnumerator<Item> IEnumerable<Item>.GetEnumerator() {
            return equipped.Cast<Item>().GetEnumerator();
        }
    }
}