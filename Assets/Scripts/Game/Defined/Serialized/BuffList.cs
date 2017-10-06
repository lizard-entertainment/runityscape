using Scripts.Game.Defined.Serialized.Spells;
using Scripts.Game.Defined.Spells;
using Scripts.Game.Defined.Unserialized.Buffs;
using Scripts.Model.Buffs;
using Scripts.Model.Characters;
using Scripts.Model.Spells;
using Scripts.Model.Stats;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Game.Defined.Serialized.Buffs {

    public class LifeSteal : Buff {
        private const float DEFAULT_LIFE_STEAL = 0.2f;
        private float lifeSteal;

        public LifeSteal()
            : base(Util.GetSprite("water-drop"),
                  "Life Steal",
                  string.Format(
                      "Basic <color=yellow>attacks</color> restore healt by {0}% of damage dealt", DEFAULT_LIFE_STEAL * 100), false) {
            this.lifeSteal = DEFAULT_LIFE_STEAL;
        }

        public void SetLifeSteal(float percent) {
            lifeSteal = percent;
        }

        public override bool IsReact(Spell spellToReactTo, Stats owner) {
            return spellToReactTo.Book is Attack && spellToReactTo.Caster.Stats == owner;
        }

        protected override void ReactHelper(Spell spellToReactTo, Stats owner) {
            SpellEffect healthDamage = null;
            for (int i = 0; (i < spellToReactTo.Result.Effects.Count) && (healthDamage == null); i++) {
                SpellEffect se = spellToReactTo.Result.Effects[i];
                AddToModStat addToModStat = se as AddToModStat;
                if (addToModStat != null && addToModStat.AffectedStat == StatType.HEALTH && addToModStat.Value < 0) {
                    healthDamage = addToModStat;
                }
            }
            if (healthDamage != null) {
                new AddToModStat(owner, StatType.HEALTH, (int)Math.Floor(healthDamage.Value * lifeSteal));
            }
        }
    }

    public class FishShook : Buff {
        private const float DEFAULT_FISH_MULT = 2;
        private float antiFishMult;

        public FishShook()
            : base(Util.GetSprite("fish"),
                  "Fish-Shook",
                  string.Format(
                      "Basic <color=yellow>attacks</color> against fishy targets deal {0} times damage. However, damage against non-fishy targets is reduced by the same factor.", DEFAULT_FISH_MULT), false) {
            antiFishMult = DEFAULT_FISH_MULT;
        }
        
        public override bool IsReact(Spell spellToReactTo, Stats owner) {
            return spellToReactTo.Book is Attack && spellToReactTo.Caster.Stats == owner;
        }

        protected override void ReactHelper(Spell spellToReactTo, Stats owner) {
            float localDmgMult = antiFishMult;
            if (spellToReactTo.Target.Look.Breed != Characters.Breed.FISH) {
                localDmgMult = 1 / localDmgMult;
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

    public class DamageResist : Buff {
        private const float DEFAULT_DAMAGE_MULT = 0.7f;
        private float damageMult;

        public DamageResist()
            : base(Util.GetSprite("round-shield"),
                  "Damage Resist",
                  String.Format("Reduces incident damage from basic attacks by {0}%.",
                      (1 - DEFAULT_DAMAGE_MULT) * 100), false) {
            damageMult = DEFAULT_DAMAGE_MULT;
        }

        protected override void ReactHelper(Spell spellToReactTo, Stats owner) {
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
                healthDamage.Value = (int)Math.Floor(healthDamage.Value * damageMult);
            }
        }

        public override bool IsReact(Spell spellToReactTo, Stats owner) {
            return spellToReactTo.Book is Attack && spellToReactTo.Target.Stats == owner;
        }
    }

    public class Ignited : Buff {
        private const int DAMAGE_PER_TURN = 1;
        private const int DURATION = 5;

        public Ignited()
            : base(DURATION,
                  Util.GetSprite("fire"),
                  "Ignited",
                  string.Format("Take {0} damage at end of turn.",
                      DAMAGE_PER_TURN), true) {
        }

        protected override IList<SpellEffect> OnEndOfTurnHelper(Stats owner) {
            return new SpellEffect[] {
                new AddToModStat(owner, StatType.HEALTH, -DAMAGE_PER_TURN)
            };
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

    public class Insight : Buff {
        private const int MANA_RECOVERED_PER_TURN = 5;

        public Insight() : base(Util.GetSprite("water-drop"), "Insight", string.Format("Regenerating {0}.", StatType.MANA), false) {
        }

        protected override IList<SpellEffect> OnEndOfTurnHelper(Stats owner) {
            return new SpellEffect[] {
                new AddToModStat(owner, StatType.MANA, MANA_RECOVERED_PER_TURN)
            };
        }
    }

    public class Poison : Buff {

        public Poison() : base(2, Util.GetSprite("fox-head"), "Poisoned", "Loses health at the end of each turn.", true) {
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

    public class Checked : Buff {

        public Checked() : base(5, Util.GetSprite("magnifying-glass"), "Checked", "Resource visibility increased.", true) {
        }

        protected override IList<SpellEffect> OnApplyHelper(Stats owner) { // TODO UNSTACKABLE
            return new SpellEffect[] { new AddToResourceVisibility(owner, 1) };
        }

        protected override IList<SpellEffect> OnTimeOutHelper(Stats owner) {
            return new SpellEffect[] { new AddToResourceVisibility(owner, -1) };
        }
    }

    public class BlackedOut : Buff {

        public BlackedOut() : base(3, Util.GetSprite("sight-disabled"), "Blackout", "Resource visibility decreased.", true) {
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

        public override bool IsReact(Spell spellToReactTo, Stats owner) {
            return spellToReactTo.Book is Attack && spellToReactTo.Target.Stats == owner;
        }

        protected override void ReactHelper(Spell spellToReactTo, Stats owner) {
            spellToReactTo.Result.Effects.Clear();
            spellToReactTo.Result.AddEffect(
                new AddToModStat(
                    spellToReactTo.Caster.Stats, StatType.HEALTH,
                    -spellToReactTo.Caster.Stats.GetStatCount(Stats.Get.MOD, StatType.STRENGTH) * DAMAGE_RATIO_FROM_ATTACK)
                );
        }
    }

    public class Defend : Buff {
        private const int DAMAGE_REDUCTION_PERCENT = 50;
        private const float DAMAGE_REDUCTION = DAMAGE_REDUCTION_PERCENT / 100f;

        public Defend()
            : base(1,
                  Util.GetSprite("round-shield"),
                  "Defend",
                  string.Format("Reduces damage taken this turn by {0}%", DAMAGE_REDUCTION_PERCENT),
                  false) {
        }

        public override bool IsReact(Spell spellToReactTo, Stats owner) {
            return spellToReactTo.Target.Stats == owner && spellToReactTo.Result.IsDealDamage;
        }

        protected override void ReactHelper(Spell spellToReactTo, Stats owner) {
            foreach (SpellEffect se in spellToReactTo.Result.Effects) {
                AddToModStat addToModStat = se as AddToModStat;
                if (addToModStat != null && addToModStat.AffectedStat == StatType.HEALTH && addToModStat.Value < 0) {
                    addToModStat.Value = (int)(addToModStat.Value * DAMAGE_REDUCTION);
                }
            }
        }
    }

    public class SpiritLink : Buff {

        public SpiritLink() : base(Util.GetSprite("knot"), "Spirit Link", "Attacks on the non-clone unit will also cause this unit to take damage.", false) {
        }

        /// <summary>
        /// The caster of the buff (the non-clone) is hit by an offensive spell that deals damage.
        /// </summary>
        public override bool IsReact(Spell spellToReactTo, Stats buffHolder) {
            return spellToReactTo.Book.SpellType == SpellType.OFFENSE
                && spellToReactTo.Target.Stats == this.BuffCaster
                && spellToReactTo.Result.Type.IsSuccessfulType
                && spellToReactTo.Result.IsDealDamage;
        }

        protected override void ReactHelper(Spell spellToReactTo, Stats buffHolder) {
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
        public override bool IsReact(Spell spellToReactTo, Stats owner) {
            return spellToReactTo.Book.SpellType == SpellType.OFFENSE
                && spellToReactTo.Target.Stats == owner
                && spellToReactTo.Result.Type.IsSuccessfulType
                && spellToReactTo.Result.IsDealDamage;
        }

        /// <summary>
        ///  Add a self damage effect, reflecting the same amount as the attack.
        /// </summary>
        protected override void ReactHelper(Spell spellToReactTo, Stats owner) {
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

    public class RegenerateHealth : StatRegen {

        public RegenerateHealth() : base(StatType.HEALTH, 1) {
        }
    }

    public class RegenerateMana : StatRegen {

        public RegenerateMana() : base(StatType.MANA, 2) {
        }
    }
}

namespace Scripts.Game.Defined.Unserialized.Buffs {

    public abstract class StatRegen : Buff {
        private int amountPerTurn;
        private StatType type;

        public StatRegen(StatType type, int amountPerTurn)
            : base(type.Sprite, string.Format("Restore {0}", type.Name), String.Format("Regenerate {0} {1} each turn.", amountPerTurn, type.ColoredName), false) {
            this.amountPerTurn = amountPerTurn;
            this.type = type;
        }

        protected override IList<SpellEffect> OnEndOfTurnHelper(Stats owner) {
            return new SpellEffect[] {
                new AddToModStat(owner, type, amountPerTurn)
            };
        }
    }

    public class StatChange : Buff {

        public StatChange(int duration, StatType type, int amount)
            : base(
                  duration,
                  type.Sprite,
                  string.Format("{0}{1}", type.Name, amount < 0 ? '-' : '+'),
                  string.Format("{0} {1} by {2}%.", type.ColoredName, amount < 0 ? "decreased" : "increased", amount),
                  true) {
            Util.Assert(amount != 0, "Amount must be nonnegative.");
            AddMultiplicativeStatBonus(type, amount);
        }
    }
}