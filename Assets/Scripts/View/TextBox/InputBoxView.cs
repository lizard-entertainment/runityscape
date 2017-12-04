using Scripts.View.ObjectPool;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scripts.View.TextBoxes {

    /// <summary>
    /// Represents a box where the user can input their own text.
    /// Used for naming the user's character, and possibly other
    /// functions in the future.
    /// </summary>
    public class InputBoxView : PooledBehaviour {

        [SerializeField]
        private InputField inputField;

        /// <summary>
        /// Gets the input.
        /// </summary>
        /// <value>
        /// The input.
        /// </value>
        public string Input { get { return inputField.text; } }

        /// <summary>
        /// Reset the state of this MonoBehavior.
        /// </summary>
        public override void Reset() {
            inputField.text = "";
        }

        public void SetFocus() {
            //Force focus on InputField
            EventSystem.current.SetSelectedGameObject(inputField.gameObject, null);
            inputField.OnPointerClick(new PointerEventData(EventSystem.current));
        }

        private void Update() {
        }
    }
}