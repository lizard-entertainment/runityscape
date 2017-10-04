using Scripts.Game.Defined.Serialized.Brains;
using Scripts.Game.Defined.Serialized.Buffs;
using Scripts.Game.Defined.Serialized.Items;
using Scripts.Game.Defined.Serialized.Spells;
using Scripts.Game.Defined.Serialized.Statistics;
using Scripts.Game.Serialized.Brains;
using Scripts.Model.Initable;
using Scripts.Model.Interfaces;
using Scripts.Model.Items;
using Scripts.Model.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Scripts.Model.SaveLoad {

    /// <summary>
    /// Table holding various Ids
    /// </summary>
    public static class IdTable {

        /// <summary>
        /// Mapping of types to unique strings
        /// </summary>
        public static TypeMap Types = new TypeMap();

        /// <summary>
        /// Mapping of type safe enums
        /// </summary>
        public static EnumMap<StatType> Stats = new EnumMap<StatType>(StatType.AllTypes);

        /// <summary>
        /// Mapping of type safe enums
        /// </summary>
        public static EnumMap<EquipType> Equips = new EnumMap<EquipType>(EquipType.AllTypes);

        private static HashSet<IInitable> initables;
        private static bool isInited;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public static void Init() {
            if (!isInited) {
                foreach (IInitable init in initables) {
                    init.Init();
                }
            }
            isInited = true;
        }

        /// <summary>
        /// Lazy initialization for initables because the set refuses to initialize
        /// before callers call its add method if I initialize it in the fields.
        /// </summary>
        /// <param name="init">Initable content for Main.cs to load.</param>
        public static void AddInit(IInitable init) {
            if (initables == null) {
                initables = new HashSet<IInitable>();
            }
            initables.Add(init);
        }
    }

    /// <summary>
    /// TypeMap holds the mappings of type and a string,
    /// used for saving and loading even if class names are refactored.
    /// </summary>
    /// <seealso cref="Scripts.Model.SaveLoad.IdMap{System.Type}" />
    public class TypeMap : IdMap<Type> {

        protected override void InitHelper() {
            Stats();
            Spells();
            Items();
            Buffs();
            Brains();
        }

        public override bool IsAllIncluded() {
            return IsEntireNamespaceIdentified() && IsDictTypesContainDefaultConstructor();
        }

        private void Stats() {
            Add<Strength>("str");
            Add<Intellect>("int");
            Add<Agility>("agi");
            Add<Vitality>("vit");

            Add<Health>("hp");
            Add<Skill>("sk");
            Add<Experience>("exp");
            Add<Mana>("mana");
        }

        private void Spells() {
            Add<Attack>("attack");
            Add<Wait>("wait");
            Add<InflictPoison>("inflictpoison");
            Add<Check>("checkSpell");
            Add<SetupCounter>("setupCounter");
            Add<EnemyHeal>("heal");
            Add<ReflectiveClone>("reflectiveClone");
            Add<Blackout>("blackout");
            Add<RevealTrueForm>("revealTrueForm");
            Add<Ignite>("ignite");
            Add<CrushingBlow>("crushingBlow");
            Add<PlayerHeal>("playerHeal");
            Add<SetupDefend>("setupDefend");
        }

        private void Items() {
            Add<Apple>("apple");
            Add<PoisonArmor>("poisonarmor");
            Add<Money>("money");
            Add<GhostArmor>("ghostArmor");
            Add<BrokenSword>("ghostSword");
            Add<Shield>("shield");
            Add<RegenArmor>("regenArmor");
            Add<FishHook>("fishHook");
            Add<SilverBoot>("silverBoot");
            Add<Wand>("wand");
            Add<MadnessStaff>("madnessStaff");
            Add<IdentifyScroll>("identifyScroll");
            Add<RevivalSeed>("revivalSeed");
            Add<CheckTome>("checkTome");
            Add<CrushingBlowTome>("crushTome");
            Add<HealTome>("healTome");
            Add<DefendTome>("defendTome");
        }

        private void Buffs() {
            Add<Poison>("poison");
            Add<Checked>("checkedDebuff");
            Add<Counter>("counterBuff");
            Add<ReflectAttack>("reflectAttack");
            Add<SpiritLink>("spiritLink");
            Add<StrengthScalingPoison>("strengthScalingPoison");
            Add<BlackedOut>("blackoutDebuff");
            Add<DamageResist>("damageResist");
            Add<Ignited>("ignitedDebuff");
            Add<Insight>("insight");
            Add<RegenerateHealth>("restore");
            Add<StrengthBoost>("strengthBoost");
            Add<FishShook>("fishShook");
            Add<Defend>("defend");
            Add<RegenerateMana>("regenMana");
            Add<LifeSteal>("lifeSteal");
        }

        private void Brains() {
            Add<Player>("player");
            Add<DebugAI>("debugai");
            Add<Attacker>("villager");
            Add<Healer>("healer");
            Add<Replicant>("replicant");
            Add<ReplicantClone>("replicantClone");
            Add<Illusionist>("illusionist");
            Add<BigKnight>("bigKnight");
            Add<BlackShuck>("blackShuck");
        }

        private void Add<Type>(string id) {
            Add(typeof(Type), id);
        }

        private Type[] GetTypesInNamespace(Assembly assembly, string nameSpace) {
            return assembly.GetTypes().Where(t => t.Namespace != null && t.Namespace.Contains(nameSpace) && !t.ToString().Contains("c__AnonStorey0")).ToArray();
        }

        /// <summary>
        /// All classes within the namespace must be in the map.
        /// </summary>
        private bool IsEntireNamespaceIdentified() {
            Type[] types = GetTypesInNamespace(Assembly.GetExecutingAssembly(), "Scripts.Game.Defined.Serialized");
            bool isEverythingIncludedInMap = true;
            foreach (Type t in types) {
                if (!map.Contains(t) && !t.IsAbstract) {
                    Debug.Log(string.Format("Unable to locate {0} in Map.", t.ToString()));
                    isEverythingIncludedInMap = false;
                }
            }
            return isEverythingIncludedInMap;
        }

        private bool IsDictTypesContainDefaultConstructor() {
            bool isAllDefault = true;
            foreach (KeyValuePair<Type, string> pair in map) {
                Type type = pair.Key;
                ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
                if (constructor == null) {
                    Debug.Log(string.Format("Unable to locate default constructor for {0} in map.", type.ToString()));
                    isAllDefault = false;
                }
            }
            return isAllDefault;
        }
    }

    public class EnumMap<T> : IdMap<T> where T : INameable {
        private ICollection<T> all;

        public EnumMap(ICollection<T> all) {
            this.all = all;
        }

        public override bool IsAllIncluded() {
            return IsAllTypesIncluded();
        }

        protected override void InitHelper() {
            foreach (T item in all) {
                Add(item, item.Name);
            }
        }

        private bool IsAllTypesIncluded() {
            bool isAllIncluded = true;
            foreach (T item in all) {
                if (!map.Contains(item)) {
                    isAllIncluded = false;
                }
            }
            return isAllIncluded;
        }
    }
}