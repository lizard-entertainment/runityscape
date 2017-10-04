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

namespace Scripts.Game.Defined.Serialized.Items {
    // Random drops

    public class BrokenSword : EquippableItem {

        public BrokenSword() : base(EquipType.WEAPON, 10, "Broken Sword", "Holding it feels uncomfortable.") {
            AddFlatStatBonus(StatType.STRENGTH, 1);
            AddFlatStatBonus(StatType.VITALITY, -2);
            AddFlatStatBonus(StatType.INTELLECT, -2);
        }
    }

    public class GhostArmor : EquippableItem {

        public GhostArmor() : base(EquipType.ARMOR, 15, "Cursed Mail", "Heavy and makes you feal uneasy.") {
            AddFlatStatBonus(StatType.VITALITY, 5);
            AddFlatStatBonus(StatType.AGILITY, -10);
            AddFlatStatBonus(StatType.INTELLECT, -5);
        }
    }

    public class SilverBoot : EquippableItem {

        public SilverBoot() : base(EquipType.WEAPON, 50, "Silver Boot", "I'm not really sure why you'd use it as a weapon, but I guess you can.") {
            AddFlatStatBonus(StatType.STRENGTH, 1);
            AddFlatStatBonus(StatType.AGILITY, -10);
        }
    }

    public class Wand : EquippableItem {

        public Wand() : base(EquipType.WEAPON, 5, "Lesser Wand", "Holding this wooden stick makes you feel slightly smarter.") {
            AddFlatStatBonus(StatType.INTELLECT, 1);
        }
    }

    public class MadnessStaff : EquippableItem {

        public MadnessStaff() : base(EquipType.WEAPON, 25, "Staff of Madness", "R'lyeh wgah'nagl fhtagn.") {
            AddFlatStatBonus(StatType.INTELLECT, 5);
            AddFlatStatBonus(StatType.STRENGTH, 5);
            AddFlatStatBonus(StatType.AGILITY, -5);
            AddFlatStatBonus(StatType.VITALITY, -2);
        }

        public override Buff CreateBuff() {
            return new RegenerateMana();
        }
    }

    // Shop items

    public class Apple : ConsumableItem {
        private const int HEALING_AMOUNT = 10;

        public Apple() : base(5, TargetType.SINGLE_ALLY, "Apple", string.Format("A juicy apple. Restores {0} {1}.", HEALING_AMOUNT, StatType.HEALTH.ColoredName)) {
        }

        public override IList<SpellEffect> GetEffects(Character caster, Character target) {
            return new SpellEffect[] { new AddToModStat(target.Stats, StatType.HEALTH, HEALING_AMOUNT) };
        }
    }

    public class IdentifyScroll : ConsumableItem {

        public IdentifyScroll()
            : base(5, TargetType.ANY, "Scroll of Identify", string.Format("Scroll that reveals info about a target.")) {
        }

        public override IList<SpellEffect> GetEffects(Character caster, Character target) {
            return new SpellEffect[] { new AddBuff(new Model.Buffs.BuffParams(caster.Stats, caster.Id), target.Buffs, new Checked()) };
        }
    }

    public class RevivalSeed : ConsumableItem {
        private const int HEALTH_RECOVERY_PERCENTAGE = 50;

        public RevivalSeed()
            : base(500,
                  TargetType.SINGLE_ALLY,
                  "Revival Seed",
                  string.Format("Use on a fallen ally to revive them to {0}% {1}.",
                      HEALTH_RECOVERY_PERCENTAGE,
                      StatType.HEALTH.ColoredName)) {
        }

        public override IList<SpellEffect> GetEffects(Character caster, Character target) {
            int revivedHealth = (int)(target.Stats.GetMissingStatCount(StatType.HEALTH) * (HEALTH_RECOVERY_PERCENTAGE / 100f));
            return new SpellEffect[] {
                new AddToModStat(target.Stats, StatType.HEALTH, revivedHealth)
            };
        }

        /// <summary>
        /// Can be used on the dead, and only the dead!
        /// </summary>
        /// <param name="caster">The caster.</param>
        /// <param name="target">The target.</param>
        /// <returns>
        ///   <c>true</c> if caster can use the item on target; otherwise, <c>false</c>.
        /// </returns>
        protected override bool IsMeetOtherRequirements(Character caster, Character target) {
            return target.Stats.State == State.DEAD;
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

    public class Inventory1x6 : InventoryExpander {

        public Inventory1x6() : base(1, 6, 200, "6Pack") {
        }
    }

    public class RegenArmor : EquippableItem {

        public RegenArmor() : base(EquipType.ARMOR, 100, "Flan's Mail", "A soothing mail that heals wounds. Smells like eggs.") {
            AddFlatStatBonus(StatType.AGILITY, -5);
            AddFlatStatBonus(StatType.VITALITY, 5);
        }

        public override Buff CreateBuff() {
            return new RegenerateHealth();
        }
    }
}