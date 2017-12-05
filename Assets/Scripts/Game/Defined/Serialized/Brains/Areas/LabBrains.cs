using System;
using System.Collections.Generic;
using Scripts.Model.Characters;
using Scripts.Model.Spells;
using Scripts.Game.Defined.Serialized.Spells;
using Scripts.Game.Defined.Unserialized.Spells;
using System.Linq;
using Scripts.Model.Stats;
using Scripts.Game.Defined.Serialized.Buffs;

namespace Scripts.Game.Serialized.Brains {
    // Crypt enemies

    public class LabKnight : PriorityBrain {
        private static readonly SpellBook ATTACK = new Attack();
        private static readonly SpellBook CRUSHING_BLOW = new CrushingBlow();
        private static readonly SpellBook DEFEND = new SetupDefend();

        public override string StartOfRoundDialogue() {
            if (CRUSHING_BLOW.IsMeetPreTargetRequirements(brainOwner)) {
                return Util.PickRandom("Now it's over!/Taste vengeance!/Power!/Eliminate!");
            }
            return string.Empty;
        }

        protected override IList<Spell> GetPriorityPlays() {
            return new Spell[] {
                this.CastOnRandom(DEFEND, () => currentBattle.TurnCount == 0),
                this.CastOnRandom(CRUSHING_BLOW),
                this.CastOnLeastTarget(ATTACK, SortByLowestHealth())
            };
        }
    }

    public class LabBigKnight : PriorityBrain {
        private static readonly SpellBook ATTACK = new Attack();
        private static readonly SpellBook REVIVE = new UnholyRevival();

        private static readonly SpellBook[] SKILL_SPELLS = new SpellBook[] {
            new CrushingBlow(),
            new SetupCounter()
        };

        private SpellBook chosenSkillSpell;

        private Character DeadPartner {
            get {
                return allies.Where(c => c.Stats.State == State.DEAD).FirstOrDefault();
            }
        }

        public override string StartOfRoundDialogue() {
            if (currentBattle.TurnCount == 0) {
                return Util.PickRandom("You will go no further!/Stop right there!");
            }
            if (DeadPartner != null
                && REVIVE.IsCastable(currentBattle, brainOwner, new Character[] { DeadPartner })
                && !currentBattle.IsChargingSpell(brainOwner)) {
                return string.Format("{0}! It is not yet your time!", DeadPartner.Look.Name);
            }
            return string.Empty;
        }

        protected override IList<Spell> GetPriorityPlays() {
            if (chosenSkillSpell == null || brainOwner.Stats.GetStatCount(Stats.Get.MOD, StatType.SKILL) == 0) {
                chosenSkillSpell = Util.ChooseRandom(SKILL_SPELLS);
            }
            return new Spell[] {
                this.CastOnRandom(REVIVE),
                this.CastOnRandom(chosenSkillSpell),
                this.CastOnLeastTarget(ATTACK, SortByLowestHealth())
            };
        }
    }

    public class Warlock : PriorityBrain {
        private static readonly SpellBook ATTACK = new Attack();
        private static readonly SpellBook INFERNO = new Inferno();

        protected override IList<Spell> GetPriorityPlays() {
            return new Spell[] {
                CastOnLeastTarget(INFERNO, c => -c.Buffs.GetBuffCount<PermanantIgnited>()),
                CastOnRandom(ATTACK)
            };
        }
    }

    public class Cleric : PriorityBrain {
        private const float HEALTH_PERCENTAGE_TO_START_DEFENDING = .50f;

        private static readonly SpellBook HEAL = new PlayerHeal();
        private static readonly SpellBook ATTACK = new Attack();
        private static readonly SpellBook DEFEND = new SetupDefend();

        private bool IsAnyAllyMissingHealth {
            get {
                return allies.Any(c => c.Stats.GetMissingStatCount(StatType.HEALTH) > 0);
            }
        }

        private float HealthPercentage {
            get {
                return this.brainOwner.Stats.GetStatPercent(StatType.HEALTH);
            }
        }

        protected override IList<Spell> GetPriorityPlays() {
            return new Spell[] {
                CastOnRandom(DEFEND, () =>
                    HealthPercentage > HEALTH_PERCENTAGE_TO_START_DEFENDING
                    && !IsAnyAllyMissingHealth),
                CastOnLeastTarget(HEAL, c => -c.Stats.GetMissingStatCount(StatType.HEALTH)), // This will cast even if no one needs healing
                CastOnRandom(ATTACK)
            };
        }
    }

    // Ocean enemies

    // Lab-unique enemies
    public class Partner : PriorityBrain {

        // TODO put these into an array for quicker adding to LabNPCs?
        private static readonly SpellBook QUICK_ATTACK = new QuickAttack();

        private static readonly SpellBook MEDITATE = new SelfHeal();
        private static readonly SpellBook INSPIRE = new Inspire();
        private static readonly SpellBook DEFEND = new SetupDefend();
        private static readonly SpellBook CRUSH = new CrushingBlow();
        private static readonly SpellBook MULTI_STRIKE = new Multistrike();
        private static readonly SpellBook ATTACK = new Attack();

        protected override IList<Spell> GetPriorityPlays() {
            return new Spell[] {
                CastQuickAttackToKill(),
                CastOnRandom(MEDITATE, () => brainOwner.Stats.GetStatPercent(StatType.HEALTH) < .25f),
                CastOnTargetMeetingCondition(INSPIRE, c => c.Stats.GetStatCount(Stats.Get.MOD, StatType.MANA) < Revive.MANA_COST),
                CastOnRandom(DEFEND, () => currentBattle.IsFoeWillUnleashSpellNextTurnOnTarget<CrushingBlow>(brainOwner)),
                CastOnTargetMeetingCondition(CRUSH, c => c.Stats.GetStatPercent(StatType.HEALTH) < .50f),
                CastOnRandom(MULTI_STRIKE),
                CastOnRandom(ATTACK)
            };
        }

        private Spell CastQuickAttackToKill() {
            return CastOnTarget(
                QUICK_ATTACK,
                foes => {
                    foreach (Character foe in foes) {
                        if (foe.Stats.GetStatCount(Stats.Get.MOD, StatType.HEALTH)
                        <= brainOwner.Stats.GetStatCount(Stats.Get.TOTAL, StatType.INTELLECT) * QuickAttack.INTELLECT_TO_DAMAGE) {
                            return foe;
                        }
                    }
                    return null;
                });
        }
    }

    public class Hero : PriorityBrain {

        // TODO put these into an array for quicker adding to LabNPCs?
        private static readonly SpellBook REVIVE = new Revive();

        private static readonly SpellBook HEAL = new PlayerHeal();
        private static readonly SpellBook DEFEND = new SetupDefend();
        private static readonly SpellBook MISSILE = new MagicMissile();
        private static readonly SpellBook ATTACK = new Attack();

        protected override IList<Spell> GetPriorityPlays() {
            return new Spell[] {
                CastOnTargetMeetingCondition(REVIVE, c => c.Stats.State == State.DEAD),
                CastOnTargetMeetingCondition(HEAL, c => c.Stats.GetStatPercent(StatType.HEALTH) < .50f),
                CastOnRandom(DEFEND, () => currentBattle.IsFoeWillUnleashSpellNextTurnOnTarget<CrushingBlow>(brainOwner)),
                CastOnRandom(MISSILE),
                CastOnRandom(ATTACK)
            };
        }
    }
}