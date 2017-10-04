using Scripts.Game.Defined.Serialized.Spells;
using Scripts.Model.Characters;
using Scripts.Model.Stats;
using System;
using System.Collections.Generic;

namespace Scripts.Game.Serialized.Brains {

    public class Swarm : PriorityBrain {
        public static readonly Attack ATTACK = new Attack();
        public static readonly EnemyHeal HEAL = new EnemyHeal(3);
        public int initialEnemiesPresent;
        public bool hasSaidDamagedMessage = false;

        protected override IList<Func<IPlayable>> SetupPriorityPlays() {
            return new Func<IPlayable>[] {
                    CastOnTargetMeetingCondition(HEAL, c => c.Stats.GetMissingStatCount(StatType.HEALTH) > 0),
                    CastOnRandom(ATTACK)
                };
        }

        public override string StartOfRoundDialogue() {
            if (currentBattle.TurnCount == 0) {
                initialEnemiesPresent = foes.Count;
                return "We have strength in numbers!/We are many, you are few./You cannnot possibly defeat us together.";
            }
            if(foes.Count < initialEnemiesPresent && !hasSaidDamagedMessage) {
                hasSaidDamagedMessage = true;
                return "Our defenses are cracking!/Our perfect unity is ruined./We will never be whole again.";
            }
            return string.Empty;
        }
    }

    public class BlackShuck : PriorityBrain {
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
                return "(It barks menacingly at you.)/(It growls threateningly.)";
            }
            return string.Empty;
        }
    }
}