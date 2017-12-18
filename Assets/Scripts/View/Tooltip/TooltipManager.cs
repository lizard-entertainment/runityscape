using Scripts.View.ObjectPool;
using Scripts.View.Tooltip;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Script.View.Tooltip {

    /// <summary>
    /// Tooltip manager, shows and hides the tooltip
    /// as needed.
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class TooltipManager : MonoBehaviour {
        private const int MOBILE_OFFSET_MULTIPLIER = 3;

        /// <summary>
        /// The box
        /// </summary>
        [SerializeField]
        private TooltipBox box;

        /// <summary>
        /// The GRC
        /// </summary>
        [SerializeField]
        private GraphicRaycaster grc;

        /// <summary>
        /// The distance multiplier
        /// </summary>
        [SerializeField]
        private float distanceMultiplier;

        /// <summary>
        /// The is over tooltip content
        /// </summary>
        private bool isOverTooltipContent;

        /// <summary>
        /// The content identifier
        /// </summary>
        private int contentID;

        private bool IsMeetTooltipCondition {
            get {
                return isOverTooltipContent
                && Cursor.visible
                && (!Application.isMobilePlatform || (Input.touchCount > 0 && Input.GetTouch(0).phase != TouchPhase.Ended));
            }
        }

        private void Start() {
            if (Application.isMobilePlatform) {
                this.distanceMultiplier *= MOBILE_OFFSET_MULTIPLIER;
            }
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        private void Update() {
            box.Pivot = CalculatePivot();
            box.Position = CalculatePosition();
            UpdateTooltip();
            if (IsMeetTooltipCondition) {
                StartCoroutine(DisplayTooltip(contentID));
            } else {
                SetRenderers(false);
            }
        }

        /// <summary>
        /// Updates the tooltip.
        /// </summary>
        private void UpdateTooltip() {
            List<RaycastResult> results = new List<RaycastResult>();
            PointerEventData ped = new PointerEventData(null);
            ped.position = Input.mousePosition;
            grc.Raycast(ped, results);

            bool isOverContent = false;
            box.Sprite = null;
            box.Title = string.Empty;
            box.Body = string.Empty;

            foreach (RaycastResult rr in results) {
                Tip tip = rr.gameObject.GetComponent<Tip>();
                if (tip != null && tip.enabled && !string.IsNullOrEmpty(tip.Body)) {
                    box.IsIconEnabled = (tip.Sprite != null);
                    box.Sprite = tip.Sprite;
                    box.Title = tip.Title;
                    box.Body = tip.Body;
                    isOverContent = true;
                    contentID = tip.gameObject.GetInstanceID();
                }
            }
            isOverTooltipContent = isOverContent;
        }

        /// <summary>
        /// Disable renderers instead of disabling the tooltip box object so that the
        /// content size fitter has enough time to work with the new text.
        /// If we disable the gameobject instead, the tooltip box will appear to jump, since it
        /// needs a moment to resize to accomodate the new text.
        /// </summary>
        /// <param name="isEnabled">True if renderers of the tooltip box object and its children should be enabled</param>
        private void SetRenderers(bool isEnabled) {
            CanvasRenderer[] renderers = box.GetComponentsInChildren<CanvasRenderer>();
            foreach (CanvasRenderer r in renderers) {
                r.SetAlpha(isEnabled ? 1 : 0);
            }
        }

        /// <summary>
        /// Delay showing of the tooltip so that
        /// there is enough time for the tooltip box's contentsizefitters
        /// to resize itself for the new text.
        /// </summary>
        /// <param name="instanceID">The instance identifier.</param>
        /// <returns></returns>
        private IEnumerator DisplayTooltip(int instanceID) {
            int id = instanceID;
            yield return new WaitForSeconds(0.00001f); // Delay is just long enough for box to be updated
            SetRenderers(this.contentID == id && isOverTooltipContent); // dont show if instanceID does not match current hovered
        }

        /// <summary>
        /// Calculates the position.
        /// </summary>
        /// <returns></returns>
        private Vector2 CalculatePosition() {
            Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            float width = Screen.width;
            float height = Screen.height;

            float x = 0;
            float y = 0;

            if (Input.mousePosition.x < width / 2) {
                x = box.Outline.effectDistance.x * distanceMultiplier;
            } else {
                x = -box.Outline.effectDistance.x * distanceMultiplier;
            }

            if (Input.mousePosition.y < height / 2) {
                y = -box.Outline.effectDistance.y * distanceMultiplier;
            } else {
                y = box.Outline.effectDistance.y * distanceMultiplier;
            }

            return new Vector2(position.x + x, position.y + y);
        }

        /// <summary>
        /// Calculates the pivot.
        /// </summary>
        /// <returns></returns>
        private Vector2 CalculatePivot() {
            float width = Screen.width;
            float height = Screen.height;

            float x = 0;
            float y = 0;

            if (Input.mousePosition.x < width / 2) {
                x = 0;
            } else {
                x = 1;
            }

            if (Input.mousePosition.y < height / 2) {
                y = 0;
            } else {
                y = 1;
            }

            return new Vector2(x, y);
        }
    }
}