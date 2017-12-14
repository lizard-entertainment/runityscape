using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.View.Other {

    public class DisableElasticIfNotNeeded : MonoBehaviour {

        [SerializeField]
        private ScrollRect rect;

        [SerializeField]
        private Scrollbar bar;

        private void Update() {
            if (bar.IsActive()) {
                rect.movementType = ScrollRect.MovementType.Elastic;
            } else {
                rect.movementType = ScrollRect.MovementType.Clamped;
            }
        }
    }
}