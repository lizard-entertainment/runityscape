using Scripts.Model.Interfaces;
using Scripts.Presenter;
using Scripts.View.ObjectPool;
using Scripts.View.Tooltip;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.View.ActionGrid {

    /// <summary>
    /// This class represents a button that can be pressed with
    /// the mouse and the keyboard
    /// </summary>
    public class HotkeyButton : PooledBehaviour {
        private static Color ACTIVE_COLOR = Color.white;

        [SerializeField]
        private Tip tip;

        [SerializeField]
        private Button button;

        [SerializeField]
        private Image icon;

        private KeyCode hotkey;

        /// <summary>
        /// Text on the button reflecting the hotkey
        /// </summary>
        [SerializeField]
        private Text hotkeyText;

        private Color INACTIVE_COLOR = Color.black;

        /// <summary>
        /// Used to detect keyboard strokes for the purposes of
        /// invoking the button
        /// </summary>
        [SerializeField]
        private UIInput input;

        [SerializeField]
        private Text text;

        private bool isHotkeyEnabled;

        /// <summary>
        /// Sets this button's properties
        /// </summary>
        /// <value>
        /// The buttonable.
        /// </value>
        public IButtonable Buttonable {
            set {
                text.text = value.ButtonText;
                icon.gameObject.SetActive(value.Sprite != null);
                icon.sprite = value.Sprite;
                tip.Sprite = value.Sprite;
                tip.Title = value.ButtonText;
                tip.Body = value.TooltipText;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => {
                    if (value.IsInvokable) {
                        value.Invoke();
                    }
                });
            }
        }

        /// <summary>
        /// Gets or sets the hotkey.
        /// </summary>
        /// <value>
        /// The hotkey.
        /// </value>
        public KeyCode Hotkey {
            get {
                return hotkey;
            }
            set {
                hotkeyText.enabled = (value != KeyCode.None);
                string text = "";
                string keyCode = value.ToString();

                // Change "Alpha1" to 1 and etc.
                if (keyCode.Contains("Alpha")) {
                    text = "" + keyCode[keyCode.Length - 1];
                } else {
                    text = keyCode;
                }
                hotkeyText.text = text;
                hotkey = value;
            }
        } //The keyboard key that interacts with this button

        /// <summary>
        /// Sets a value indicating whether this button is visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsVisible {
            set {
                if (value) {
                    ShowInactiveAppearance();
                } else {
                    ShowDisabledAppearance();
                }
                this.enabled = value;
                button.enabled = value;
            }
        }

        /// <summary>
        /// Sets a value indicating whether this instance's hotkey is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance's hotkey is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsHotkeyEnabled {
            set {
                this.isHotkeyEnabled = (Application.platform != RuntimePlatform.Android) && value;
            }
        }

        /// <summary>
        /// Reset the state of this MonoBehavior.
        /// </summary>
        public override void Reset() {
            Hotkey = KeyCode.None;
            icon.sprite = null;
        }

        /// <summary>
        /// Shows the active appearance.
        /// </summary>
        private void ShowActiveAppearance() {
            button.image.color = ACTIVE_COLOR;
            icon.color = ACTIVE_COLOR;
            text.color = INACTIVE_COLOR;
            hotkeyText.color = INACTIVE_COLOR;
        }

        /// <summary>
        /// Shows the disabled appearance.
        /// </summary>
        private void ShowDisabledAppearance() {
            text.text = string.Empty;
            tip.enabled = false;
            icon.color = Color.clear;
            button.image.color = Color.clear;
            hotkeyText.color = Color.clear;
        }

        /// <summary>
        /// Shows the inactive appearance.
        /// </summary>
        private void ShowInactiveAppearance() {
            button.image.color = INACTIVE_COLOR;
            tip.enabled = true;
            icon.color = ACTIVE_COLOR;
            text.color = ACTIVE_COLOR;
            hotkeyText.color = ACTIVE_COLOR;
        }

        // Update is called once per frame
        private void Update() {
            hotkeyText.gameObject.SetActive(isHotkeyEnabled);
            if (Input.GetKeyDown(this.Hotkey) && isHotkeyEnabled) {
                button.onClick.Invoke();
            }
        }
    }
}