using NUnit.Framework;
using Scripts.Game.Defined.Characters;
using Scripts.Game.Defined.Serialized.Brains;
using Scripts.Game.Defined.Serialized.Buffs;
using Scripts.Game.Defined.Serialized.Items;
using Scripts.Game.Defined.Serialized.Spells;
using Scripts.Game.Defined.Serialized.Statistics;
using Scripts.Model.Buffs;
using Scripts.Model.Characters;
using Scripts.Model.Items;
using Scripts.Model.Pages;
using Scripts.Model.SaveLoad;
using Scripts.Model.SaveLoad.SaveObjects;
using Scripts.Model.Spells;
using Scripts.Model.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[TestFixture]
public class BasicTests {

    /// <summary>
    /// Tests whether all serializable content has been added
    /// to the IDTable.
    /// </summary>
    [TestFixture]
    public class IDTableTests {

        [OneTimeSetUp]
        public void Setup() {
            IdTable.Init();
        }

        [Test]
        public void TypeIDTableContainsAllSerializableMaterial() {
            IDTableIncludesEverythingHelper(IdTable.Types);
        }

        [Test]
        public void StatTypeIDTableContainsAllStatTypes() {
            IDTableIncludesEverythingHelper(IdTable.Stats);
        }

        [Test]
        public void EquipTypeIDTableContainsAllStatTypes() {
            IDTableIncludesEverythingHelper(IdTable.Equips);
        }

        private void IDTableIncludesEverythingHelper<T>(IdMap<T> idMap) {
            Assert.IsTrue(idMap.IsAllIncluded());
        }
    }

    /// <summary>
    /// Tests to see if saving and loading is working.
    /// </summary>
    [TestFixture]
    public class SerializationTests {

        [OneTimeSetUp]
        public void Setup() {
            IdTable.Init();
        }

        public string ToJson(object o) {
            string json = JsonUtility.ToJson(o);
            Debug.Log("Serialized to:\n" + json);
            return json;
        }

        public T FromJson<T>(string json) {
            return JsonUtility.FromJson<T>(json);
        }

        [Serializable]
        public sealed class ArrayWrapper {
            public int[] Array;

            public ArrayWrapper(int[] array) {
                this.Array = array;
            }
        }

        [Serializable]
        public sealed class ListWrapper {
            public List<int> List;

            public ListWrapper(List<int> list) {
                this.List = list;
            }
        }

        [Test]
        public void ListsAreSerializable() {
            List<int> list = new List<int>();
            list.Add(0);
            list.Add(1);
            list.Add(2);
            list.Add(3);

            string json = JsonUtility.ToJson(new ListWrapper(list));

            ListWrapper restored = JsonUtility.FromJson<ListWrapper>(json);
            Assert.AreEqual(list.Count, restored.List.Count);
            for (int i = 0; i <= 3; i++) {
                Assert.AreEqual(restored.List[i], i);
            }
        }

        [Test]
        public void ArraysAreSerializable() {
            int[] list = new int[4];
            list[0] = 0;
            list[1] = 1;
            list[2] = 2;
            list[3] = 3;

            string json = JsonUtility.ToJson(new ArrayWrapper(list));

            ArrayWrapper restored = JsonUtility.FromJson<ArrayWrapper>(json);
            Assert.AreEqual(list.Length, restored.Array.Length);
            for (int i = 0; i <= 3; i++) {
                Assert.AreEqual(restored.Array[i], i);
            }
        }

        /// <typeparam name="O">Type being serialized</typeparam>
        /// <typeparam name="S">SaveObject used to save</typeparam>
        /// <param name=""></param>
        private void IdSerializableHelper<O, S>(O saveable) where S : IdSaveObject<O> where O : ISaveable<S> {
            S initialSaveObject = saveable.GetSaveObject();
            string json = ToJson(initialSaveObject);
            S newSaveObject = FromJson<S>(json);
            O newObject = newSaveObject.CreateObjectFromID();
            newObject.InitFromSaveObject(newSaveObject);
            Assert.IsTrue(saveable.Equals(newObject));
            Assert.AreEqual(saveable, newObject);
        }

        private void BasicSerializableHelper<O, S>(O saveable) where O : ISaveable<S> {
            S initialSaveObject = saveable.GetSaveObject();
            string json = ToJson(initialSaveObject);
            S newSaveObject = FromJson<S>(json);
            O newObject = Util.TypeToObject<O>(typeof(O));
            newObject.InitFromSaveObject(newSaveObject);
            Assert.IsTrue(saveable.Equals(newObject));
            Assert.AreEqual(saveable, newObject);
        }

        private void TypeSafeEnumSerializableHelper<T, S>(T type) where S : TypeSafeEnumSave<T> where T : ISaveable<S> {
            S initialSaveObject = type.GetSaveObject();
            string json = ToJson(initialSaveObject);
            S newSaveObject = FromJson<S>(json);
            ISaveable<S> newObject = newSaveObject.Restore();
            Assert.AreEqual(type, newObject);
            Assert.AreSame(type, newObject);
        }

        private void ContainerSerializationHelper<O, S, T>(O original) where O : ISaveable<S>, IEnumerable<T> {
            S initialSaveObject = original.GetSaveObject();
            string json = ToJson(initialSaveObject);
            S newSaveObject = FromJson<S>(json);
            O newObject = Util.TypeToObject<O>(original.GetType());
            newObject.InitFromSaveObject(newSaveObject);
            Assert.AreEqual(original, newObject);
            CollectionAssert.AreEquivalent(original, newObject);
        }

        [Test]
        public void IDsInputMatchesOutput() {
            foreach (KeyValuePair<Type, string> pair in IdTable.Types) {
                Assert.AreEqual(IdTable.Types.Get(pair.Key), pair.Value);
                Assert.AreEqual(IdTable.Types.Get(pair.Value), pair.Key);
            }
            foreach (KeyValuePair<StatType, string> pair in IdTable.Stats) {
                Assert.AreEqual(IdTable.Stats.Get(pair.Key), pair.Value);
                Assert.AreEqual(IdTable.Stats.Get(pair.Value), pair.Key);
            }
            foreach (KeyValuePair<EquipType, string> pair in IdTable.Equips) {
                Assert.AreEqual(IdTable.Equips.Get(pair.Key), pair.Value);
                Assert.AreEqual(IdTable.Equips.Get(pair.Value), pair.Key);
            }
        }

        [Test]
        public void AgilitySerializationWorks() {
            IdSerializableHelper<Stat, StatSave>(new Agility(5, 25));
        }

        [Test]
        public void ItemIsSerializable() {
            IdSerializableHelper<Item, ItemSave>(new Apple());
        }

        [Test]
        public void StatTypeIsSerializable() {
            foreach (StatType type in StatType.AllTypes) {
                TypeSafeEnumSerializableHelper<StatType, StatTypeSave>(type);
            }
        }

        [Test]
        public void EquipTypeIsSerializable() {
            foreach (EquipType type in EquipType.AllTypes) {
                TypeSafeEnumSerializableHelper<EquipType, EquipTypeSave>(type);
            }
        }

        [Test]
        public void StatIsSerializable() {
            IdSerializableHelper<Stat, StatSave>(new Strength(5, 10));
        }

        [Test]
        public void CharacterStatsAreSerializable() {
            Stats stats = new Stats(1, 2, 3, 4, 5);
            ContainerSerializationHelper<Stats, CharacterStatsSave, KeyValuePair<StatType, Stat>>(stats);
        }

        [Test]
        public void SpellBookIsSerializable() {
            IdSerializableHelper<SpellBook, SpellBookSave>(new Attack());
        }

        [Test]
        public void CharacterSpellBooksAreSerializable() {
            SpellBooks sbs = new SpellBooks();
            sbs.AddSpellBook(new InflictPoison());
            sbs.AddSpellBook(new Attack());
            ContainerSerializationHelper<SpellBooks, CharacterSpellBooksSave, SpellBook>(sbs);
        }

        [Test]
        public void BrainEqualityWorks() {
            Assert.IsTrue(new Player().Equals(new Player()));
            Assert.AreEqual(new Player(), new Player());
            Assert.AreNotEqual(new Player(), new DebugAI());
        }

        [Test]
        public void CharacterBrainIsSerializable() {
            IdSerializableHelper<Brain, BrainSave>(new Player());
        }

        [Test]
        public void InventoryIsSerializable() {
            Inventory inv = new Inventory();
            inv.ForceAdd(new Apple(), 5);
            inv.ForceAdd(new PoisonArmor(), 2);
            ContainerSerializationHelper<Inventory, InventorySave, Item>(inv);
        }

        [Test]
        public void EmptyInventoryIsSerializable() {
            Inventory inv = new Inventory();
            ContainerSerializationHelper<Inventory, InventorySave, Item>(inv);
        }

        [Test]
        public void LookIsSerializable() {
            BasicSerializableHelper<Look, LookSave>(new Look("test", "fox-head", "test5", Breed.UNKNOWN));
        }

        [Test]
        public void EquippedItemIsSerializable() {
            IdSerializableHelper<EquippableItem, EquipItemSave>(new PoisonArmor());
        }

        [Test]
        public void EquipmentIsSerializable() {
            Equipment equips = new Equipment();
            Inventory inv = new Inventory();
            inv.Add(new PoisonArmor());
            equips.AddEquip(inv, new BuffParams(new Stats(1, 2, 3, 4, 5), 2), new PoisonArmor());

            EquipmentSave initialSaveObject = equips.GetSaveObject();
            initialSaveObject.Buffs.ForEach(b => b.Buff.SetupAsCasterNotInParty());
            string json = ToJson(initialSaveObject);
            EquipmentSave newSaveObject = FromJson<EquipmentSave>(json);
            Equipment newObject = Util.TypeToObject<Equipment>(equips.GetType());
            newObject.InitFromSaveObject(newSaveObject);
            Assert.AreEqual(equips, newObject);
            CollectionAssert.AreEquivalent(equips, newObject);
        }

        [Test]
        public void SavingSingleCharacterPartyThrowsNoErrors() {
            SavingCharactersInPartyThrowsNoErrors(1);
        }

        [Test]
        public void SavingMultipleCharacterPartyThrowsNoErrors() {
            for (int i = 0; i < 10; i++) {
                SavingCharactersInPartyThrowsNoErrors(i);
            }
        }

        private void SavingCharactersInPartyThrowsNoErrors(int characterCount) {
            Party party = new Party();
            for (int i = 0; i < characterCount; i++) {
                party.AddMember(CharacterList.TestEnemy());
            }
            ToJson(party.GetSaveObject());
        }

        [Test]
        public void LoadingSingleCharacterPartyThrowsNoErrors() {
            LoadingCharactersInPartyThrowsNoErrors(1);
        }

        [Test]
        public void LoadingMultipleCharacterPartyThrowsNoErrors() {
            for (int i = 0; i < 10; i++) {
                LoadingCharactersInPartyThrowsNoErrors(i);
            }
        }

        private void LoadingCharactersInPartyThrowsNoErrors(int characterCount) {
            Party party = new Party();
            for (int i = 0; i < characterCount; i++) {
                party.AddMember(CharacterList.TestEnemy());
            }
            string json = ToJson(party.GetSaveObject());
            PartySave partySave = JsonUtility.FromJson<PartySave>(json);
            Party party2 = new Party();
            party2.InitFromSaveObject(partySave);
        }

        [Test]
        public void SettingCasterForBuffSetsEverythingCorrectly() {
            Character caster = CharacterList.TestEnemy();
            Buff poison = new Poison();
            poison.Caster = new BuffParams(caster.Stats, caster.Id);
            Assert.AreEqual(caster.Id, poison.CasterId);
            Assert.AreSame(caster.Stats, poison.BuffCaster);
        }

        [Test]
        public void EquippedItemsAreSaved() {
            Character dummy = CharacterList.TestEnemy();
            dummy.Inventory.ForceAdd(new BrokenSword());
            dummy.Equipment.AddEquip(dummy.Inventory, new BuffParams(dummy.Stats, dummy.Id), new BrokenSword());
            Party party = new Party();
            party.AddMember(dummy);
            PartySave ps = party.GetSaveObject();
            PartySave retrieved = FromJson<PartySave>(ToJson(party.GetSaveObject()));
            Party party2 = new Party();
            party2.InitFromSaveObject(retrieved);
            Assert.AreEqual(party, party2);
        }

        [Test, Timeout(2000)]
        public void SaveLoadMaintainsReferencesForPartyMemberCastedBuffs() {
            Party party = new Party();
            Character dummy = CharacterList.TestEnemy();
            Character buffCaster = CharacterList.TestEnemy();
            Character buffRecipient = CharacterList.TestEnemy();

            Poison poison = new Poison();
            Debug.Log("BuffcasterID: " + buffCaster.Id);
            poison.Caster = new BuffParams(buffCaster.Stats, buffCaster.Id);

            buffRecipient.Buffs.AddBuff(poison);

            party.AddMember(dummy);
            party.AddMember(buffCaster);
            party.AddMember(buffRecipient);

            PartySave retrieved = FromJson<PartySave>(ToJson(party.GetSaveObject()));

            Party party2 = new Party();
            party2.InitFromSaveObject(retrieved);

            List<Character> b = party.ToList();

            Character caster = b[1];
            Character target = b[2];

            Assert.IsTrue(party.Equals(party2));
            Assert.AreEqual(party, party2);
            Assert.AreSame(party.Collection.ToList()[1].Stats, party.Collection.ToList()[2].Buffs.ToList()[0].BuffCaster);
        }

        [Test, Timeout(2000)]
        public void SaveLoadDoesNotMaintainsReferencesForNonPartyMemberCastedBuffs() {
            Party party = new Party();
            Character dummy = CharacterList.TestEnemy();
            Character buffCaster = CharacterList.TestEnemy();
            Character buffRecipient = CharacterList.TestEnemy();

            Poison poison = new Poison();
            Debug.Log("BuffcasterID: " + buffCaster.Id);
            poison.Caster = new BuffParams(buffCaster.Stats, buffCaster.Id);

            buffRecipient.Buffs.AddBuff(poison);

            party.AddMember(dummy);
            party.AddMember(CharacterList.TestEnemy());
            party.AddMember(buffRecipient);

            PartySave retrieved = FromJson<PartySave>(ToJson(party.GetSaveObject()));

            Party party2 = new Party();
            party2.InitFromSaveObject(retrieved);

            List<Character> a = party.ToList();
            List<Character> b = party.ToList();

            Character caster = b[1];
            Character target = b[2];

            Assert.IsTrue(party.Equals(party2));
            Assert.AreEqual(party, party2);
            Assert.AreNotSame(party.Collection.ToList()[1].Stats, party.Collection.ToList()[2].Buffs.ToList()[0].BuffCaster);
        }

        [Test]
        public void SaveLoadMaintainsBonusesFromEquipmentAndBuffsForNonPartyMemberCastedBuffs() {
            Party party = new Party();
            Character dummy = CharacterList.TestEnemy();
            Character buffCaster = CharacterList.TestEnemy();

            buffCaster.Inventory.ForceAdd(new BrokenSword());
            buffCaster.Equipment.AddEquip(buffCaster.Inventory, new BuffParams(buffCaster.Stats, buffCaster.Id), new BrokenSword());
            buffCaster.AddBuff(new StrengthBoost());

            Character buffRecipient = CharacterList.TestEnemy();

            StrengthScalingPoison poison = new StrengthScalingPoison();
            Debug.Log("BuffcasterID: " + buffCaster.Id);
            poison.Caster = new BuffParams(buffCaster.Stats, buffCaster.Id);
            buffRecipient.Buffs.AddBuff(poison);

            int initialStrength = buffCaster.Stats.GetStatCount(Stats.Get.TOTAL, StatType.STRENGTH);
            Assert.AreEqual(buffCaster.Stats.GetMultiplicativeBuffBonus(StatType.STRENGTH), StrengthBoost.STRENGTH_INCREASE_AMOUNT);
            Assert.AreEqual(buffCaster.Stats.GetFlatEquipmentBonus(StatType.STRENGTH), new BrokenSword().StatBonuses[StatType.STRENGTH]);

            party.AddMember(dummy);
            party.AddMember(CharacterList.TestEnemy());
            party.AddMember(buffRecipient);

            PartySave retrieved = FromJson<PartySave>(ToJson(party.GetSaveObject()));

            Party party2 = new Party();
            party2.InitFromSaveObject(retrieved);

            Stats spoofedCasterStats = party2.Collection.ToArray()[2].Buffs.ToArray()[0].BuffCaster;
            int spoofedStrength = spoofedCasterStats.GetStatCount(Stats.Get.TOTAL, StatType.STRENGTH);
            Assert.AreEqual(buffCaster.Stats.GetStatCount(Stats.Get.MOD, StatType.STRENGTH), spoofedCasterStats.GetStatCount(Stats.Get.MOD, StatType.STRENGTH));
            Assert.AreEqual(StrengthBoost.STRENGTH_INCREASE_AMOUNT, spoofedCasterStats.GetMultiplicativeBuffBonus(StatType.STRENGTH));
            Assert.AreEqual(new BrokenSword().StatBonuses[StatType.STRENGTH], spoofedCasterStats.GetFlatEquipmentBonus(StatType.STRENGTH));
            Assert.AreEqual(buffCaster.Stats.GetStatCount(Stats.Get.TOTAL, StatType.STRENGTH), spoofedStrength);
            Assert.AreEqual(buffCaster.Stats, spoofedCasterStats);
        }

        [Test]
        public void SaveLoadMaintainsBonusesFromEquipmentAndBuffsForPartyMemberCastedBuffs() {
            Party party = new Party();
            Character dummy = CharacterList.TestEnemy();
            Character buffCaster = CharacterList.TestEnemy();

            buffCaster.Inventory.ForceAdd(new BrokenSword());
            buffCaster.Equipment.AddEquip(buffCaster.Inventory, new BuffParams(buffCaster.Stats, buffCaster.Id), new BrokenSword());
            buffCaster.AddBuff(new StrengthBoost());

            Character buffRecipient = CharacterList.TestEnemy();

            StrengthScalingPoison poison = new StrengthScalingPoison();
            Debug.Log("BuffcasterID: " + buffCaster.Id);
            poison.Caster = new BuffParams(buffCaster.Stats, buffCaster.Id);
            buffRecipient.Buffs.AddBuff(poison);

            party.AddMember(dummy);
            party.AddMember(CharacterList.TestEnemy());
            party.AddMember(buffRecipient);
            party.AddMember(buffCaster);

            PartySave retrieved = FromJson<PartySave>(ToJson(party.GetSaveObject()));

            Party party2 = new Party();
            party2.InitFromSaveObject(retrieved);

            Assert.AreEqual(buffCaster.Stats, party2.Collection.ToArray()[3].Stats);
            Assert.AreEqual(buffCaster.Buffs, party2.Collection.ToArray()[3].Buffs);
        }
    }

    /// <summary>
    /// Tests advanced battle logic.
    /// </summary>
    [TestFixture]
    public class BattleLogicTests {

        [Test]
        public void CharacterWithHigherAgilityIsLessThanSlowerCharacter() {
            Character fast = CharacterList.TestEnemy();
            fast.Stats.AddToStat(StatType.AGILITY, Stats.Set.MOD_UNBOUND, 100);
            Character slow = CharacterList.TestEnemy();
            int difference = fast.Stats.CompareTo(slow.Stats);
            Debug.Log("Difference is: " + difference);
            Assert.IsTrue(difference < 0);
        }

        [Test]
        public void SpellsAreSortedCorrectly() {
            Character fast = CharacterList.TestEnemy();
            fast.Stats.AddToStat(StatType.AGILITY, Stats.Set.MOD_UNBOUND, 100);
            Character slow = CharacterList.TestEnemy();
            Battle dummy = new Battle(new Page("dummy"), new Page("dummy"), Music.NORMAL, "Dummy", new Character[] { fast }, new Character[] { slow });

            IPlayable lowPriorityWithSlowCaster = new Spell(new ReflectiveClone(), new Result(), slow, fast);
            IPlayable lowPriorityWithFastCaster = new Spell(new ReflectiveClone(), new Result(), fast, slow);
            IPlayable normalPriorityWithSlowCaster = new Spell(new Attack(), new Result(), slow, fast);
            IPlayable normalPriorityWithFastCaster = new Spell(new Attack(), new Result(), fast, slow);
            IPlayable highPriorityWithSlowCaster = new Spell(new EnemyHeal(), new Result(), slow, slow);
            IPlayable highPriorityWithFastCaster = new Spell(new EnemyHeal(), new Result(), fast, slow);

            IPlayable[] expectedOrder = new IPlayable[] {
                highPriorityWithFastCaster,
                highPriorityWithSlowCaster,
                normalPriorityWithFastCaster,
                normalPriorityWithSlowCaster,
                lowPriorityWithFastCaster,
                lowPriorityWithSlowCaster
            };

            List<IPlayable> actualOrder = new List<IPlayable>();
            actualOrder.Add(normalPriorityWithSlowCaster);
            actualOrder.Add(normalPriorityWithFastCaster);
            actualOrder.Add(highPriorityWithSlowCaster);
            actualOrder.Add(highPriorityWithFastCaster);
            actualOrder.Add(lowPriorityWithSlowCaster);
            actualOrder.Add(lowPriorityWithFastCaster);
            actualOrder.Sort();

            for (int i = 0; i < expectedOrder.Length; i++) {
                Debug.Log(string.Format("Index {0}\nExpected: {1}\nActual: {2}\n",
                    i,
                    GetSpellDetails(expectedOrder[i].MySpell),
                    GetSpellDetails(actualOrder[i].MySpell)));
                Assert.AreSame(expectedOrder[i], actualOrder[i]);
            }
        }

        private string GetSpellDetails(Spell spell) {
            return string.Format(
                "Caster agility: {0}/Spell priority: {1}",
                spell.MySpell.Caster.Stats.GetStatCount(Stats.Get.TOTAL, StatType.AGILITY),
                spell.MySpell.Book.Priority
                );
        }
    }
}