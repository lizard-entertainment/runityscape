using Scripts.Game.Defined.Spells;
using Scripts.Model.Stats;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Scripts.Model.Spells {
    /// <summary>
    /// Describes the result of a Spell
    /// </summary>
    public class Result {
        /// <summary>
        /// The type of result.
        /// </summary>
        public ResultType Type;

        /// <summary>
        /// The SFX
        /// </summary>
        private readonly List<IEnumerator> sfx;
        /// <summary>
        /// The effects
        /// </summary>
        private readonly List<SpellEffect> effects;

        /// <summary>
        /// Initializes a new instance of the <see cref="Result"/> class.
        /// </summary>
        public Result() {
            this.effects = new List<SpellEffect>();
            this.sfx = new List<IEnumerator>();
        }

        /// <summary>
        /// Gets the SFX.
        /// </summary>
        /// <value>
        /// The SFX.
        /// </value>
        public IList<IEnumerator> SFX {
            get {
                return sfx;
            }
        }
        /// <summary>
        /// Gets the effects.
        /// </summary>
        /// <value>
        /// The effects.
        /// </value>
        public IList<SpellEffect> Effects {
            get {
                return effects;
            }
        }

        public bool IsDealDamage {
            get {
                foreach (SpellEffect effect in Effects) {
                    AddToModStat modStat = effect as AddToModStat;
                    if (modStat != null && modStat.AffectedStat == StatType.HEALTH && modStat.Value < 0) {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Adds the effect.
        /// </summary>
        /// <param name="se">The se.</param>
        public void AddEffect(SpellEffect se) {
            Effects.Add(se);
        }

        /// <summary>
        /// Adds the effects.
        /// </summary>
        /// <param name="effects">The effects.</param>
        public void AddEffects(IEnumerable<SpellEffect> effects) {
            this.effects.AddRange(effects);
        }

        /// <summary>
        /// Adds the SFX.
        /// </summary>
        /// <param name="sfx">The SFX.</param>
        public void AddSFX(IEnumerator sfx) {
            SFX.Add(sfx);
        }

        /// <summary>
        /// Adds the SFX.
        /// </summary>
        /// <param name="sfx">The SFX.</param>
        public void AddSFX(ICollection<IEnumerator> sfx) {
            this.sfx.AddRange(sfx);
        }
    }
}