using Scripts.Game.Dungeons;
using Scripts.Model.Pages;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts.Game.Serialized;
using Scripts.Model.Characters;
using Scripts.Game.Areas;

namespace Scripts.Game.Stages {

    public class BattleStage : Stage {

        /// <summary>
        /// The encounters
        /// </summary>
        public readonly Func<Encounter[]> Encounters;

        /// <summary>
        /// Initializes a new instance of the <see cref="Stage"/> class.
        /// </summary>
        /// <param name="stageName">Name of the stage.</param>
        /// <param name="encounters">The encounters.</param>
        public BattleStage(string stageName, Func<Encounter[]> encounters) : base(stageName) {
            this.Encounters = encounters;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stage"/> class.
        /// </summary>
        public BattleStage() : this("Placeholder", () => new Encounter[] { new Encounter(Music.NORMAL) }) { }

        public override Page GetPage(int dungeonIndex, int areaTotalStageCount, Flags flags, IEnumerable<Character> party, Page camp, Page quests, AreaType type) {
            return new Dungeon(
                    party,
                    AreaList.AREA_MUSIC[type],
                    camp,
                    quests,
                    camp,
                    StageName,
                    string.Format("Selected stage {0} of {1}:\n{2}.\n\nEnter?",
                        dungeonIndex,
                        type.GetDescription(),
                        StageName
                        ),
                    Encounters,
                    () => {
                        flags.ShouldAdvanceTimeInCamp = true;
                        OnStageClear(areaTotalStageCount, type, flags, dungeonIndex);
                    }
                    ).Root;
        }
    }
}