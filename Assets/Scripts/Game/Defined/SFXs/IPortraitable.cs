using UnityEngine;

namespace Scripts.Game.Defined.SFXs {

    public interface IPortraitable {

        RectTransform RectTransform {
            get;
        }

        RectTransform OriginalPos {
            get;
        }

        void ParentToEffects(GameObject go);
    }
}