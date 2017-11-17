using Scripts.Game.Areas;
using Scripts.Game.Dungeons;
using Scripts.Game.Pages;
using Scripts.Model.SaveLoad;
using Scripts.Model.SaveLoad.SaveObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Scripts.Game.Serialized {

    public enum TimeOfDay {

        [Description("<color=white>Dawn</color>")]
        DAWN,

        [Description("<color=yellow>Morning</color>")]
        MORNING,

        [Description("<color=orange>Midday</color>")]
        MIDDAY,

        [Description("<color=red>Afternoon</color>")]
        AFTERNOON,

        [Description("<color=magenta>Dusk</color>")]
        DUSK,

        [Description("<color=blue>Night</color>")]
        NIGHT,
    }

    /// <summary>
    /// Game state flags.
    /// </summary>
    /// <seealso cref="Scripts.Model.SaveLoad.ISaveable{Scripts.Model.SaveLoad.SaveObjects.FlagsSave}" />
    [Serializable]
    public class Flags : ISaveable<FlagsSave> {
        public const int NO_STAGES_CLEARED = -1;

        public TimeOfDay Time;
        public int DayCount;
        public AreaType CurrentArea;
        public AreaType LastClearedArea;
        public int LastClearedStage;
        public bool ShouldAdvanceTimeInCamp;

        /// <summary>
        /// Initializes a new instance of the <see cref="Flags"/> class.
        /// </summary>
        public Flags() {
            this.LastClearedArea = AreaType.NONE;
            this.CurrentArea = AreaType.RUINS;
            this.LastClearedStage = NO_STAGES_CLEARED;
        }

        /// <summary>
        /// Gets the save object.
        /// </summary>
        /// <returns></returns>
        public FlagsSave GetSaveObject() {
            return new FlagsSave(this);
        }

        /// <summary>
        /// Determines true if the particular stage index of area is cleared.
        /// </summary>
        /// <param name="stageIndex">Index of the stage.</param>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if the particular stage index of area is cleared.; otherwise, <c>false</c>.
        /// </returns>
        public bool IsStageCleared(int stageIndex, AreaType type) {
            return this.LastClearedArea >= type || this.LastClearedStage >= stageIndex || Util.IS_DEBUG;
        }

        /// <summary>
        /// Determines whether the area is unlocked.
        /// </summary>
        /// <param name="type">The type of area to check for unlockage.</param>
        /// <returns>
        ///   <c>true</c> if the area is unlocked with this flag.; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAreaUnlocked(AreaType type) {
            return ((int)this.LastClearedArea + 1) >= (int)type || Util.IS_DEBUG;
        }

        /// <summary>
        /// Initializes from save object.
        /// </summary>
        /// <param name="saveObject">The save object.</param>
        public void InitFromSaveObject(FlagsSave saveObject) {
        }
    }
}