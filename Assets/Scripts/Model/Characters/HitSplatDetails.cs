using Scripts.Model.Stats;
using UnityEngine;

namespace Scripts.Model.Characters {

    /// <summary>
    /// Details needed to add a hitsplat
    /// </summary>
    public struct SplatDetails {
        public readonly Sprite Sprite;
        public readonly Color Color;
        public readonly string Text;

        /// <summary>
        /// Initializes a new instance of the <see cref="SplatDetails"/> struct.
        /// </summary>
        /// <param name="color">The color of the hitsplat.</param>
        /// <param name="text">The text of the splat.</param>
        /// <param name="sprite">The sprite of the plat.</param>
        public SplatDetails(Color color, string text, Sprite sprite = null) {
            this.Color = color;
            this.Text = text;
            this.Sprite = sprite;
        }

        public SplatDetails(StatType type, int amount) {
            Color color = Color.grey;
            if (amount < 0) {
                color = Color.red;
            } else if (amount > 0) {
                color = Color.green;
            } else {
                color = Color.grey;
            }
            this.Color = color;
            this.Sprite = type.Sprite;
            this.Text = Util.Sign(amount);
        }
    }
}