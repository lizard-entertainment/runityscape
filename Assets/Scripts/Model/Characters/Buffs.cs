using Scripts.Model.Buffs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using Scripts.Model.SaveLoad.SaveObjects;
using Scripts.Model.SaveLoad;
using Scripts.Model.Stats;

namespace Scripts.Model.Characters {

    /// <summary>
    /// Describes the type of removal for a buff
    /// </summary>
    public enum RemovalType {
        TIMED_OUT, // Buff ran out of time
        DISPEL, // Buff is being prematurely dispelled
    }

    /// <summary>
    /// Represents Buffs on a certain character.
    /// </summary>
    public class Buffs : IEnumerable<Buff>, ISaveable<CharacterBuffsSave> {

        /// <summary>
        /// Adds a splat detailing something about the buff.
        /// </summary>
        public Action<SplatDetails> AddSplat;

        /// <summary>
        /// Set of buffs. Used to avoid duplicates.
        /// </summary>
        private HashSet<Buff> set;

        /// <summary>
        /// Stats of the owner character.
        /// </summary>
        private Stats ownerStats;

        /// <summary>
        /// The percentage stat bonuses. Int is used to avoid rounding errors.
        /// 1 = 1% multiplier, -1 = 99% multiplier.
        /// </summary>
        private IDictionary<StatType, int> percentageStatBonuses;

        // Temporary fields used in serializable, will be null otherwise
        private List<Character> partyMembers;

        private bool isSetupTempFieldsBefore;

        public Buffs(Stats ownerStats) {
            this.ownerStats = ownerStats;
            SetupStatBonusDict();
            set = new HashSet<Buff>(new IdNumberEqualityComparer<Buff>());
            this.AddSplat = (a => { });
        }

        /// <summary>
        /// Adds a buff to this character.
        /// </summary>
        /// <param name="buff">Buff to add.</param>
        /// <param name="caster">Buff's caster.</param>
        public void AddBuff(Buff buff, Character caster) {
            buff.Caster = new BuffParams(caster.Stats, caster.Id);
            AddBuff(buff);
        }

        /// <summary>
        /// No caster variant. Buff already has caster set.
        /// </summary>
        /// <param name="buff">Buff to add.</param>
        public void AddBuff(Buff buff) {
            Util.Assert(buff.BuffCaster != null, "Buff's caster is null.");
            buff.OnApply(ownerStats);
            set.Add(buff);
            foreach (KeyValuePair<StatType, int> statBonus in buff) {
                AddStatBonus(statBonus.Key, statBonus.Value);
                AddSplat(new SplatDetails(statBonus.Key, statBonus.Value));
            }
            AddSplat(new SplatDetails(Color.green, string.Format("+"), buff.Sprite));
        }

        /// <summary>
        /// Removes a buff by various means.
        /// </summary>
        /// <param name="type">Type of removal.</param>
        /// <param name="buff">Buff to remove.</param>
        public void RemoveBuff(RemovalType type, Buff buff) {
            switch (type) {
                case RemovalType.TIMED_OUT:
                    buff.OnTimeOut(ownerStats);
                    break;

                case RemovalType.DISPEL:
                    buff.OnDispell(ownerStats);
                    break;

                default:
                    Util.Assert(false, "Unknown removal type: " + type);
                    break;
            }

            // Remove if possible
            if (buff.IsDispellable || type == RemovalType.TIMED_OUT) {
                foreach (KeyValuePair<StatType, int> statBonus in buff) {
                    int amountToRecover = -statBonus.Value;
                    AddStatBonus(statBonus.Key, amountToRecover);
                    AddSplat(new SplatDetails(statBonus.Key, amountToRecover));
                }
                set.Remove(buff);
                AddSplat(new SplatDetails(Color.red, string.Format("-"), buff.Sprite));
            }
        }

        /// <summary>
        /// Dispels all buffs.
        /// </summary>
        public void DispelAllBuffs() {
            Buff[] allBuffs = set.ToArray();
            foreach (Buff buff in allBuffs) {
                RemoveBuff(RemovalType.DISPEL, buff);
            }
        }

        /// <summary>
        /// Does the character have a buff of this type?
        /// </summary>
        /// <typeparam name="T">Type of buff to find</typeparam>
        /// <returns>True if character has this kind of buff.</returns>
        public bool HasBuff<T>() where T : Buff {
            return set.Any(b => b is T);
        }

        IEnumerator<Buff> IEnumerable<Buff>.GetEnumerator() {
            return set.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return set.GetEnumerator();
        }

        /// <summary>
        /// Serialize all the buffs being held.
        /// </summary>
        /// <returns>Serializable object with all the buffs being held.</returns>
        public CharacterBuffsSave GetSaveObject() {
            return new CharacterBuffsSave(
                set.Select(b => b.GetSaveObject()).ToList(),
                percentageStatBonuses.Select(statBonus => new StatBonusSave(statBonus.Key.GetSaveObject(), statBonus.Value)).ToList()
                );
        }

        /// <summary>
        /// Check set equality
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <returns>True if sets have equal buffs and stats.</returns>
        public override bool Equals(object obj) {
            var item = obj as Buffs;

            if (item == null) {
                return false;
            }

            return
                new HashSet<Buff>(set).SetEquals(new HashSet<Buff>(item.set))
                && Util.IsDictionariesEqual<StatType, int>(percentageStatBonuses, item.percentageStatBonuses);
        }

        /// <summary>
        /// No hash possible
        /// </summary>
        /// <returns>0</returns>
        public override int GetHashCode() {
            return 0;
        }

        /// <summary>
        /// Initialize by loading buffs and readding them.
        /// </summary>
        /// <param name="saveObject">Serializable buffs object.</param>
        public void InitFromSaveObject(CharacterBuffsSave saveObject) {
            foreach (BuffSave bs in saveObject.BuffSaves) {
                set.Add(CharacterBuffsSave.SetupBuffCasterFromSave(bs, partyMembers));
            }
            foreach (StatBonusSave statSave in saveObject.StatBonusSaves) {
                percentageStatBonuses[statSave.StatType.Restore()] = statSave.Bonus;
            }
            partyMembers = null;
        }

        /// <summary>
        /// Gets the stat multiplier.
        /// </summary>
        /// <param name="type">The type of stat to get.</param>
        /// <returns>
        /// Gets the stat multiplier as an int to avoid rounding issues.
        ///
        /// Stat should be affected as follows:
        /// Stat * (100 + MultiplicativeBonus) / 100
        /// </returns>
        public int GetMultiplicativeStatBonus(StatType type) {
            int amount = 0;
            if (percentageStatBonuses.ContainsKey(type)) {
                amount = percentageStatBonuses[type];
            }
            return amount;
        }

        /// <summary>
        /// Setup party members for re-referencing after loading
        /// </summary>
        /// <param name="partyMembers">Party members in this particular game.</param>
        public void SetupTemporarySaveFields(List<Character> partyMembers) {
            Util.Assert(!isSetupTempFieldsBefore, "This function is only callable once when the object is being loaded.");
            this.partyMembers = partyMembers;
            isSetupTempFieldsBefore = true;
        }

        private void SetupStatBonusDict() {
            this.percentageStatBonuses = new Dictionary<StatType, int>();
            foreach (StatType assignable in StatType.ASSIGNABLES) {
                percentageStatBonuses[assignable] = 0;
            }
        }

        private void AddStatBonus(StatType type, int amount) {
            Util.Assert(percentageStatBonuses.ContainsKey(type), "Non-assignable type: " + type);
            Util.Assert(amount != 0, "Must add a non-zero amount.");
            percentageStatBonuses[type] += amount;
        }
    }
}