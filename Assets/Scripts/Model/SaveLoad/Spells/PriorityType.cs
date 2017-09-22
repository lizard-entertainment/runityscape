using System.ComponentModel;

namespace Scripts.Model.Spells {

    /// <summary>
    /// List of priorities for spells.
    /// </summary>
    public enum PriorityType {
        [Description("<color=cyan>Highest</color>")]
        HIGHEST = 3,
        [Description("<color=cyan>Higher</color>")]
        HIGHER = 2,
        [Description("<color=cyan>High</color>")]
        HIGH = 1,
        [Description("Normal")]
        NORMAL = 0,
        [Description("<color=magenta>Low</color>")]
        LOW = -1,
        [Description("<color=magenta>Lower</color>")]
        LOWER = -2,
        [Description("<color=magenta>Lowest</color>")]
        LOWEST = -3
    }
}