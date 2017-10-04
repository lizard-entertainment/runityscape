using Scripts.Game.Defined.Characters;
using Scripts.Model.Buffs;
using Scripts.Model.Characters;
using Scripts.Model.Items;
using Scripts.Model.Spells;
using Scripts.Model.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using Scripts.Model.SaveLoad.SaveObjects;
using Scripts.Game.Serialized;
using Scripts.Game.Pages;
using Scripts.Game.Dungeons;

namespace Scripts.Model.SaveLoad.SaveObjects {

    /// <summary>
    /// Saves various class types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class IdSaveObject<T> {

        /// <summary>
        /// The class identifier
        /// </summary>
        public string ClassId;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdSaveObject{T}"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public IdSaveObject(Type type) {
            this.ClassId = IdTable.Types.Get(type);
        }

        /// <summary>
        /// Creates the object from identifier.
        /// </summary>
        /// <returns></returns>
        public T CreateObjectFromID() {
            Type type = IdTable.Types.Get(ClassId);
            return Util.TypeToObject<T>(type);
        }
    }

    /// <summary>
    /// Saves various type safe enums
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public abstract class TypeSafeEnumSave<T> {

        /// <summary>
        /// The identifier
        /// </summary>
        public string Id;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeSafeEnumSave{T}"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public TypeSafeEnumSave(string id) {
            this.Id = id;
        }

        /// <summary>
        /// Restores this instance.
        /// </summary>
        /// <returns></returns>
        public abstract T Restore();
    }

    /// <summary>
    /// Saves stat types
    /// </summary>
    /// <seealso cref="Scripts.Model.SaveLoad.SaveObjects.TypeSafeEnumSave{Scripts.Model.Stats.StatType}" />
    [Serializable]
    public class StatTypeSave : TypeSafeEnumSave<StatType> {

        /// <summary>
        /// Initializes a new instance of the <see cref="StatTypeSave"/> class.
        /// </summary>
        /// <param name="st">The st.</param>
        public StatTypeSave(StatType st) : base(IdTable.Stats.Get(st)) { }

        /// <summary>
        /// Restores this instance.
        /// </summary>
        /// <returns></returns>
        public override StatType Restore() {
            return IdTable.Stats.Get(Id);
        }
    }

    /// <summary>
    /// Saves equip types
    /// </summary>
    /// <seealso cref="Scripts.Model.SaveLoad.SaveObjects.TypeSafeEnumSave{Scripts.Model.Items.EquipType}" />
    [Serializable]
    public class EquipTypeSave : TypeSafeEnumSave<EquipType> {

        /// <summary>
        /// Initializes a new instance of the <see cref="EquipTypeSave"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public EquipTypeSave(EquipType type) : base(IdTable.Equips.Get(type)) { }

        /// <summary>
        /// Restores this instance.
        /// </summary>
        /// <returns></returns>
        public override EquipType Restore() {
            return IdTable.Equips.Get(Id);
        }
    }

    /// <summary>
    /// Saves stats
    /// </summary>
    /// <seealso cref="Scripts.Model.SaveLoad.SaveObjects.IdSaveObject{Scripts.Model.Stats.Stat}" />
    [Serializable]
    public sealed class StatSave : IdSaveObject<Stat> {

        /// <summary>
        /// The stat type
        /// </summary>
        public StatTypeSave StatType;

        /// <summary>
        /// The mod
        /// </summary>
        public int Mod;

        /// <summary>
        /// The maximum
        /// </summary>
        public int Max;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatSave"/> class.
        /// </summary>
        /// <param name="statType">Type of the stat.</param>
        /// <param name="type">The type.</param>
        /// <param name="mod">The mod.</param>
        /// <param name="max">The maximum.</param>
        public StatSave(StatTypeSave statType, Type type, int mod, int max) : base(type) {
            this.StatType = statType;
            this.Mod = mod;
            this.Max = max;
        }
    }

    /// <summary>
    /// Saves StatType and its count. Used for equipment bonuses and bonuses in general.
    /// </summary>
    [Serializable]
    public sealed class StatBonusSave {

        /// <summary>
        /// The stat type
        /// </summary>
        public StatTypeSave StatType;

        /// <summary>
        /// The bonus
        /// </summary>
        public int Bonus;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatBonusSave"/> class.
        /// </summary>
        /// <param name="statType">Type of the stat.</param>
        /// <param name="value">The value.</param>
        public StatBonusSave(StatTypeSave statType, int value) {
            this.StatType = statType;
            this.Bonus = value;
        }
    }

    /// <summary>
    /// Save character's stats
    /// </summary>
    /// <seealso cref="System.Collections.Generic.IEnumerable{Scripts.Model.SaveLoad.SaveObjects.StatSave}" />
    [Serializable]
    public sealed class CharacterStatsSave : IEnumerable<StatSave> {

        /// <summary>
        /// The stats
        /// </summary>
        public List<StatSave> BaseStats;

        /// <summary>
        /// The equipment bonuses, used for spoofing.
        ///
        /// Save equipment bonuses needs to handle this rare situation:
        /// 1. Enemy with +Stat gear equipped casts Buff that depends on Stat on Party member
        /// 2. Save and Load
        /// 3. Buff still needs to depend on +Stat gear of enemy that no longer exists
        /// </summary>
        public List<StatBonusSave> EquipmentBonuses;

        /// <summary>
        /// The buff bonuses, also used for spoofing.
        /// </summary>
        public List<StatBonusSave> BuffBonuses;

        /// <summary>
        /// The level
        /// </summary>
        public int Level;

        /// <summary>
        /// The unassigned stat point count
        /// </summary>
        public int UnassignedStatPoints;

        /// <summary>
        /// The resource visibility
        /// </summary>
        public int ResourceVisibility;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacterStatsSave"/> class.
        /// </summary>
        /// <param name="resourceVisibility">The resource visibility.</param>
        /// <param name="level">The level.</param>
        /// <param name="unassignedStatPoints">The stat points.</param>
        /// <param name="baseStats">The stats.</param>
        /// <param name="buffBonus">The stat bonus.</param>
        /// <param name="equipmentBonuses">The equipment bonuses.</param>
        public CharacterStatsSave(
            int resourceVisibility,
            int level,
            int unassignedStatPoints,
            List<StatSave> baseStats,
            List<StatBonusSave> equipmentBonuses,
            List<StatBonusSave> buffBonuses) {
            this.ResourceVisibility = resourceVisibility;
            this.BaseStats = baseStats;
            this.EquipmentBonuses = equipmentBonuses;
            this.BuffBonuses = buffBonuses;
            this.Level = level;
            this.UnassignedStatPoints = unassignedStatPoints;
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return BaseStats.GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator<StatSave> IEnumerable<StatSave>.GetEnumerator() {
            return BaseStats.GetEnumerator();
        }
    }

    /// <summary>
    /// Save spellbook
    /// </summary>
    /// <seealso cref="Scripts.Model.SaveLoad.SaveObjects.IdSaveObject{Scripts.Model.Spells.SpellBook}" />
    [Serializable]
    public sealed class SpellBookSave : IdSaveObject<SpellBook> {

        /// <summary>
        /// Initializes a new instance of the <see cref="SpellBookSave"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public SpellBookSave(Type type) : base(type) { }
    }

    /// <summary>
    /// Save character's spells
    /// </summary>
    /// <seealso cref="System.Collections.Generic.IEnumerable{Scripts.Model.SaveLoad.SaveObjects.SpellBookSave}" />
    [Serializable]
    public sealed class CharacterSpellBooksSave : IEnumerable<SpellBookSave> {

        /// <summary>
        /// The books
        /// </summary>
        public List<SpellBookSave> Books;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacterSpellBooksSave"/> class.
        /// </summary>
        /// <param name="books">The books.</param>
        public CharacterSpellBooksSave(List<SpellBookSave> books) {
            this.Books = books;
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return Books.GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator<SpellBookSave> IEnumerable<SpellBookSave>.GetEnumerator() {
            return Books.GetEnumerator();
        }
    }

    /// <summary>
    /// Saves character's brain
    /// </summary>
    /// <seealso cref="Scripts.Model.SaveLoad.SaveObjects.IdSaveObject{Scripts.Model.Characters.Brain}" />
    [Serializable]
    public sealed class BrainSave : IdSaveObject<Brain> {

        /// <summary>
        /// Initializes a new instance of the <see cref="BrainSave"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public BrainSave(Type type) : base(type) { }
    }

    /// <summary>
    /// Save item
    /// </summary>
    /// <seealso cref="Scripts.Model.SaveLoad.SaveObjects.IdSaveObject{Scripts.Model.Items.Item}" />
    [Serializable]
    public sealed class ItemSave : IdSaveObject<Item> {

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemSave"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public ItemSave(Type type) : base(type) { }
    }

    /// <summary>
    /// Save inventory
    /// </summary>
    /// <seealso cref="System.Collections.Generic.IEnumerable{Scripts.Model.SaveLoad.SaveObjects.InventorySave.ItemCount}" />
    [Serializable]
    public sealed class InventorySave : IEnumerable<InventorySave.ItemCount> {

        /// <summary>
        ///
        /// </summary>
        [Serializable]
        public struct ItemCount {

            /// <summary>
            /// The item
            /// </summary>
            public ItemSave Item;

            /// <summary>
            /// The count
            /// </summary>
            public int Count;
        }

        /// <summary>
        /// The inventory capacity
        /// </summary>
        public int InventoryCapacity;

        /// <summary>
        /// The items
        /// </summary>
        public List<ItemCount> Items;

        /// <summary>
        /// Initializes a new instance of the <see cref="InventorySave"/> class.
        /// </summary>
        /// <param name="inventoryCapacity">The inventory capacity.</param>
        /// <param name="items">The items.</param>
        public InventorySave(
            int inventoryCapacity,
            List<ItemCount> items) {
            this.InventoryCapacity = inventoryCapacity;
            this.Items = items;
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return Items.GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<ItemCount> GetEnumerator() {
            return Items.GetEnumerator();
        }
    }

    /// <summary>
    /// Saves a character's look
    /// </summary>
    [Serializable]
    public sealed class LookSave {

        /// <summary>
        /// The name
        /// </summary>
        public string Name;

        /// <summary>
        /// The sprite
        /// </summary>
        public Sprite Sprite;

        /// <summary>
        /// The text color
        /// </summary>
        public Color TextColor;

        /// <summary>
        /// The tooltip
        /// </summary>
        public string Tooltip;

        /// <summary>
        /// The breed
        /// </summary>
        public Breed Breed;

        /// <summary>
        /// Initializes a new instance of the <see cref="LookSave"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="sprite">The sprite.</param>
        /// <param name="text">The text.</param>
        /// <param name="tooltip">The tooltip.</param>
        /// <param name="breed">The breed.</param>
        public LookSave(string name, Sprite sprite, Color text, string tooltip, Breed breed) {
            this.Name = name;
            this.Sprite = sprite;
            this.TextColor = text;
            this.Tooltip = tooltip;
            this.Breed = breed;
        }
    }

    /// <summary>
    /// Saves equipped items
    /// </summary>
    /// <seealso cref="Scripts.Model.SaveLoad.SaveObjects.IdSaveObject{Scripts.Model.Items.EquippableItem}" />
    [Serializable]
    public sealed class EquipItemSave : IdSaveObject<EquippableItem> {

        /// <summary>
        /// The equip type save
        /// </summary>
        public EquipTypeSave EquipTypeSave;

        /// <summary>
        /// Initializes a new instance of the <see cref="EquipItemSave"/> class.
        /// </summary>
        /// <param name="equipTypeSave">The equip type save.</param>
        /// <param name="type">The type.</param>
        public EquipItemSave(EquipTypeSave equipTypeSave, Type type) : base(type) {
            this.EquipTypeSave = equipTypeSave;
        }
    }

    /// <summary>
    /// Saves character's equipment
    /// </summary>
    /// <seealso cref="System.Collections.Generic.IEnumerable{Scripts.Model.SaveLoad.SaveObjects.EquipItemSave}" />
    /// <seealso cref="System.Collections.Generic.IEnumerable{Scripts.Model.SaveLoad.SaveObjects.EquipmentSave.EquipBonus}" />
    [Serializable]
    public class EquipmentSave : IEnumerable<EquipItemSave>, IEnumerable<EquipmentSave.EquipBonus> {

        /// <summary>
        ///
        /// </summary>
        [Serializable]
        public struct EquipBonus {

            /// <summary>
            /// The stat
            /// </summary>
            public StatTypeSave Stat;

            /// <summary>
            /// The bonus
            /// </summary>
            public int Bonus;
        }

        /// <summary>
        ///
        /// </summary>
        [Serializable]
        public class EquipBuff {

            /// <summary>
            /// The equip type
            /// </summary>
            public EquipTypeSave EquipType;

            /// <summary>
            /// The buff
            /// </summary>
            public BuffSave Buff;
        }

        /// <summary>
        /// The equipped
        /// </summary>
        public List<EquipItemSave> Equipped;

        /// <summary>
        /// The buffs
        /// </summary>
        public List<EquipBuff> Buffs;

        /// <summary>
        /// The bonuses
        /// </summary>
        public List<EquipBonus> Bonuses;

        /// <summary>
        /// Initializes a new instance of the <see cref="EquipmentSave"/> class.
        /// </summary>
        /// <param name="equipped">The equipped.</param>
        /// <param name="buffs">The buffs.</param>
        /// <param name="bonuses">The bonuses.</param>
        public EquipmentSave(List<EquipItemSave> equipped, List<EquipBuff> buffs, List<EquipBonus> bonuses) {
            this.Equipped = equipped;
            this.Buffs = buffs;
            this.Bonuses = bonuses;
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator<EquipItemSave> IEnumerable<EquipItemSave>.GetEnumerator() {
            return Equipped.GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return Equipped.GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator<EquipBonus> IEnumerable<EquipBonus>.GetEnumerator() {
            return Bonuses.GetEnumerator();
        }
    }

    /// <summary>
    /// Saves buffs
    /// </summary>
    /// <seealso cref="Scripts.Model.SaveLoad.SaveObjects.IdSaveObject{Scripts.Model.Buffs.Buff}" />
    [Serializable]
    public class BuffSave : IdSaveObject<Buff> {

        /// <summary>
        ///
        /// </summary>
        public enum SaveType {

            /// <summary>
            /// The unknown
            /// </summary>
            UNKNOWN = 0,

            /// <summary>
            /// The caster in party
            /// </summary>
            CASTER_IN_PARTY,

            /// <summary>
            /// The caster not in party
            /// </summary>
            CASTER_NOT_IN_PARTY
        }

        /// <summary>
        /// The type
        /// </summary>
        public SaveType Type;

        /// <summary>
        /// The turns remaining
        /// </summary>
        public int TurnsRemaining;

        /// <summary>
        /// The stat copy
        /// </summary>
        public CharacterStatsSave StatCopy;

        /// <summary>
        /// The caster character identifier
        /// </summary>
        public int CasterCharacterId;

        /// <summary>
        /// The party index
        /// </summary>
        public int PartyIndex; // Setup in Party

        /// <summary>
        /// Initializes a new instance of the <see cref="BuffSave"/> class.
        /// </summary>
        /// <param name="turnsRemaining">The turns remaining.</param>
        /// <param name="statCopy">The stat copy.</param>
        /// <param name="casterId">The caster identifier.</param>
        /// <param name="type">The type.</param>
        public BuffSave(int turnsRemaining, CharacterStatsSave statCopy, int casterId, Type type) : base(type) {
            this.TurnsRemaining = turnsRemaining;
            this.StatCopy = statCopy;
            this.CasterCharacterId = casterId;
            this.PartyIndex = -1;
        }

        /// <summary>
        /// Setups as caster in party.
        /// </summary>
        /// <param name="partyIndex">Index of the party.</param>
        public void SetupAsCasterInParty(int partyIndex) {
            this.Type = SaveType.CASTER_IN_PARTY;
            this.PartyIndex = partyIndex;
            this.StatCopy = null;
            this.CasterCharacterId = Character.UNKNOWN_ID;
        }

        /// <summary>
        /// Setups as caster not in party.
        /// </summary>
        public void SetupAsCasterNotInParty() {
            this.Type = SaveType.CASTER_NOT_IN_PARTY;
            this.PartyIndex = -1;
            this.CasterCharacterId = Character.UNKNOWN_ID;
        }
    }

    /// <summary>
    /// Saves character's buffs
    /// </summary>
    /// <seealso cref="System.Collections.Generic.IEnumerable{Scripts.Model.SaveLoad.SaveObjects.BuffSave}" />
    [Serializable]
    public class CharacterBuffsSave : IEnumerable<BuffSave> {

        /// <summary>
        /// The buff saves
        /// </summary>
        public List<BuffSave> BuffSaves;

        /// <summary>
        /// The stat bonus saves
        /// </summary>
        public List<StatBonusSave> StatBonusSaves;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacterBuffsSave"/> class.
        /// </summary>
        /// <param name="buffSaves">The buff saves.</param>
        public CharacterBuffsSave(List<BuffSave> buffSaves, List<StatBonusSave> statBonusSaves) {
            this.BuffSaves = buffSaves;
            this.StatBonusSaves = statBonusSaves;
        }

        /// <summary>
        /// Setups the buff caster from save.
        /// </summary>
        /// <param name="bs">The bs.</param>
        /// <param name="partyMembers">The party members.</param>
        /// <returns></returns>
        public static Buff SetupBuffCasterFromSave(BuffSave bs, List<Character> partyMembers) {
            Buff b = bs.CreateObjectFromID();
            b.InitFromSaveObject(bs);

            Characters.Stats caster = null;
            int id = 0;
            switch (bs.Type) {
                case BuffSave.SaveType.CASTER_IN_PARTY:
                    Util.Assert(bs.PartyIndex >= 0, "Party index is negative.");
                    Character character = partyMembers[bs.PartyIndex];
                    caster = character.Stats;
                    id = character.Id;
                    break;

                // Spoof their stats, and equipment too!
                case BuffSave.SaveType.CASTER_NOT_IN_PARTY:
                    Util.Assert(bs.StatCopy != null, "Statcopy is null.");
                    caster = new Characters.Stats();
                    caster.SetupTemporarySaveFields(true);
                    caster.InitFromSaveObject(bs.StatCopy);
                    id = Character.UNKNOWN_ID;
                    break;

                default:
                    Util.Assert(false, "Unknown SaveType");
                    break;
            }
            b.Caster = new BuffParams(caster, id);

            return b;
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return BuffSaves.GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator<BuffSave> IEnumerable<BuffSave>.GetEnumerator() {
            return BuffSaves.GetEnumerator();
        }
    }

    /// <summary>
    /// Saves a character
    /// </summary>
    [Serializable]
    public class CharacterSave {

        /// <summary>
        /// The identifier
        /// </summary>
        public int Id;

        /// <summary>
        /// The stats
        /// </summary>
        public CharacterStatsSave Stats;

        /// <summary>
        /// The buffs
        /// </summary>
        public CharacterBuffsSave Buffs;

        /// <summary>
        /// The look
        /// </summary>
        public LookSave Look;

        /// <summary>
        /// The spells
        /// </summary>
        public CharacterSpellBooksSave Spells;

        /// <summary>
        /// The brain
        /// </summary>
        public BrainSave Brain;

        // Party shares an inventory, so we associate it with the party
        /// <summary>
        /// The equipment
        /// </summary>
        public EquipmentSave Equipment;

        /// <summary>
        /// The flags
        /// </summary>
        public List<Characters.Flag> Flags;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacterSave"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="stats">The stats.</param>
        /// <param name="buffs">The buffs.</param>
        /// <param name="look">The look.</param>
        /// <param name="spells">The spells.</param>
        /// <param name="brain">The brain.</param>
        /// <param name="equipment">The equipment.</param>
        public CharacterSave(
            int id,
            List<Characters.Flag> flags,
            CharacterStatsSave stats,
            CharacterBuffsSave buffs,
            LookSave look,
            CharacterSpellBooksSave spells,
            BrainSave brain,
            EquipmentSave equipment) {
            this.Id = id;
            this.Flags = flags;
            this.Stats = stats;
            this.Buffs = buffs;
            this.Look = look;
            this.Spells = spells;
            this.Brain = brain;
            this.Equipment = equipment;
        }
    }

    /// <summary>
    ///Saves a party
    /// </summary>
    [Serializable]
    public sealed class PartySave {

        /// <summary>
        /// The characters
        /// </summary>
        public List<CharacterSave> Characters;

        /// <summary>
        /// The inventory
        /// </summary>
        public InventorySave Inventory;

        /// <summary>
        /// Initializes a new instance of the <see cref="PartySave"/> class.
        /// </summary>
        /// <param name="characters">The characters.</param>
        /// <param name="inventory">The inventory.</param>
        public PartySave(List<CharacterSave> characters, InventorySave inventory) {
            this.Characters = characters;
            this.Inventory = inventory;
        }
    }

    /// <summary>
    /// Saves game flags
    /// </summary>
    [Serializable]
    public sealed class FlagsSave {

        /// <summary>
        /// The flags
        /// </summary>
        public Flags Flags;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlagsSave"/> class.
        /// </summary>
        /// <param name="flags">The flags.</param>
        public FlagsSave(Flags flags) {
            this.Flags = flags;
        }
    }

    /// <summary>
    /// SAVE the World
    /// </summary>
    [Serializable]
    public sealed class WorldSave {

        /// <summary>
        /// The flags
        /// </summary>
        public FlagsSave Flags;

        /// <summary>
        /// The party
        /// </summary>
        public PartySave Party;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorldSave"/> class.
        /// </summary>
        /// <param name="party">The party.</param>
        /// <param name="flags">The flags.</param>
        public WorldSave(PartySave party, FlagsSave flags) {
            this.Party = party;
            this.Flags = flags;
        }
    }
}