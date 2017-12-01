using Scripts.Game.Defined.Serialized.Buffs;
using Scripts.Game.Defined.Unserialized.Items;
using Scripts.Model.Buffs;
using Scripts.Model.Items;
using Scripts.Model.Stats;

namespace Scripts.Game.Defined.Serialized.Items {

    public static class Constants {
        public const int BASIC_HEALING_PRICE = 250;
        public const int ADVANCED_HEALING_PRICE = 750;

        public const int ARMOR_PRICE = 7500;
        public const int WEAPON_PRICE = 5000;
        public const int OFFHAND_PRICE = 2500;
        public const int TRINKET_PRICE = 2000;
    }

    // Shop items
    public class HealingPotion : HealingItem {
        public const string NAME = "Hyper Potion";

        public HealingPotion()
            : base(
                  NAME,
                  "potion-ball",
                  "Smells like apples and fish.",
                  Constants.BASIC_HEALING_PRICE,
                  25,
                  false) {
        }
    }

    public class LifeGem : HealingItem {

        public LifeGem()
            : base(
                  "Life Ruby",
                  "emerald",
                  string.Format("Made of concentrated {0}s and <color=red>LOVE</color>.", HealingPotion.NAME),
                  Constants.ADVANCED_HEALING_PRICE,
                  50,
                  true) {
        }
    }

    public class ManaPotion : ManaItem {
        public const string NAME = "Syrup";

        public ManaPotion()
            : base(
                  NAME,
                  "round-bottom-flask",
                  "Made of concentrated water. Don't ask how that works.",
                  Constants.BASIC_HEALING_PRICE,
                  50) {
        }
    }

    public class ManaGem : ManaItem {

        // "saphir" is not a typo, that's the icon's name.
        public ManaGem()
            : base(
                  "Mana Opal",
                  "saphir",
                  string.Format("Made of concentrated {0}. How does that even work?",
                      ManaPotion.NAME),
                  Constants.ADVANCED_HEALING_PRICE,
                  100) { }
    }

    public class FinalSword : EquippableItem {

        public FinalSword() : base("relic-blade", EquipType.WEAPON, Constants.WEAPON_PRICE, "Whirlwind Sword", "A weightless sword that cuts like wind.") {
            AddFlatStatBonus(StatType.AGILITY, 15);
            AddFlatStatBonus(StatType.STRENGTH, 10);
        }
    }

    public class FinalMeleeArmor : EquippableItem {

        public FinalMeleeArmor() : base("lamellar", EquipType.ARMOR, Constants.ARMOR_PRICE, "Runed Platebody", "Oziach's finest armor. Enchanted to be weightless.") {
            AddFlatStatBonus(StatType.VITALITY, 20);
            AddFlatStatBonus(StatType.INTELLECT, -10);
        }
    }

    public class FinalMeleeTrinket : EquippableItem {

        public FinalMeleeTrinket() : base("mineral-heart", EquipType.TRINKET, Constants.TRINKET_PRICE, "Mountain Heart", "Holds the essence of the earth.") {
            AddFlatStatBonus(StatType.STRENGTH, 5);
            AddFlatStatBonus(StatType.VITALITY, 5);
        }
    }

    public class FinalMeleeOffhand : EquippableItem {

        public FinalMeleeOffhand() : base("bordered-shield", EquipType.OFFHAND, Constants.OFFHAND_PRICE, "Runic Bulwark", "A shield from an old time.") {
            AddFlatStatBonus(StatType.VITALITY, 15);
        }
    }

    public class FinalStaff : EquippableItem {

        public FinalStaff() : base("skull-staff", EquipType.WEAPON, Constants.WEAPON_PRICE, "Death's Charge", "Heavier than death.") {
            AddFlatStatBonus(StatType.INTELLECT, 15);
            AddFlatStatBonus(StatType.VITALITY, 10);
        }
    }

    public class FinalCasterArmor : EquippableItem {

        public FinalCasterArmor() : base("wing-cloak", EquipType.ARMOR, Constants.ARMOR_PRICE, "Seraphim's Cloak", "Cloak from a fallen angel.") {
            AddFlatStatBonus(StatType.INTELLECT, 10);
            AddFlatStatBonus(StatType.VITALITY, 10);
        }
    }

    public class FinalCasterOffhand : EquippableItem {

        public FinalCasterOffhand() : base("spell-book", EquipType.OFFHAND, Constants.OFFHAND_PRICE, "The Black Book", "Voices are coming out from it...") {
            AddFlatStatBonus(StatType.INTELLECT, 5);
        }
    }

    public class FinalCasterTrinket : EquippableItem {

        public FinalCasterTrinket() : base("fluffy-wing", EquipType.TRINKET, Constants.TRINKET_PRICE, "Angel Wing", "You feel lightheaded holding this.") {
            AddFlatStatBonus(StatType.INTELLECT, 5);
            AddFlatStatBonus(StatType.AGILITY, 5);
        }
    }

    // Final boss items

    public class EvilCloneArmor : EquippableItem {

        public EvilCloneArmor() : base("chain-mail", EquipType.ARMOR, 1, "Infernal Platebody", "A powerful armor pulsing with firey energy.") {
        }

        public override PermanentBuff CreateBuff() {
            return new FlamingArmor();
        }
    }

    public class EvilCloneTrinket : EquippableItem {
        private const int STAT_AMOUNT = 5;

        public EvilCloneTrinket()
            : base(
                  Util.GetSprite("gem-pendant"),
                  EquipType.TRINKET,
                  1,
                  "Healer's Vow",
                  "A pendant that greatly boosts magical power.") {
            AddFlatStatBonus(StatType.AGILITY, STAT_AMOUNT);
            AddFlatStatBonus(StatType.INTELLECT, STAT_AMOUNT);
        }

        public override PermanentBuff CreateBuff() {
            return new HighManaRegeneration();
        }
    }

    public class EvilFriendTrinket : EquippableItem {
        private const int STAT_AMOUNT = 10;

        public EvilFriendTrinket()
            : base(
                  Util.GetSprite("gem-pendant"),
                  EquipType.TRINKET,
                  1,
                  "Knight's Vow",
                  "A pendant that greatly boosts physical power.") {
            AddFlatStatBonus(StatType.STRENGTH, STAT_AMOUNT);
            AddFlatStatBonus(StatType.AGILITY, STAT_AMOUNT);
            AddFlatStatBonus(StatType.VITALITY, STAT_AMOUNT);
        }

        public override PermanentBuff CreateBuff() {
            return new SkillRegeneration();
        }
    }
}