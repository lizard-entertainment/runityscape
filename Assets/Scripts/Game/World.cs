using System;
using Scripts.Model.Characters;
using Scripts.Model.SaveLoad;
using Scripts.Model.SaveLoad.SaveObjects;
using Scripts.Game.Areas;

namespace Scripts.Game.Serialized {

    /// <summary>
    /// Used for saving the world (entire game state).
    /// </summary>
    /// <seealso cref="Scripts.Model.SaveLoad.ISaveable{Scripts.Model.SaveLoad.SaveObjects.WorldSave}" />
    public class World : ISaveable<WorldSave> {

        /// <summary>
        /// The party
        /// </summary>
        public Party Party;

        /// <summary>
        /// The flags
        /// </summary>
        public Flags Flags;

        public string FileName {
            get {
                AreaType[] areaTypes = Util.EnumAsArray<AreaType>();
                bool isCompletedGame = Flags.LastClearedArea == areaTypes[areaTypes.Length - 1];

                string name = string.Empty;
                if (isCompletedGame) {
                    name = string.Format("{0}-{1}", "Cleared", Flags.DayCount);
                } else {
                    // NOT to be confused with the current area, which is the area user is current at
                    // User could be at the Lab, but if they saved in the Ruins region, their current area is the Ruins.
                    AreaType currentUnclearedArea = areaTypes[(int)(Flags.LastClearedArea) + 1];

                    name = string.Format("{0}-{1}", currentUnclearedArea.GetDescription(), Flags.LastClearedStage + 1);
                }

                return name;
            }
        }

        /// <summary>
        /// Gets the save object. A save object contains the neccessary
        /// information to initialize a clean class to its saved state.
        /// A save object is also serializable.
        /// </summary>
        /// <returns></returns>
        public WorldSave GetSaveObject() {
            return new WorldSave(
                Party.GetSaveObject(),
                Flags.GetSaveObject());
        }

        /// <summary>
        /// Initializes from save object.
        /// </summary>
        /// <param name="saveObject">The save object.</param>
        public void InitFromSaveObject(WorldSave saveObject) {
            Party restoredParty = new Party();
            restoredParty.InitFromSaveObject(saveObject.Party);
            this.Party = restoredParty;
            this.Flags = saveObject.Flags.Flags;
            Flags.InitFromSaveObject(saveObject.Flags);
        }
    }
}