using Scripts.Model.Interfaces;
using Scripts.Model.SaveLoad;
using Scripts.Model.SaveLoad.SaveObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using Scripts.Model.Characters;

namespace Scripts.Model.Stats {

    /// <summary>
    /// Type-safe enum representing the various stat types in the game.
    /// </summary>
    public sealed class StatType : IComparable<StatType>, INameable, ISaveable<StatTypeSave>, ICostable {
        private static readonly HashSet<StatType> allTypes = new HashSet<StatType>(new IdentityEqualityComparer<StatType>());

        private static readonly IDictionary<BoundType, Bounds> attributeBounds = new Dictionary<BoundType, Bounds>() {
            { BoundType.RESOURCE, new Bounds(0, int.MaxValue) },
            { BoundType.ASSIGNABLE, new Bounds(0, 9999) },
        };

        public static readonly StatType STRENGTH = new StatType(
                                                                STANDARD_INCREASE_FROM_STAT_POINT_AMOUNT,
                                                                BoundType.ASSIGNABLE,
                                                                          "Strength",
                                                                          "fist",
                                                                          "Increases basic attack damage.",
                                                                          Color.red);

        public static readonly StatType INTELLECT = new StatType(STANDARD_INCREASE_FROM_STAT_POINT_AMOUNT,
                                                                BoundType.ASSIGNABLE,
                                                                           "Intellect",
                                                                           "light-bulb",
                                                                           "Increases spell effects.",
                                                                           Color.cyan);

        public static readonly StatType AGILITY = new StatType(BOOSTED_INCREASE_FROM_STAT_POINT_AMOUNT,
                                                                BoundType.ASSIGNABLE,
                                                                         "Agility",
                                                                         "power-lightning",
                                                                         "Increases critical hit rate and accuracy.",
                                                                         Color.green);

        public static readonly StatType VITALITY = new StatType(BOOSTED_INCREASE_FROM_STAT_POINT_AMOUNT,
                                                                BoundType.ASSIGNABLE,
                                                                          "Vitality",
                                                                          "health-normal",
                                                                          "Increases Health.",
                                                                          Color.yellow);

        public static readonly HashSet<StatType> ASSIGNABLES = new HashSet<StatType>() { STRENGTH, INTELLECT, AGILITY, VITALITY };

        public static readonly StatType HEALTH = new StatType(BoundType.RESOURCE,
                                                              "Health",
                                                              "hearts",
                                                              "Life state. Units are defeated if this reaches 0.",
                                                              Color.green,
                                                              Color.red);

        public static readonly StatType SKILL = new StatType(BoundType.RESOURCE,
                                                                     "Skill",
                                                                     "concentration-orb",
                                                                     "A spell resource that is replenished on basic attacks.",
                                                                     Color.yellow);

        public static readonly StatType MANA = new StatType(BoundType.RESOURCE,
                                                                    "Mana",
                                                                    "water-drop",
                                                                    "Magical resources.",
                                                                    Color.cyan);

        public static readonly StatType CHARGE = new StatType(BoundType.RESOURCE,
                                                                      "Charge",
                                                                      "fox-head",
                                                                      "Actions may be performed when full.",
                                                                      Color.white);

        public static readonly StatType CORRUPTION = new StatType(BoundType.RESOURCE,
                                                                          "Corruption",
                                                                          "fox-head",
                                                                          "Corruption level.",
                                                                          Color.magenta);

        public static readonly StatType EXPERIENCE = new StatType(BoundType.RESOURCE,
                                                                    "Experience",
                                                                    "upgrade",
                                                                    "Needed to level up.",
                                                                    Color.white);

        public static readonly HashSet<StatType> RESOURCES = new HashSet<StatType>() { HEALTH, SKILL, MANA, CHARGE, CORRUPTION, EXPERIENCE };
        public static readonly HashSet<StatType> RESTORED = new HashSet<StatType>() { HEALTH, MANA, STRENGTH, AGILITY, INTELLECT, VITALITY };

        public readonly Bounds Bounds;
        public readonly string Name;
        public readonly Sprite Sprite;
        public readonly string Description;
        public readonly int StatPointIncreaseAmount;
        public readonly Color Color;
        public readonly Color NegativeColor;

        private readonly int order;

        private const int STANDARD_INCREASE_FROM_STAT_POINT_AMOUNT = 1;
        private const int BOOSTED_INCREASE_FROM_STAT_POINT_AMOUNT = 2;
        private static int orderCounter;

        private StatType(
            int statPointIncreaseAmount,
            BoundType boundType,
            string name,
            string spriteLoc,
            string description,
            Color color,
            Color negativeColor) {
            attributeBounds.TryGetValue(boundType, out Bounds);
            this.StatPointIncreaseAmount = statPointIncreaseAmount;
            this.Name = name;
            this.Sprite = Util.GetSprite(spriteLoc);
            this.Description = description;
            this.Color = color;
            this.order = orderCounter++;
            this.NegativeColor = negativeColor;
            allTypes.Add(this);
        }

        private StatType(
            BoundType boundType,
            string name,
            string spriteLoc,
            string description,
            Color color,
            Color negativeColor) : this(0, boundType, name, spriteLoc, description, color, negativeColor) { }

        private StatType(
            BoundType boundType,
            string name,
            string spriteLoc,
            string description,
            Color color) : this(0, boundType, name, spriteLoc, description, color, Color.grey) { }

        private StatType(
            int statPointIncreaseAmount,
            BoundType boundType,
            string name,
            string spriteLoc,
            string description,
            Color color) : this(statPointIncreaseAmount, boundType, name, spriteLoc, description, color, Color.grey) { }

        public static ICollection<StatType> AllTypes {
            get {
                return new ReadOnlyCollection<StatType>(allTypes.ToArray());
            }
        }

        public string ColoredName {
            get {
                return Util.ColorString(Name, Color);
            }
        }

        string INameable.Name {
            get {
                return Name;
            }
        }

        public override bool Equals(object obj) {
            return this == obj;
        }

        public override int GetHashCode() {
            return order;
        }

        public override string ToString() {
            return this.ColoredName;
        }

        public Color DetermineColor(float value) {
            Color c = Color.grey;
            if (value < 0) {
                c = this.NegativeColor;
            } else if (value == 0) {
                c = Color.grey;
            } else {
                c = this.Color;
            }
            return c;
        }

        public int CompareTo(StatType other) {
            return this.order.CompareTo(other.order);
        }

        public int Clamp(int amountToBeClamped) {
            return Mathf.Clamp(amountToBeClamped, Bounds.Low, Bounds.High);
        }

        public StatTypeSave GetSaveObject() {
            return new StatTypeSave(this);
        }

        public void InitFromSaveObject(StatTypeSave saveObject) {
            // Nothing
        }

        public bool CanAfford(int amount, Character characterToCheck) {
            Util.Assert(amount > 0, "Amount must be nonnegative.");
            return characterToCheck.Stats.GetStatCount(Characters.Stats.Get.MOD, this) >= amount;
        }

        public void DeductCostFromCharacter(int amount, Character unitToDeductFrom) {
            Util.Assert(CanAfford(amount, unitToDeductFrom));
            unitToDeductFrom.Stats.AddToStat(this, Characters.Stats.Set.MOD, -amount);
        }

        public string GetName() {
            return this.ColoredName;
        }
    }
}