using Scripts.Game.Defined.Serialized.Spells;
using Scripts.Model.Characters;
using System;
using System.Collections.Generic;

namespace Scripts.Game.Serialized.Brains {

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
                return "(It barks menacingly at you.)";
            }
            return string.Empty;
        }
    }
}