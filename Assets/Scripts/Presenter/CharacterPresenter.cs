using Scripts.Game.Defined.SFXs;
using Scripts.Model.Characters;
using Scripts.Model.Spells;
using Scripts.Model.Stats;
using Scripts.View.Portraits;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts.Presenter {

    /// <summary>
    /// Updates the portraitview from the character.
    /// </summary>
    public class CharacterPresenter {
        private Character character;
        private PortraitView portrait;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacterPresenter"/> class.
        /// </summary>
        /// <param name="character">The character associated with the portrait.</param>
        /// <param name="portraitView">The portrait associated with the character.</param>
        public CharacterPresenter(Character character, PortraitView portraitView) {
            this.character = character;
            this.portrait = portraitView;

            while (this.character.Effects.Count > 0) {
                ParentToEffects(character.Effects.Dequeue());
            }
        }

        /// <summary>
        /// Ticks this instance.
        /// </summary>
        public void Tick() {
            SetupFuncs();
        }

        private RectTransform IconRect {
            get {
                if (portrait == null) {
                    return null;
                }
                return portrait.Image.rectTransform;
            }
        }

        private void ParentToEffects(GameObject go) {
            if (portrait != null) {
                Util.Parent(go, portrait.EffectsHolder);
            }
        }

        /// <summary>
        /// Setups functions in character.
        /// </summary>
        private void SetupFuncs() {
            character.GetIconRectFunc = () => IconRect;
            character.GetIconOriginalPosFunc = () => portrait.OriginalIconPos;
            character.ParentToEffectsFunc = (go) => ParentToEffects(go);

            character.Stats.AddSplat = (sd => AddHitsplat(sd));
            character.Buffs.AddSplat = (sd => AddHitsplat(sd));
            character.Equipment.AddSplat = (sd => AddHitsplat(sd));
        }

        /// <summary>
        /// Function that adds a hitsplat to the portrait
        /// </summary>
        /// <param name="details">The details needed to customize the splat.</param>
        private void AddHitsplat(SplatDetails details) {
            if (portrait != null && portrait.isActiveAndEnabled) {
                portrait.StartCoroutine(SFX.DoHitSplat(character, details));
            }
        }
    }
}