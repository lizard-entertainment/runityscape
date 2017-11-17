using Scripts.Model.Pages;
using System;
using Scripts.Game.Serialized;
using Scripts.Model.Characters;
using System.Collections.Generic;
using Scripts.Game.Dungeons;
using Scripts.Game.Areas;

namespace Scripts.Game.Stages {

    /// <summary>
    /// Stages are groups of enemy encounters.
    /// </summary>
    /// <seealso cref="Scripts.Game.Dungeons.IStageable" />
    public abstract class Stage {

        /// <summary>
        /// The stage name
        /// </summary>
        public readonly string StageName;

        /// <summary>
        /// Initializes a new instance of the <see cref="Stage"/> class.
        /// </summary>
        /// <param name="stageName">Name of the stage.</param>
        /// <param name="encounters">The encounters.</param>
        public Stage(string stageName) {
            this.StageName = stageName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stage"/> class.
        /// </summary>
        public Stage() {
            this.StageName = "Placeholder";
        }

        /// <summary>
        /// Gets the Page from this instance.
        /// </summary>
        /// <param name="dungeonIndex">Index of the dungeon in the area.</param>
        /// <param name="areaDungeonCount">The number of dungeons in the area.</param>
        /// <param name="flags">Game flags.</param>
        /// <param name="party">Traversing party.</param>
        /// <param name="camp">Camp reference.</param>
        /// <param name="quests">Quest page reference.</param>
        /// <param name="area">The area.</param>
        /// <returns></returns>
        public abstract Page GetPage(int dungeonIndex, int areaDungeonCount, Flags flags, IEnumerable<Character> party, Page camp, Page quests, AreaType type);

        /// <summary>
        /// Called when the stage is cleared.
        /// </summary>
        /// <param name="areaTotalStageCount">The area total stage count.</param>
        /// <param name="type">The type of area this stage is in.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="stageIndex">Index of this stage.</param>
        protected void OnStageClear(int areaTotalStageCount, AreaType type, Flags flags, int stageIndex) {
            if (!flags.IsStageCleared(stageIndex, type)) {
                // Not the last stage in an area
                if (stageIndex < (areaTotalStageCount - 1)) {
                    flags.LastClearedStage = stageIndex;

                    // is the last stage in an area, but ISN'T the final one ever
                } else if (!IsLastStageInTheGame(stageIndex, areaTotalStageCount, type)) {
                    AreaType[] types = Util.EnumAsArray<AreaType>();
                    AreaType nextArea = types[((int)type + 1) % types.Length];
                    flags.CurrentArea = nextArea;
                    flags.LastClearedArea = type;
                    flags.LastClearedStage = Flags.NO_STAGES_CLEARED;
                }
            }
        }

        private bool IsLastStageInTheGame(int stageIndex, int areaTotalStageCount, AreaType area) {
            AreaType[] areas = Util.EnumAsArray<AreaType>();
            return stageIndex == (areaTotalStageCount - 1) && (area == areas[areas.Length - 1]);
        }
    }
}