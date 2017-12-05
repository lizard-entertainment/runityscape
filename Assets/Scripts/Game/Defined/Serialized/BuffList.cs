using Scripts.Game.Defined.Serialized.Spells;
using Scripts.Game.Defined.Spells;
using Scripts.Game.Defined.Unserialized.Buffs;
using Scripts.Game.Defined.Unserialized.Spells;
using Scripts.Model.Buffs;
using Scripts.Model.Characters;
using Scripts.Model.Spells;
using Scripts.Model.Stats;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Game.Defined.Serialized.Buffs {

    public class FlamingArmor : PermanentBuff {
        public const int IGNITION_CHANCE = 25;

        public FlamingArmor()
            : base(
                  Util.GetSprite("fire"),
                  "Flame Body",
                  String.Format("Basic attacks on this unit have a {0}% chance of igniting the attacker.", IGNITION_CHANCE)
                  ) {
        }

        public override bool IsReact(SingleSpell incomingSpell, Stats ownerOfThisBuff) {
            return incomingSpell.Target.Stats == ownerOfThisBuff
                && incomingSpell.SpellBook is Attack
                && incomingSpell.Result.IsDealDamage
                && Util.IsChance(IGNITION_CHANCE / 100.0f);
        }

        protected override void ReactHelper(SingleSpell spellCast, Stats ownerOfThisBuff) {
            spellCast.Result.AddEffect(
                new AddBuff(new BuffParams(ownerOfThisBuff, spellCast.Target.Id), spellCast.Caster.Buffs, new SuperIgnited()));
        }
    }

    public class AntiHeal : Buff {

        public AntiHeal() : base(8, Util.GetSprite("skull-crossed-bones"), "Antiheal", "Healing on this unit is nullified.", false) {
        }

        public override bool IsReact(SingleSpell spellToReactTo, Stats owner) {
            return (GetHealingElement(spellToReactTo) != null) && spellToReactTo.Target.Stats == owner;
        }

        protected override void ReactHelper(SingleSpell spellToReactTo, Stats owner) {
            SpellEffect healthIncrease = GetHealingElement(spellToReactTo);
            if (healthIncrease != null) {
                healthIncrease.Value = 0;
            }
        }

        private SpellEffect GetHealingElement(SingleSpell spellToReactTo) {
            SpellEffect healthIncrease = null;
            for (int i = 0; (i < spellToReactTo.Result.Effects.Count) && (healthIncrease == null); i++) {
                SpellEffect se = spellToReactTo.Result.Effects[i];
                AddToModStat addToModStat = se as AddToModStat;
                if (addToModStat != null && addToModStat.AffectedStat == StatType.HEALTH && addToModStat.Value > 0) {
                    healthIncrease = addToModStat;
                }
            }
            return healthIncrease;
        }
    }

    public class HealBoost : Buff {
        private int HEAL_MULTIPLIER = 2;

        public HealBoost() : base(3, Util.GetSprite("health-normal"), "Heal Buff", "Healing targeting this unit is doubled.", false) {
        }

        public override bool IsReact(SingleSpell spellToReactTo, Stats owner) {
            return (GetHealingElement(spellToReactTo) != null) && spellToReactTo.Target.Stats == owner;
        }

        protected override void ReactHelper(SingleSpell spellToReactTo, Stats owner) {
            SpellEffect healthIncrease = GetHealingElement(spellToReactTo);
            if (healthIncrease != null) {
                healthIncrease.Value = (int)(healthIncrease.Value * HEAL_MULTIPLIER);
            }
        }

        private SpellEffect GetHealingElement(SingleSpell spellToReactTo) {
            SpellEffect healthIncrease = null;
            for (int i = 0; (i < spellToReactTo.Result.Effects.Count) && (healthIncrease == null); i++) {
                SpellEffect se = spellToReactTo.Result.Effects[i];
                AddToModStat addToModStat = se as AddToModStat;
                if (addToModStat != null && addToModStat.AffectedStat == StatType.HEALTH && addToModStat.Value > 0) {
                    healthIncrease = addToModStat;
                }
            }
            return healthIncrease;
        }
    }

    public class FishShook : PermanentBuff {
        private const float FISHY_TARGET_MULTIPLIER = 2;
        private const float OTHER_TARGET_MULTIPLIER = 0.5f;

        public FishShook()
            : base(Util.GetSprite("fish"),
                  "Fish-Shook",
                  string.Format(
                      "Basic attacks do {0}x damage on fishy targets and {1}x damage on non-fish.", FISHY_TARGET_MULTIPLIER, OTHER_TARGET_MULTIPLIER)) {
        }

        public override bool IsReact(SingleSpell spellToReactTo, Stats owner) {
            return spellToReactTo.SpellBook is Attack && spellToReactTo.Result.IsDealDamage && spellToReactTo.Caster.Stats == owner;
        }

        protected override void ReactHelper(SingleSpell spellToReactTo, Stats owner) {
            float localDmgMult = 0;

            // is a fish
            if (spellToReactTo.Target.Look.Breed == Characters.Breed.FISH) {
                localDmgMult = FISHY_TARGET_MULTIPLIER;
            } else { // not a fish
                localDmgMult = OTHER_TARGET_MULTIPLIER;
            }

            SpellEffect healthDamage = null;
            for (int i = 0; (i < spellToReactTo.Result.Effects.Count) && (healthDamage == null); i++) {
                SpellEffect se = spellToReactTo.Result.Effects[i];
                AddToModStat addToModStat = se as AddToModStat;
                if (addToModStat != null && addToModStat.AffectedStat == StatType.HEALTH && addToModStat.Value < 0) {
                    healthDamage = addToModStat;
                }
            }
            if (healthDamage != null) {
                healthDamage.Value = (int)Math.Floor(healthDamage.Value * localDmgMult);
            }
        }
    }

    public class DamageResist : PermanentBuff {
        private const float DAMAGE_MULTIPLIER = 0.8f;

        public DamageResist()
            : base(Util.GetSprite("round-shield"),
                  "Damage Resist",
                  String.Format("Reduces incident damage from basic attacks by {0}%.",
                      (1 - DAMAGE_MULTIPLIER) * 100)) { }

        protected override void ReactHelper(SingleSpell spellToReactTo, Stats owner) {
            SpellEffect healthDamage = null;
            for (int i = 0; (i < spellToReactTo.Result.Effects.Count) && (healthDamage == null); i++) {
                SpellEffect se = spellToReactTo.Result.Effects[i];
                AddToModStat addToModStat = se as AddToModStat;
                if (addToModStat != null && addToModStat.AffectedStat == StatType.HEALTH && addToModStat.Value < 0) {
                    healthDamage = addToModStat;
                }
            }
            if (healthDamage != null) {
                int tempValue = healthDamage.Value;
                healthDamage.Value = (int)Math.Floor(healthDamage.Value * DAMAGE_MULTIPLIER);
            }
        }

        public override bool IsReact(SingleSpell spellToReactTo, Stats owner) {
            return spellToReactTo.SpellBook is Attack && spellToReactTo.Target.Stats == owner;
        }
    }

    public class SuperIgnited : AbstractIgnited {
        private const int DURATION = 5;

        public SuperIgnited() : base(DURATION, 2, 40) {
        }
    }

    public class TempIgnited : AbstractIgnited {
        private const int DURATION = 2;

        public TempIgnited() : base(DURATION) {
        }
    }

    public class PermanantIgnited : AbstractIgnited {

        public PermanantIgnited() : base() {
        }
    }

    public class Poison : PermanentBuff {

        public Poison() : base(Util.GetSprite("fox-head"), "Poisoned", "Loses health at the end of each turn.") {
        }

        protected override IList<SpellEffect> OnEndOfTurnHelper(Model.Characters.Stats owner) {
            return new SpellEffect[] {
                    new AddToModStat(owner, StatType.HEALTH, -1)
                };
        }
    }

    public class StrengthScalingPoison : Buff {

        public StrengthScalingPoison() : base(2, Util.GetSprite("fox-head"), "StrengthScalingPoison", "Loses health at the end of each turn.", true) {
        }

        protected override IList<SpellEffect> OnEndOfTurnHelper(Model.Characters.Stats owner) {
            return new SpellEffect[] {
                new AddToModStat(owner, StatType.HEALTH, BuffCaster.GetStatCount(Stats.Get.TOTAL, StatType.STRENGTH))
            };
        }
    }

    public class SuperCheck : Buff {
        private const int AGILITY_REDUCTION = 10;

        public SuperCheck()
            : base(20,
                  Util.GetSprite("magnifying-glass"),
                  "Checked",
                  string.Format("Resource bar visibility increased. {0} reduced by {1}%.", StatType.AGILITY, AGILITY_REDUCTION),
                  true) {
            AddMultiplicativeStatBonus(StatType.AGILITY, -AGILITY_REDUCTION);
        }

        protected override IList<SpellEffect> OnApplyHelper(Stats owner) { // TODO UNSTACKABLE
            return new SpellEffect[] { new AddToResourceVisibility(owner, 1) };
        }

        protected override IList<SpellEffect> OnTimeOutHelper(Stats owner) {
            return new SpellEffect[] { new AddToResourceVisibility(owner, -1) };
        }
    }

    public class BasicChecked : Buff {

        public BasicChecked() : base(10, Util.GetSprite("magnifying-glass"), "Checked", "Resource bar visibility increased.", true) {
        }

        protected override IList<SpellEffect> OnApplyHelper(Stats owner) { // TODO UNSTACKABLE
            return new SpellEffect[] { new AddToResourceVisibility(owner, 1) };
        }

        protected override IList<SpellEffect> OnTimeOutHelper(Stats owner) {
            return new SpellEffect[] { new AddToResourceVisibility(owner, -1) };
        }
    }

    public class BlackedOut : Buff {

        public BlackedOut() : base(10, Util.GetSprite("sight-disabled"), "Blackout", "Resource visibility decreased.", true) {
        }

        protected override IList<SpellEffect> OnApplyHelper(Stats owner) {
            return new SpellEffect[] { new AddToResourceVisibility(owner, -1) };
        }

        protected override IList<SpellEffect> OnTimeOutHelper(Stats owner) {
            return new SpellEffect[] { new AddToResourceVisibility(owner, 1) };
        }
    }

    public class Counter : Buff {
        private int DAMAGE_RATIO_FROM_ATTACK = 2;

        public Counter() : base(2, Util.GetSprite("round-shield"), "Counter", "Basic <color=yellow>Attack</color>s on this unit are reflected.", false) {
        }

        public override bool IsReact(SingleSpell spellToReactTo, Stats owner) {
            return spellToReactTo.SpellBook is Attack && spellToReactTo.Target.Stats == owner;
        }

        protected override void ReactHelper(SingleSpell spellToReactTo, Stats owner) {
            spellToReactTo.Result.Effects.Clear();
            spellToReactTo.Result.AddEffect(
                new AddToModStat(
                    spellToReactTo.Caster.Stats, StatType.HEALTH,
                    -spellToReactTo.Caster.Stats.GetStatCount(Stats.Get.MOD, StatType.STRENGTH) * DAMAGE_RATIO_FROM_ATTACK)
                );
        }
    }

    public class Defend : Buff {
        public const string DEFEND_NAME = "Guard";
        private const int DAMAGE_REDUCTION_PERCENT = 80;

        private const float DAMAGE_REDUCTION = (100 - DAMAGE_REDUCTION_PERCENT) / 100f;

        public Defend()
            : base(1,
                  Util.GetSprite("shield"),
                  DEFEND_NAME,
                  string.Format("Reduces damage taken from spells this turn by {0}%.", DAMAGE_REDUCTION_PERCENT),
                  false) {
        }

        public override bool IsReact(SingleSpell spellToReactTo, Stats owner) {
            return spellToReactTo.Target.Stats == owner && spellToReactTo.Result.IsDealDamage;
        }

        protected override void ReactHelper(SingleSpell spellToReactTo, Stats owner) {
            foreach (SpellEffect se in spellToReactTo.Result.Effects) {
                AddToModStat addToModStat = se as AddToModStat;
                if (addToModStat != null && addToModStat.AffectedStat == StatType.HEALTH && addToModStat.Value < 0) {
                    addToModStat.Value = (int)(addToModStat.Value * DAMAGE_REDUCTION);
                }
            }
        }
    }

    public class RegenerateHealth : StatRegen {

        public RegenerateHealth() : base(StatType.HEALTH, 1) {
        }
    }

    public class RegenerateMana : StatRegen {

        public RegenerateMana() : base(StatType.MANA, 2) {
        }
    }

    public class HighManaRegeneration : StatRegen {

        public HighManaRegeneration() : base(StatType.MANA, 5) {
        }
    }

    public class SkillRegeneration : StatRegen {

        public SkillRegeneration() : base(StatType.SKILL, 1) {
        }
    }

    public class StrengthSirenSong : SirenSong {

        public StrengthSirenSong() : base(StatType.STRENGTH, "Lyric of Lethargy") {
        }
    }

    public class AgilitySirenSong : SirenSong {

        public AgilitySirenSong() : base(StatType.AGILITY, "Shanty of Snails") {
        }
    }

    public class IntellectSirenSong : SirenSong {

        public IntellectSirenSong() : base(StatType.INTELLECT, "Hymn of Horror") {
        }
    }

    public class VitalitySirenSong : SirenSong {

        public VitalitySirenSong() : base(StatType.VITALITY, "Melody of Mortality") {
        }
    }

    public class DelayedDeath : DelayedEffectSong {
        private static readonly StatType AFFECTED_STAT = StatType.HEALTH;

        public DelayedDeath() : base(8, "skull-crack", AFFECTED_STAT, "Curse: Death") {
        }
    }

    public class DelayedHyperDeath : DelayedEffectSong {
        private static readonly StatType AFFECTED_STAT = StatType.VITALITY;

        public DelayedHyperDeath() : base(5, "skull-crossed-bones", AFFECTED_STAT, "Curse: Hyperdeath") {
        }
    }

    public class CalmedMind : StatChange {
        private const int INTELLECT_BOOST = 30;

        public CalmedMind() : base(3, StatType.INTELLECT, INTELLECT_BOOST) {
        }
    }

    public class StrengthBoost : Buff {
        public const int STRENGTH_INCREASE_AMOUNT = 50;

        public StrengthBoost()
            : base(Util.GetSprite("fist"),
                  "Strength boost",
                  "Increases strength.",
                  true) {
            AddMultiplicativeStatBonus(StatType.STRENGTH, STRENGTH_INCREASE_AMOUNT);
        }
    }

    public class RoughSkin : Thorns {

        public RoughSkin() : base(1, "Rough Skin") {
        }
    }

    public class RougherSkin : Thorns {

        public RougherSkin() : base(3, "Rougher Skin") {
        }
    }

    public class RoughestSkin : Thorns {

        public RoughestSkin() : base(10, "Roughest Skin") {
        }
    }
}

namespace Scripts.Game.Defined.Unserialized.Buffs {

    public class Insight : ManaRegen {
        public const int MANA_RECOVERED_PER_TURN = 5;

        public Insight() : base(MANA_RECOVERED_PER_TURN) {
        }
    }

    public class UnholyInsight : ManaRegen {
        public const int MANA_RECOVERED_PER_TURN = 10;

        public UnholyInsight() : base(MANA_RECOVERED_PER_TURN) {
        }
    }

    public abstract class Thorns : PermanentBuff {
        private readonly int damageToReturn;

        public Thorns(int damageToReturn, string name)
            : base(
                  Util.GetSprite("crown-of-thorns"),
                  name,
                  string.Format("Successful basic attacks on this unit cause the attacker to take {0} damage.",
                      damageToReturn
                      )
                  ) {
            this.damageToReturn = damageToReturn;
        }

        public override bool IsReact(SingleSpell incomingSpell, Stats ownerOfThisBuff) {
            return incomingSpell.Target.Stats == ownerOfThisBuff
                && incomingSpell.SpellBook is Attack
                && incomingSpell.Result.IsDealDamage;
        }

        protected override void ReactHelper(SingleSpell spellCast, Stats ownerOfThisBuff) {
            spellCast.Result.AddEffect(
                new AddToModStat(spellCast.Caster.Stats, StatType.HEALTH, -damageToReturn));
        }
    }

    public class Immunity : Buff {

        public Immunity() : base(5, Util.GetSprite("beams-aura"), "Immunity", "Immune to basic attack and spell damage.", true) {
        }

        public override bool IsReact(SingleSpell incomingSpell, Stats ownerOfThisBuff) {
            return incomingSpell.Target.Stats == ownerOfThisBuff && incomingSpell.Result.IsDealDamage;
        }

        protected override void ReactHelper(SingleSpell spellCast, Stats ownerOfThisBuff) {
            foreach (SpellEffect se in spellCast.Result.Effects) {
                AddToModStat addToModStat = se as AddToModStat;
                if (addToModStat != null && addToModStat.AffectedStat == StatType.HEALTH && addToModStat.Value < 0) {
                    addToModStat.Value = 0;
                }
            }
        }
    }

    public abstract class DelayedEffectSong : Buff {
        private readonly StatType affectedStat;

        public DelayedEffectSong(int duration, string spriteLoc, StatType affectedStat, string name)
            : base(duration,
                  Util.GetSprite(spriteLoc),
                  name,
                  string.Format("On time-out, affected unit loses ALL {0}.", affectedStat.ColoredName),
                  true) {
            this.affectedStat = affectedStat;
        }

        protected override IList<SpellEffect> OnDispellHelper(Stats ownerOfThisBuff) {
            return new SpellEffect[0];
        }

        protected override IList<SpellEffect> OnTimeOutHelper(Stats ownerOfThisBuff) {
            return new SpellEffect[] {
                new AddToModStat(ownerOfThisBuff, affectedStat, -ownerOfThisBuff.GetStatCount(Stats.Get.TOTAL, affectedStat))
            };
        }
    }

    public class SpiritLink : Buff {

        public SpiritLink() : base(Util.GetSprite("knot"), "Spirit Link", "Attacks on the non-clone unit will also cause this unit to take damage.", false) {
        }

        /// <summary>
        /// The caster of the buff (the non-clone) is hit by an offensive spell that deals damage.
        /// </summary>
        public override bool IsReact(SingleSpell spellToReactTo, Stats buffHolder) {
            return spellToReactTo.SpellBook.SpellType == SpellType.OFFENSE
                && spellToReactTo.Target.Stats == this.BuffCaster
                && spellToReactTo.Result.Type.IsSuccessfulType
                && spellToReactTo.Result.IsDealDamage;
        }

        protected override void ReactHelper(SingleSpell spellToReactTo, Stats buffHolder) {
            SpellEffect healthDamage = null;
            for (int i = 0; (i < spellToReactTo.Result.Effects.Count) && (healthDamage == null); i++) {
                SpellEffect se = spellToReactTo.Result.Effects[i];
                AddToModStat addToModStat = se as AddToModStat;
                if (addToModStat != null && addToModStat.AffectedStat == StatType.HEALTH && addToModStat.Value < 0) {
                    healthDamage = addToModStat;
                }
            }
            if (healthDamage != null) {
                buffHolder.AddToStat(StatType.HEALTH, Stats.Set.MOD, healthDamage.Value);
            }
        }
    }

    public class ReflectAttack : Buff {
        private const int REFLECT_DAMAGE_RATIO = 1;

        public ReflectAttack() : base(Util.GetSprite("round-shield"), "Reflect Attack", "Next <color=yellow>Attack</color> on this unit is reflected.", false) {
        }

        /// <summary>
        /// The owner of the buff (the clone) is hit by an offensive spell that deals damage.
        /// </summary>
        public override bool IsReact(SingleSpell spellToReactTo, Stats owner) {
            return spellToReactTo.SpellBook.SpellType == SpellType.OFFENSE
                && spellToReactTo.Target.Stats == owner
                && spellToReactTo.Result.Type.IsSuccessfulType
                && spellToReactTo.Result.IsDealDamage;
        }

        /// <summary>
        ///  Add a self damage effect, reflecting the same amount as the attack.
        /// </summary>
        protected override void ReactHelper(SingleSpell spellToReactTo, Stats owner) {
            int damageToReflect = 0;
            foreach (SpellEffect se in spellToReactTo.Result.Effects) {
                AddToModStat damageHealth = se as AddToModStat;
                if (damageHealth != null && damageHealth.AffectedStat == StatType.HEALTH) {
                    damageToReflect = damageHealth.Value * REFLECT_DAMAGE_RATIO;
                }
            }
            spellToReactTo.Result.AddEffect(
                new AddToModStat(
                    spellToReactTo.Caster.Stats, StatType.HEALTH, damageToReflect)
                );
        }
    }

    public class Interceptor : Buff {
        private const int INTERCEPTION_DAMAGE = 2;

        // TODO add sprite
        public Interceptor() : base(Util.GetSprite("suckered-tentacle"), "Interceptor", "Unit will intercept attacks on its summoner.", false) { }

        public override bool IsReact(SingleSpell incomingSpell, Stats statsOfTheCharacterTheBuffIsOn) {
            bool isDealDamageToBuffCaster = false;

            // Intercepting adds an add to mod stat, so the spell technically does damage even after we wipe it
            // isDealDamage returns true regardless of whether the caster or target is taking damage
            // But we want to stop adding damage after the first tentacle intercepts the attack
            // which wipes the spell's effects and replaces it with a single addToModStat health damage
            foreach (SpellEffect se in incomingSpell.Result.Effects) {
                AddToModStat addToModStat = se as AddToModStat;
                if (addToModStat != null
                    && addToModStat.AffectedStat == StatType.HEALTH
                    && addToModStat.Value < 0
                    && addToModStat.Target == BuffCaster) {
                    isDealDamageToBuffCaster = true;
                }
            }

            return incomingSpell.Target.Stats == BuffCaster && isDealDamageToBuffCaster;
        }

        protected override void ReactHelper(SingleSpell s, Stats owner) {
            s.Result.Effects.Clear();
            s.Result.AddEffect(new AddToModStat(s.Caster.Stats, StatType.HEALTH, -INTERCEPTION_DAMAGE));
        }
    }

    public abstract class StatRegen : PermanentBuff {
        private int amountPerTurn;
        private StatType type;

        public StatRegen(StatType type, int amountPerTurn)
            : base(type.Sprite, string.Format("Restore {0}", type.Name), String.Format("Regenerate {0} {1} each turn.", amountPerTurn, type.ColoredName)) {
            this.amountPerTurn = amountPerTurn;
            this.type = type;
        }

        protected override IList<SpellEffect> OnEndOfTurnHelper(Stats owner) {
            return new SpellEffect[] {
                new AddToModStat(owner, type, amountPerTurn)
            };
        }
    }

    public abstract class SirenSong : StatChange {
        private const int STAT_REDUCTION_PERCENT = 10;
        private const int DURATION = 3;

        public SirenSong(StatType type, string buffName)
            : base(DURATION, type, -STAT_REDUCTION_PERCENT, buffName, Util.GetSprite("g-clef")) { }
    }

    public abstract class StatChange : Buff {

        public StatChange(int duration, StatType type, int amount, string buffName, Sprite sprite)
            : base(
                  duration,
                  sprite,
                  buffName,
                  string.Format("{0} {1} by {2}%.", type.ColoredName, amount < 0 ? "decreased" : "increased", amount < 0 ? -amount : amount),
                  true) {
            Util.Assert(amount != 0, "Amount must be nonzero.");
            AddMultiplicativeStatBonus(type, amount);
        }

        public StatChange(int duration, StatType type, int amount)
            : this(
                      duration,
                      type,
                      amount,
                      string.Format("{0}{1}", type.Name, amount < 0 ? '-' : '+'),
                      type.Sprite) {
        }
    }

    public class Evasion : Buff {
        private const int AGILITY_INCREASE = 1337;

        public Evasion()
            : base(
                  10,
                  Util.GetSprite("power-lightning"),
                  "Evasion",
                  string.Format("{0} boosted by {1}%.",
                      StatType.AGILITY,
                      AGILITY_INCREASE),
                  true) {
            AddMultiplicativeStatBonus(StatType.STRENGTH, AGILITY_INCREASE);
        }
    }

    public abstract class AbstractIgnited : Buff {
        public const string NAME = "Ignited";
        private static readonly StatType REDUCED_STAT = StatType.STRENGTH;
        private const int STAT_REDUCTION_PERCENT = 20;
        private const int DEFUALT_DAMAGE_PER_TURN = 1;
        private int damagePerTurn = DEFUALT_DAMAGE_PER_TURN;

        public AbstractIgnited(int duration, int damagePerTurn, int statReductionPercent)
    : base(duration,
          Util.GetSprite("fire"),
          NAME,
          string.Format("{0} reduced by {2}%. Take {1} damage at end of turn.",
              REDUCED_STAT.ColoredName,
              damagePerTurn,
              statReductionPercent),
          true) {
            this.damagePerTurn = damagePerTurn;
            AddMultiplicativeStatBonus(REDUCED_STAT, -statReductionPercent);
        }

        public AbstractIgnited(int duration)
            : base(duration,
                  Util.GetSprite("fire"),
                  NAME,
                  string.Format("{0} reduced by 20%. Take {1} damage at end of turn.",
                      REDUCED_STAT.ColoredName,
                      DEFUALT_DAMAGE_PER_TURN),
                  true) {
            AddMultiplicativeStatBonus(REDUCED_STAT, -STAT_REDUCTION_PERCENT);
        }

        public AbstractIgnited()
            : base(Util.GetSprite("fire"),
                  "Ignited",
                  string.Format("{0} reduced by 20%. Take {1} damage at end of turn.",
                      REDUCED_STAT.ColoredName,
                      DEFUALT_DAMAGE_PER_TURN),
                  true) {
        }

        protected override IList<SpellEffect> OnEndOfTurnHelper(Stats owner) {
            return new SpellEffect[] {
                new AddToModStat(owner, StatType.HEALTH, -damagePerTurn)
            };
        }
    }

    public abstract class ManaRegen : Buff {
        private readonly int manaRecoveryPerTurn;

        public ManaRegen(int manaRecoveryPerTurn) : base(Util.GetSprite("water-drop"), "Insight", string.Format("Regenerating {0} {1} per turn.", manaRecoveryPerTurn, StatType.MANA.ColoredName), false) {
            this.manaRecoveryPerTurn = manaRecoveryPerTurn;
        }

        protected override IList<SpellEffect> OnEndOfTurnHelper(Stats owner) {
            return new SpellEffect[] {
                new AddToModStat(owner, StatType.MANA, manaRecoveryPerTurn)
            };
        }
    }

    public abstract class Countdown : Buff {
        private const int STAT_BOOST = 666;

        public Countdown(int turnsUntilEnrage)
            : base(
                  turnsUntilEnrage,
                  Util.GetSprite("dragon-head"),
                  "Countdown",
                  string.Format("On expiration, unit gains {0} increased {1} and {2}.", STAT_BOOST, StatType.STRENGTH, StatType.INTELLECT),
                  false) {
        }

        protected override IList<SpellEffect> OnTimeOutHelper(Stats ownerOfThisBuff) {
            return new SpellEffect[] {
                new AddToModStat(ownerOfThisBuff, StatType.STRENGTH, STAT_BOOST, false),
                new AddToModStat(ownerOfThisBuff, StatType.INTELLECT, STAT_BOOST, false)
            };
        }
    }

    public class StandardCountdown : Countdown {
        private const int STANDARD_DURATION = 20;

        public StandardCountdown() : base(STANDARD_DURATION) {
        }
    }

    public abstract class OnlyAffectedByCharacterWithStat : Buff {
        private readonly StatType statThatCanAffect;

        public OnlyAffectedByCharacterWithStat(StatType statThatCanAffect, string buffName)
             : base(Util.GetSprite("shield"),
                    buffName,
                    string.Format("Only casters with the {0} resource can affect this unit.", statThatCanAffect),
                    false) {
            Util.Assert(StatType.RESOURCES.Contains(statThatCanAffect), "Stat is not a resource!");
            this.statThatCanAffect = statThatCanAffect;
        }

        public override bool IsReact(SingleSpell incomingSpell, Stats ownerOfThisBuff) {
            return incomingSpell.Target.Stats == ownerOfThisBuff && !incomingSpell.Caster.Stats.HasStat(statThatCanAffect);
        }

        protected override void ReactHelper(SingleSpell spellCast, Stats ownerOfThisBuff) {
            spellCast.Result.Effects.Clear();
        }
    }

    public class OnlyAffectedByHero : OnlyAffectedByCharacterWithStat {

        public OnlyAffectedByHero() : base(StatType.MANA, "Magical Affinity") {
        }
    }

    public class OnlyAffectedByPartner : OnlyAffectedByCharacterWithStat {

        public OnlyAffectedByPartner() : base(StatType.SKILL, "Heroic Affinity") {
        }
    }
}