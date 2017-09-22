using Scripts.Model.Characters;
using UnityEngine;

namespace Scripts.Model.Spells {

    /// <summary>
    ///SpellEffects are the raw material that composes a spell.
    /// </summary>
    public abstract class SpellEffect {
        /// <summary>
        /// The value
        /// </summary>
        public readonly int Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpellEffect"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public SpellEffect(int value) {
            this.Value = value;
        }

        /// <summary>
        /// Causes the effect.
        /// </summary>
        public abstract void CauseEffect();
    }
}