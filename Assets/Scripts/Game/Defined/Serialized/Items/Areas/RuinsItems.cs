using Scripts.Game.Defined.Serialized.Buffs;
using Scripts.Game.Defined.Serialized.Spells;
using Scripts.Game.Defined.Spells;
using Scripts.Model.Buffs;
using Scripts.Model.Characters;
using Scripts.Model.Items;
using Scripts.Model.Spells;
using Scripts.Model.Stats;
using System.Collections.Generic;
using System;
using Scripts.Game.Defined.Unserialized.Items;
using Scripts.Model.Pages;

namespace Scripts.Game.Defined.Serialized.Items {
    // Random drops

    public class BrokenSword : EquippableItem {

        public BrokenSword() : base("shattered-sword", EquipType.WEAPON, 5, "Broken Sword", "Holding it feels uncomfortable.") {
            AddFlatStatBonus(StatType.STRENGTH, 1);
            AddFlatStatBonus(StatType.VITALITY, -2);
            AddFlatStatBonus(StatType.INTELLECT, -2);
        }
    }

    public class SpiritRobes : EquippableItem {

        public SpiritRobes() : base("robe", EquipType.ARMOR, 10, "Spirit Robes", "A weightless set of robes.") {
            AddFlatStatBonus(StatType.AGILITY, 1);
            AddFlatStatBonus(StatType.INTELLECT, 2);
            AddFlatStatBonus(StatType.VITALITY, -1);
        }
    }

    public class IllusionOrb : EquippableItem {

        public IllusionOrb() : base("ubisoft-sun", EquipType.OFFHAND, 20, "Illusion Orb", "A swirling ball of magic.") {
            AddFlatStatBonus(StatType.INTELLECT, 2);
        }
    }

    public class GhastlyDefender : EquippableItem {

        public GhastlyDefender() : base("sacrificial-dagger", EquipType.OFFHAND, 20, "Ghastly Defender", "A mini sword, used for parrying attacks.") {
            AddFlatStatBonus(StatType.STRENGTH, 1);
            AddFlatStatBonus(StatType.AGILITY, 2);
            AddFlatStatBonus(StatType.VITALITY, -1);
        }
    }

    public class BigSword : EquippableItem {

        public BigSword() : base("rune-sword", EquipType.WEAPON, 20, "Big Sword", "A massive, hulking sword.") {
            AddFlatStatBonus(StatType.STRENGTH, 3);
            AddFlatStatBonus(StatType.AGILITY, -5);
        }
    }

    public class BigArmor : EquippableItem {

        public BigArmor() : base("layered-armor", EquipType.ARMOR, 20, "Big Armor", "A very heavy chestpiece.") {
            AddFlatStatBonus(StatType.AGILITY, -5);
            AddFlatStatBonus(StatType.VITALITY, 10);
        }
    }

    public class WornDagger : EquippableItem {

        public WornDagger() : base("plain-dagger", EquipType.WEAPON, 10, "Worn Dagger", "Perfect for pacifists and killers alike.") {
            AddFlatStatBonus(StatType.STRENGTH, 1);
            AddFlatStatBonus(StatType.AGILITY, 1);
        }
    }

    public class RealKnife : EquippableItem {

        public RealKnife() : base("curvy-knife", EquipType.WEAPON, 20, "Real Knife", "Only a killer would use this weapon.") {
            AddFlatStatBonus(StatType.STRENGTH, 2);
            AddFlatStatBonus(StatType.AGILITY, 1);
        }
    }

    public class UsedBandage : HealingItem {

        public UsedBandage()
            : base("Used Bandage", "bandage-roll", "A dirty used bandage.", 5, 2) { }
    }

    public class CleanBandage : HealingItem {

        public CleanBandage()
            : base("Bandage", "bandage-roll", "A clean bandage.", 10, 5) { }
    }

    public class SpiritOrb : HealingItem {

        public SpiritOrb()
            : base("Spirit Orb", "gold-shell", "A dusty orb left by a spirit.", 20, 1, true) { }
    }

    public class SpiritDust : ManaItem {

        public SpiritDust()
            : base("Spirit Dust", "swap-bag", "A dusty substance left by a spirit.", 10, 10) { }
    }

    public class GhostArmor : EquippableItem {

        public GhostArmor() : base("armor-vest", EquipType.ARMOR, 15, "Cursed Mail", "Heavy and makes you feal uneasy.") {
            AddFlatStatBonus(StatType.VITALITY, 5);
            AddFlatStatBonus(StatType.AGILITY, -10);
            AddFlatStatBonus(StatType.INTELLECT, -5);
        }
    }

    public class SilverBoot : EquippableItem {

        public SilverBoot() : base("walking-boot", EquipType.WEAPON, 50, "Silver Boot", "A boot made of silver. Makes for a poor weapon.") {
            AddFlatStatBonus(StatType.STRENGTH, 1);
            AddFlatStatBonus(StatType.AGILITY, -10);
        }
    }

    public class Wand : EquippableItem {

        public Wand() : base("orb-wand", EquipType.WEAPON, 5, "Lesser Wand", "Holding this wooden stick makes you feel slightly smarter.") {
            AddFlatStatBonus(StatType.INTELLECT, 1);
        }
    }

    public class BetterWand : EquippableItem {

        public BetterWand() : base("lunar-wand", EquipType.WEAPON, 15, "Greater Wand", "A magical stick. Not to be confused with a non-magical wooden stick.") {
            AddFlatStatBonus(StatType.INTELLECT, 2);
        }
    }

    public class MinorVitalityTrinket : SingleStatTrinket {

        public MinorVitalityTrinket() : base(StatType.VITALITY, 20, 2, "Wellness") {
        }
    }

    public class MinorAgilityTrinket : SingleStatTrinket {

        public MinorAgilityTrinket() : base(StatType.AGILITY, 20, 2, "Speed") {
        }
    }

    public class MinorIntellectTrinket : SingleStatTrinket {

        public MinorIntellectTrinket() : base(StatType.INTELLECT, 20, 1, "Wisdom") {
        }
    }

    public class MinorStrengthTrinket : SingleStatTrinket {

        public MinorStrengthTrinket() : base(StatType.STRENGTH, 20, 1, "Power") {
        }
    }

    public class HorrorEmblem : EquippableItem {

        public HorrorEmblem() : base("cursed-star", EquipType.TRINKET, 50, "Emblem of Horrors", "A horrifying emblem.") {
            AddFlatStatBonus(StatType.INTELLECT, 3);
            AddFlatStatBonus(StatType.STRENGTH, 3);
            AddFlatStatBonus(StatType.AGILITY, -1);
            AddFlatStatBonus(StatType.VITALITY, -1);
        }
    }

    public class MadnessStaff : EquippableItem {

        public MadnessStaff() : base("wizard-staff", EquipType.WEAPON, 50, "Staff of Madness", "R'lyeh wgah'nagl fhtagn.") {
            AddFlatStatBonus(StatType.INTELLECT, 5);
            AddFlatStatBonus(StatType.STRENGTH, 5);
            AddFlatStatBonus(StatType.AGILITY, -5);
            AddFlatStatBonus(StatType.VITALITY, -2);
        }

        public override PermanentBuff CreateBuff() {
            return new RegenerateMana();
        }
    }

    // Shop items

    public class Apple : HealingItem {
        private const int HEALING_AMOUNT = 10;

        public Apple() : base("Apple", "shiny-apple", "A magical apple that heals wounds.", 10, 10) {
        }
    }

    public class IdentifyScroll : ConsumableItem {

        public IdentifyScroll()
            : base("tied-scroll", 5, TargetType.ANY, "Scroll of Identify", string.Format("Scroll that reveals info about a target.")) {
            flags.Remove(Model.Items.Flag.USABLE_OUT_OF_COMBAT);
        }

        public override IList<SpellEffect> GetEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] { new AddBuff(new Model.Buffs.BuffParams(caster.Stats, caster.Id), target.Buffs, new BasicChecked()) };
        }
    }

    public class RevivalSeed : ConsumableItem {
        private const int HEALTH_RECOVERY_PERCENTAGE = 50;

        public RevivalSeed()
            : base(
                  "acorn",
                  100,
                  TargetType.ONE_ALLY,
                  "Revival Seed",
                  string.Format("Use on an ally to restore them by {0}% of their missing {1}. Can be used on fallen allies.",
                      HEALTH_RECOVERY_PERCENTAGE,
                      StatType.HEALTH.ColoredName)) {
        }

        public override IList<SpellEffect> GetEffects(Page page, Character caster, Character target) {
            return new SpellEffect[] {
                new RestoreMissingStatPercent(target.Stats, StatType.HEALTH, HEALTH_RECOVERY_PERCENTAGE)
            };
        }

        protected override bool IsMeetOtherRequirements(Character caster, Character target) {
            return true;
        }
    }

    public class CheckTome : Tome {

        public CheckTome()
            : base(2, 25, new Check()) { }
    }

    public class CrushingBlowTome : Tome {

        public CrushingBlowTome()
            : base(2, 25, new CrushingBlow()) { }
    }

    public class HealTome : Tome {

        public HealTome()
            : base(4, 50, new PlayerHeal()) { }
    }

    public class DefendTome : Tome {

        public DefendTome()
            : base(4, 50, new SetupDefend()) { }
    }

    public class RegenArmor : EquippableItem {

        public RegenArmor() : base("chain-mail", EquipType.ARMOR, 500, "Flan's Mail", "A soothing mail that heals wounds. Smells like eggs.") {
            AddFlatStatBonus(StatType.AGILITY, -5);
            AddFlatStatBonus(StatType.VITALITY, 5);
        }

        public override PermanentBuff CreateBuff() {
            return new RegenerateHealth();
        }
    }
}