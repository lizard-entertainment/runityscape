using Scripts.Game.Defined.Serialized.Statistics;
using Scripts.Model.SaveLoad;
using Scripts.Model.SaveLoad.SaveObjects;
using Scripts.Model.Stats;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Model.Characters {

    /// <summary>
    /// A character's stats.
    /// </summary>
    /// <seealso cref="System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{Scripts.Model.Stats.StatType, Scripts.Model.Stats.Stat}}" />
    /// <seealso cref="Scripts.Model.SaveLoad.ISaveable{Scripts.Model.SaveLoad.SaveObjects.CharacterStatsSave}" />
    /// <seealso cref="System.IComparable{Scripts.Model.Characters.Stats}" />
    public class Stats : IEnumerable<KeyValuePair<StatType, Stat>>, ISaveable<CharacterStatsSave>, IComparable<Stats> {
        public const int NUMBER_OF_CHARS_IN_SHORT_STAT_NAME = 3;

        /// <summary>
        ///
        /// </summary>
        public enum Set {

            /// <summary>
            /// The mod--Set, upper bound by the max value.
            /// </summary>
            MOD,

            /// <summary>
            /// The mod unbound--Set while ignoring the max value
            /// </summary>
            MOD_UNBOUND,

            /// <summary>
            /// The maximum
            /// </summary>
            MAX
        }

        /// <summary>
        /// Various parts of a stat that can be gotten.
        /// </summary>
        public enum Get {

            /// <summary>
            /// The mod
            /// </summary>
            MOD,

            /// <summary>
            /// Mod + equip + buff bonus
            /// </summary>
            TOTAL,

            /// <summary>
            /// The maximum
            /// </summary>
            MAX
        }

        /// <summary>
        /// Minimum amount a stat is restored by.
        /// </summary>
        private const int MINIMUM_RESTORE_AMOUNT = 1;

        /// <summary>
        /// Gets the FLAT stat bonus from the equipment.
        /// </summary>
        public Func<StatType, int> GetFlatEquipmentBonus;

        /// <summary>
        /// Gets the MULTIPLICATIVE stat bonus from the buffs.
        /// </summary>
        public Func<StatType, int> GetMultiplicativeBuffBonus;

        /// <summary>
        /// Add a splat function
        /// </summary>
        public Action<SplatDetails> AddSplat;

        /// <summary>
        /// The base stats dictionary
        /// </summary>
        private readonly IDictionary<StatType, Stat> baseStats;

        /// <summary>
        /// The level
        /// </summary>
        public int Level;

        /// <summary>
        /// The stat points
        /// </summary>
        public int UnassignedStatPoints;

        /// <summary>
        /// The resource visibility
        /// </summary>
        private int resourceVisibility;

        // Temporary fields for initing
        /// <summary>
        /// if is true, then the stats don't belong to a party member and are faked.
        /// There is no character owning the stats.
        /// </summary>
        private bool isSpoofed;

        /// <summary>
        /// If true, then initialization has finished.
        /// </summary>
        private bool isDoneIniting;

        /// <summary>
        /// Initializes a new instance of the <see cref="Stats"/> class.
        /// </summary>
        public Stats() {
            this.baseStats = new Dictionary<StatType, Stat>();
            this.AddSplat = (a => { });
            SetDefaultStats();
            SetDefaultResources();
            GetFlatEquipmentBonus = (st) => 0;
            GetMultiplicativeBuffBonus = (st) => 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stats"/> class.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="str">The string.</param>
        /// <param name="agi">The agi.</param>
        /// <param name="intel">The intel.</param>
        /// <param name="vit">The vit.</param>
        public Stats(int level, int str, int agi, int intel, int vit) : this() {
            InitializeStats(level, str, agi, intel, vit);
        }

        /// <summary>
        /// Gets the resources.
        /// </summary>
        /// <value>
        /// The resources.
        /// </value>
        public IEnumerable<Stat> Resources {
            get {
                return baseStats.Values.Where(v => StatType.RESOURCES.Contains(v.Type));
            }
        }

        /// <summary>
        /// Gets the resource visibility.
        /// </summary>
        /// <value>
        /// The resource visibility.
        /// </value>
        public int ResourceVisibility {
            get {
                return resourceVisibility;
            }
        }

        /// <summary>
        /// Gets the short attribute distribution.
        /// </summary>
        /// <value>
        /// The short attribute distribution.
        /// </value>
        public string ShortAttributeDistribution {
            get {
                List<string> assignables = new List<string>();
                foreach (KeyValuePair<StatType, Stat> pair in baseStats) {
                    if (StatType.ASSIGNABLES.Contains(pair.Key)) {
                        assignables.Add(string.Format("{0} {1}",
                            GetStatCount(Get.TOTAL, pair.Key),
                            Util.ColorString(pair.Key.Name.Substring(0, NUMBER_OF_CHARS_IN_SHORT_STAT_NAME), pair.Key.Color)));
                    }
                }
                return string.Format(
                    "{0}",
                    string.Join("  ", assignables.ToArray())
                    );
            }
        }

        /// <summary>
        /// Gets the long attribute distribution.
        /// </summary>
        /// <value>
        /// The long attribute distribution.
        /// </value>
        public string LongAttributeDistribution {
            get {
                List<string> assignables = new List<string>();
                List<string> resources = new List<string>();
                List<string> other = new List<string>();
                foreach (KeyValuePair<StatType, Stat> pair in baseStats) {
                    string s = string.Format("{0} {1}/{2} {3} {4}",
                        pair.Key.ColoredName,
                        pair.Value.Mod,
                        pair.Value.Max,
                        GetFlatEquipmentBonus(pair.Key) == 0 ? string.Empty : Util.Sign(GetFlatEquipmentBonus(pair.Key)),
                        GetMultiplicativeBuffBonus(pair.Key) == 0 ? string.Empty : Util.Sign(GetMultiplicativeBuffBonus(pair.Key)) + "%"
                        );
                    if (StatType.ASSIGNABLES.Contains(pair.Key)) {
                        assignables.Add(s);
                    } else if (StatType.RESOURCES.Contains(pair.Key)) {
                        resources.Add(s);
                    }
                }
                return string.Format("Level {0}\n<Assignables>\n{1}\n<Resources>\n{2}",
                    this.Level,
                    string.Join("\n", assignables.ToArray()),
                    string.Join("\n", resources.ToArray())
                    );
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has stat points.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has stat points; otherwise, <c>false</c>.
        /// </value>
        public bool HasUnassignedStatPoints {
            get {
                return this.UnassignedStatPoints > 0;
            }
        }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public State State {
            get {
                if (GetStatCount(Get.MOD, StatType.HEALTH) <= 0 || GetStatCount(Get.TOTAL, StatType.VITALITY) <= 0) {
                    return State.DEAD;
                } else {
                    return State.ALIVE;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can level up.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can level up; otherwise, <c>false</c>.
        /// </value>
        public bool CanLevelUp {
            get {
                return GetStatCount(Get.MOD, StatType.EXPERIENCE) >= GetStatCount(Get.MAX, StatType.EXPERIENCE);
            }
        }

        /// <summary>
        /// Increases the resource visibility.
        /// </summary>
        public void IncreaseResourceVisibility() {
            resourceVisibility++;
        }

        /// <summary>
        /// Decreases the resource visibility.
        /// </summary>
        public void DecreaseResourceVisibility() {
            resourceVisibility--;
        }

        /// <summary>
        /// Adds the stat.
        /// </summary>
        /// <param name="stat">The stat.</param>
        public void AddStat(Stat stat) {
            if (!baseStats.ContainsKey(stat.Type)) {
                AddSplat(new SplatDetails(stat.Type.Color, "+", stat.Type.Sprite));
            }
            this.baseStats.Add(stat.Type, stat);
        }

        /// <summary>
        /// Removes the stat.
        /// </summary>
        /// <param name="type">The type.</param>
        protected void RemoveStat(StatType type) {
            this.baseStats.Remove(type);
        }

        /// <summary>
        /// Gets the missing stat count.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public int GetMissingStatCount(StatType type) {
            return GetStatCount(Get.MAX, type) - GetStatCount(Get.MOD, type);
        }

        public float GetStatPercent(StatType type) {
            float returnValue = 0;
            if (HasStat(type)) {
                returnValue = (GetStatCount(Get.TOTAL, type) + 0.0f) / GetStatCount(Get.MAX, type);
            }
            return returnValue;
        }

        /// <summary>
        /// Sets to stat.
        /// </summary>
        /// <param name="statType">Type of the stat.</param>
        /// <param name="type">The type.</param>
        /// <param name="amount">The amount.</param>
        public void SetToStat(StatType statType, Set type, int amount) {
            if (HasStat(statType)) {
                Stat stat = baseStats[statType];
                if (type == Set.MOD) {
                    stat.Mod = amount;
                } else if (type == Set.MAX) {
                    stat.Max = amount;
                } else if (type == Set.MOD_UNBOUND) {
                    stat.SetMod(amount, false);
                }
                AddSplat(new SplatDetails(statType.DetermineColor(amount), string.Format("={0}", amount), statType.Sprite));
            }
        }

        /// <summary>
        /// Adds to stat.
        /// </summary>
        /// <param name="statType">Type of the stat.</param>
        /// <param name="type">The type.</param>
        /// <param name="amount">The amount.</param>
        public void AddToStat(StatType statType, Set type, int amount) {
            if (HasStat(statType)) {
                if (amount != 0) {
                    Stat stat = baseStats[statType];
                    if (type == Set.MOD) {
                        stat.Mod += amount;
                    } else if (type == Set.MAX) {
                        stat.Max += amount;
                    } else if (type == Set.MOD_UNBOUND) {
                        stat.SetMod(stat.Mod + amount, false);
                    }
                }
                AddSplat(new SplatDetails(statType.DetermineColor(amount), StatUtil.ShowSigns(amount), statType.Sprite));
            }
        }

        /// <summary>
        /// Determines whether the character has stat.
        /// </summary>
        /// <param name="statType">Type of the stat.</param>
        /// <returns>
        ///   <c>true</c> if the specified stat type has stat; otherwise, <c>false</c>.
        /// </returns>
        public bool HasStat(StatType statType) {
            return baseStats.ContainsKey(statType);
        }

        /// <summary>
        /// Gets the stat count.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="statTypes">The stat types.</param>
        /// <returns></returns>
        public int GetStatCount(Get type, params StatType[] statTypes) {
            Util.Assert(statTypes.Length > 0, "No stat types specified.");
            int sum = 0;
            foreach (StatType st in statTypes) {
                if (HasStat(st)) {
                    Stat stat;
                    baseStats.TryGetValue(st, out stat);

                    int amountToAddToSum = 0;
                    if (type == Get.MOD) {
                        amountToAddToSum += stat.Mod;
                    } else if (type == Get.TOTAL) {
                        amountToAddToSum += (int)((stat.Mod + GetFlatEquipmentBonus(st)) * GetStatMultiplierFromBuffs(st));
                    } else if (type == Get.MAX) {
                        amountToAddToSum += stat.Max;
                    }

                    sum += st.Clamp(amountToAddToSum);
                }
            }
            return sum;
        }

        /// <summary>
        /// Updates the specified character's stats for dependent stats.
        /// </summary>
        /// <param name="owner">The stats owner.</param>
        public void Update(Character owner) {
            ICollection<Stat> stats = baseStats.Values;
            foreach (Stat stat in stats) {
                stat.Update(owner);
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) {
            var item = obj as Stats;

            if (item == null) {
                return false;
            }

            return
                Util.IsDictionariesEqual<StatType, Stat>(this.baseStats, item.baseStats)
                && this.resourceVisibility.Equals(item.resourceVisibility)
                && this.Level.Equals(item.Level)
                && this.UnassignedStatPoints.Equals(item.UnassignedStatPoints)
                && StatType.ASSIGNABLES.All(st => this.GetFlatEquipmentBonus(st) == item.GetFlatEquipmentBonus(st))
                && StatType.ASSIGNABLES.All(st => this.GetMultiplicativeBuffBonus(st) == item.GetMultiplicativeBuffBonus(st));
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
        /// Initializes the stats.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="str">The string.</param>
        /// <param name="agi">The agi.</param>
        /// <param name="intel">The intel.</param>
        /// <param name="vit">The vit.</param>
        protected void InitializeStats(int level, int str, int agi, int intel, int vit) {
            this.Level = level;
            SetToBothStat(StatType.STRENGTH, str);
            SetToBothStat(StatType.AGILITY, agi);
            SetToBothStat(StatType.INTELLECT, intel);
            SetToBothStat(StatType.VITALITY, vit);
        }

        /// <summary>
        /// Initializes the resources.
        /// </summary>
        public void InitializeResources() {
            ICollection<Stat> stats = baseStats.Values;
            foreach (Stat stat in stats) {
                if (StatType.RESTORED.Contains(stat.Type)) {
                    stat.Mod = stat.Max;
                }
            }
        }

        /// <summary>
        /// Restores assignable resources by a missing percentage.
        /// </summary>
        /// <param name="missingPercentage">The missing percentage.</param>
        public void RestoreResourcesByMissingPercentage(float missingPercentage) {
            foreach (StatType type in StatType.RESTORED) {
                int missing = (GetStatCount(Stats.Get.MAX, type) - GetStatCount(Stats.Get.MOD, type));
                int restoreAmount = Mathf.Max((int)(missing * missingPercentage), MINIMUM_RESTORE_AMOUNT);
                if (missing > 0) {
                    AddToStat(type, Stats.Set.MOD, restoreAmount);
                }
            }
        }

        /// <summary>
        /// Sets to both stat.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="amount">The amount.</param>
        private void SetToBothStat(StatType type, int amount) {
            SetToStat(type, Set.MOD_UNBOUND, amount);
            SetToStat(type, Set.MAX, amount);
        }

        /// <summary>
        /// Sets the default stats.
        /// </summary>
        private void SetDefaultStats() {
            this.AddStat(new Strength(0, 0));
            this.AddStat(new Agility(0, 0));
            this.AddStat(new Intellect(0, 0));
            this.AddStat(new Vitality(0, 0));
        }

        /// <summary>
        /// Sets the default resources.
        /// </summary>
        private void SetDefaultResources() {
            this.AddStat(new Health(0, 0));
        }

        private float GetStatMultiplierFromBuffs(StatType type) {
            return (100f + GetMultiplicativeBuffBonus(type)) / 100f;
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator<KeyValuePair<StatType, Stat>> IEnumerable<KeyValuePair<StatType, Stat>>.GetEnumerator() {
            return baseStats.GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return baseStats.GetEnumerator();
        }

        /// <summary>
        /// Gets the save object. A save object contains the neccessary
        /// information to initialize a clean class to its saved state.
        /// A save object is also serializable.
        /// </summary>
        /// <returns></returns>
        public CharacterStatsSave GetSaveObject() {
            List<StatSave> baseStatistics = new List<StatSave>();
            foreach (KeyValuePair<StatType, Stat> pair in baseStats) {
                baseStatistics.Add(pair.Value.GetSaveObject());
            }

            List<StatBonusSave> equipmentBonuses = new List<StatBonusSave>();
            foreach (StatType st in StatType.AllTypes) {
                equipmentBonuses.Add(new StatBonusSave(st.GetSaveObject(), GetFlatEquipmentBonus(st)));
            }

            List<StatBonusSave> buffBonuses = new List<StatBonusSave>();
            foreach (StatType st in StatType.AllTypes) {
                buffBonuses.Add(new StatBonusSave(st.GetSaveObject(), GetMultiplicativeBuffBonus(st)));
            }

            return new CharacterStatsSave(this.resourceVisibility, this.Level, this.UnassignedStatPoints, baseStatistics, equipmentBonuses, buffBonuses);
        }

        /// <summary>
        /// Initializes from save object.
        /// </summary>
        /// <param name="saveObject">The save object.</param>
        public void InitFromSaveObject(CharacterStatsSave saveObject) {
            this.Level = saveObject.Level;
            this.UnassignedStatPoints = saveObject.UnassignedStatPoints;
            baseStats.Clear();
            foreach (StatSave save in saveObject.BaseStats) {
                Stat stat = save.CreateObjectFromID();
                stat.InitFromSaveObject(save);
                baseStats.Add(stat.Type, stat);
            }

            /**
             * If we're spoofing the stats for a nonparty member, we want to include the equipment and buff bonuses too,
             * Otherwise buffs that scale off of mod + equip will have the bonus portion be 0
             * Example: DOT that deals damage based on caster's strength. Caster has a +10 strength sword, +10 strength from buffs and casts it on a party member
             * After a save, we want to maintain the strength bonus.
             */
            if (isSpoofed) {
                // Spoof equipment bonuses
                IDictionary<StatType, int> spoofedEquipment = new Dictionary<StatType, int>();
                foreach (StatBonusSave save in saveObject.EquipmentBonuses) {
                    spoofedEquipment.Add(save.StatType.Restore(), save.Bonus);
                }
                GetFlatEquipmentBonus = (st => spoofedEquipment[st]);

                // Spoof buff bonuses
                IDictionary<StatType, int> spoofedBuffs = new Dictionary<StatType, int>();
                foreach (StatBonusSave save in saveObject.BuffBonuses) {
                    spoofedBuffs.Add(save.StatType.Restore(), save.Bonus);
                }
                GetMultiplicativeBuffBonus = (st => spoofedBuffs[st]);
            }
            isDoneIniting = true;
        }

        /// <summary>
        /// Setups the temporary save fields.
        /// </summary>
        /// <param name="isSpoofed">if set to <c>true</c> [is spoofed].</param>
        public void SetupTemporarySaveFields(bool isSpoofed) {
            Util.Assert(!isDoneIniting, "Done initializing, this function can no longer be called.");
            this.isSpoofed = isSpoofed;
        }

        /// <summary>
        /// Compares agility values to determine who goes first.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns></returns>
        public int CompareTo(Stats other) {
            return StatUtil.GetDifference(StatType.AGILITY, other, this);
        }
    }
}