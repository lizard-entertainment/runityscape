using Scripts.Game.Defined.Characters;
using Scripts.Game.Defined.Serialized.Buffs;
using Scripts.Game.Defined.Serialized.Spells;
using Scripts.Model.Characters;
using Scripts.Model.Spells;
using Scripts.Model.Stats;
using Scripts.Presenter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scripts.Game.Serialized.Brains {

    public class Attacker : PriorityBrain {
        public static readonly Attack ATTACK = new Attack();

        protected override IList<Func<IPlayable>> SetupPriorityPlays() {
            return new Func<IPlayable>[] {
                CastOnRandom(ATTACK)
            };
        }
    }

    public class Healer : PriorityBrain {
        public static readonly Attack ATTACK = new Attack();
        public static readonly EnemyHeal HEAL = new EnemyHeal();

        protected override IList<Func<IPlayable>> SetupPriorityPlays() {
            return new Func<IPlayable>[] {
                    CastOnTargetMeetingCondition(HEAL, c => c.Stats.GetMissingStatCount(StatType.HEALTH) > 0)
                };
        }
    }

    public class Wizard : PriorityBrain {
        public static readonly Ignite IGNITE = new Ignite();
        public static readonly Attack ATTACK = new Attack();

        protected override IList<Func<IPlayable>> SetupPriorityPlays() {
            return new Func<IPlayable>[] {
                    CastOnRandom(IGNITE),
                    CastOnRandom(ATTACK)
                };
        }
    }

    public class BigKnight : PriorityBrain {
        public static readonly SetupCounter COUNTER = new SetupCounter();
        public static readonly Attack ATTACK = new Attack();

        protected override IList<Func<IPlayable>> SetupPriorityPlays() {
            return new Func<IPlayable>[] {
                    CastOnRandom(COUNTER),
                    CastOnRandom(ATTACK)
                };
        }

        public override string StartOfRoundDialogue() {
            if (currentBattle.TurnCount == 0) {
                return Util.PickRandom("Stand and deliver!/You will go further!/Stop right there!");
            }
            return string.Empty;
        }
    }

    public class Illusionist : PriorityBrain {
        public static readonly Attack ATTACK = new Attack();
        public static readonly Blackout BLACKOUT = new Blackout();

        protected override IList<Func<IPlayable>> SetupPriorityPlays() {
            return new Func<IPlayable>[] {
                    CastOnTargetMeetingCondition(BLACKOUT, c => !c.Buffs.HasBuff<BlackedOut>()),
                    CastOnRandom(ATTACK)
                };
        }
    }

    public class Replicant : BasicBrain {
        private const int TURNS_BETWEEN_CLONES = 5;
        private const int SHED_DISGUISE_HEALTH = 20;
        private const int LOW_HEALTH = 10;
        public static readonly SpellBook WAIT = new Wait();
        public static readonly SpellBook ATTACK = new Attack();
        public static readonly SpellBook CLONE = new ReflectiveClone();
        public static readonly SpellBook TRANSFORM = new RevealTrueForm();
        private bool commentedOnDeadHealers;
        private bool hasTransformed;
        private bool commentedOnLowHealth;
        private bool commentedOnTransform;
        private int lastTurnCloned;

        private bool IsAnyHealersAlive {
            get {
                return currentBattle.GetAllies(brainOwner).Count(c => c.Brain is Healer) > 0;
            }
        }

        private bool ShouldCastClone {
            get {
                return CloneCount == 0 && (((currentBattle.TurnCount - lastTurnCloned) >= TURNS_BETWEEN_CLONES) || IsLowHealth) && !IsAnyHealersAlive;
            }
        }

        private bool IsLowHealth {
            get {
                return brainOwner.Stats.GetStatCount(Stats.Get.MOD, StatType.HEALTH) <= LOW_HEALTH;
            }
        }

        private int CloneCount {
            get {
                return currentBattle.GetAllies(brainOwner).Count(c => c.HasFlag(Model.Characters.Flag.IS_CLONE) && c.Stats.State == State.ALIVE);
            }
        }

        protected override IPlayable GetPlay() {
            // Phase 1: Attacks only while healers are up
            if (IsAnyHealersAlive) {
                return CastOnRandom(WAIT);
            } else { // Phase 2: Start using clone
                if (!hasTransformed) {
                    hasTransformed = true;
                    return CastOnRandom(TRANSFORM);
                } else if (ShouldCastClone) {
                    lastTurnCloned = currentBattle.TurnCount;
                    return CastOnRandom(CLONE);
                } else {
                    return CastOnRandom(ATTACK);
                }
            }
        }

        public override string ReactToSpell(Spell spell) {
            int cloneCount = CloneCount;
            if (cloneCount > 0 && spell.Book.SpellType == SpellType.OFFENSE && spell.Result.Type.IsSuccessfulType && brainOwner.Stats.State == State.ALIVE) {
                return Util.PickRandom("YOUR NEXT ATTEMPT WILL NOT BE AS SUCCESSFUL./FOOL! YOU DELAY THE INEVITABLE!/MADNESS WILL CONSUME YOU!/DROWN IN INSANITY!");
            }
            return string.Empty;
        }

        public override string StartOfRoundDialogue() {
            if (currentBattle.TurnCount == 0) {
                return Util.PickRandom("Stop! There will be no violence in this place!/Stop! There will be no killing here!");
            }
            if (!commentedOnDeadHealers && !IsAnyHealersAlive) {
                commentedOnDeadHealers = true;
                return Util.PickRandom("Haha, you killed them so quickly./Aren't you just so remorseless?/A bit merciless, wouldn't you say?/Without even a second thought.");
            }
            if (!commentedOnLowHealth && IsLowHealth) {
                commentedOnLowHealth = true;
                return Util.PickRandom("All things come to an end./Death awaits us all./The end is coming.");
            }
            return string.Empty;
        }
    }

    public class ReplicantClone : BasicBrain {
        public static readonly Attack ATTACK = new Attack();

        private int CloneCount {
            get {
                return currentBattle.GetAllies(brainOwner).Count(c => c.HasFlag(Model.Characters.Flag.IS_CLONE));
            }
        }

        protected override IPlayable GetPlay() {
            return this.CastOnRandom(ATTACK);
        }

        public override string ReactToSpell(Spell spell) {
            if (spell.Book.SpellType == SpellType.OFFENSE && spell.Result.Type.IsSuccessfulType && spell.Result.IsDealDamage) {
                return
                    Util.PickRandom(
                        "One step closer to death./Do you feel the end coming?/Madness does not die./You slay merely a simulacrum of myself./Merely a replication of my true self./Once again you've fallen for a clone./Maddening, is it not?");
            }
            return string.Empty;
        }
    }
}