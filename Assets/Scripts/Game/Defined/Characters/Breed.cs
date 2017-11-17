using System.ComponentModel;

namespace Scripts.Game.Defined.Characters {

    /// <summary>
    /// Represents the enemy's "race" seen on icon hover-over.
    /// </summary>
    public enum Breed {

        [Description("???")]
        UNKNOWN = 0,

        [Description("Programmer")]
        PROGRAMMER,

        [Description("Human")]
        HUMAN,

        [Description("Ghost")]
        SPIRIT,

        [Description("Beast")]
        BEAST,

        [Description("Fish")]
        FISH,

        [Description("Shark Bait")]
        SHARK_BAIT,

        [Description("Dummy")]
        DUMMY,

        // These are here for the credits
        [Description("Tester")]
        TESTER,

        [Description("Persister")]
        CREATOR,

        [Description("Commenter")]
        COMMENTER
    }
}