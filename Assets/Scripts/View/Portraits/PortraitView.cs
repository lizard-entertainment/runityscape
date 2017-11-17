using Scripts.Model.Tooltips;
using Scripts.View.ObjectPool;
using Scripts.View.Tooltip;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.View.Portraits {

    /// <summary>
    /// This class represents a Portrait viewable
    /// on either the left or right side of the screen.
    /// </summary>
    public class PortraitView : PooledBehaviour, ITippable {

        [SerializeField]
        private GameObject effectsHolder;

        [SerializeField]
        private Image iconImage;

        [SerializeField]
        private Text portraitName;

        [SerializeField]
        private BuffHolderView buffsHolder;

        [SerializeField]
        private ResourceHolderView resourcesHolder;

        [SerializeField]
        private Tooltip.Tip tip;

        private Vector3 startingPosition;

        /// <summary>
        /// Gets the effects holder. Effects are children of this!
        /// </summary>
        /// <value>
        /// The effects holder.
        /// </value>
        public GameObject EffectsHolder {
            get {
                return effectsHolder;
            }
        }

        public Image Image {
            get {
                return iconImage;
            }
            set {
                iconImage = value;
            }
        }

        public string PortraitName {
            get {
                return portraitName.text;
            }
            set {
                portraitName.text = value;
            }
        }

        public Text PortraitText {
            get {
                return portraitName;
            }
        }

        public Sprite Sprite {
            get {
                return iconImage.sprite;
            }
            set {
                iconImage.sprite = value;
            }
        }

        Tip ITippable.Tip {
            get {
                return tip;
            }
        }

        private void Start() {
            this.startingPosition = iconImage.GetComponent<RectTransform>().anchoredPosition;
        }

        /// <summary>
        /// Setups the specified sprite.
        /// </summary>
        /// <param name="sprite">The sprite.</param>
        /// <param name="title">The title.</param>
        /// <param name="body">The body.</param>
        /// <param name="resources">The resources.</param>
        /// <param name="buffs">The buffs.</param>
        /// <param name="isRevealed">if set to <c>true</c> [is revealed].</param>
        public void Setup(Sprite sprite, string title, string body, List<ResourceHolderView.ResourceContent> resources, IList<BuffHolderView.BuffContent> buffs, bool isRevealed) {
            tip.Setup(new TooltipBundle(sprite, title, body));
            PortraitName = title;
            Sprite = sprite;
            if (isRevealed) {
                resources.Sort((left, right) => left.Id.CompareTo(right.Id));
                resourcesHolder.AddContents(resources);
                buffsHolder.AddContents(buffs);
            } else {
                resourcesHolder.AddContents(new ResourceHolderView.ResourceContent[0]);
                buffsHolder.AddContents(new BuffHolderView.BuffContent[0]);
            }
        }

        /// <summary>
        /// Reset the state of this MonoBehavior.
        /// </summary>
        public override void Reset() {
            PortraitName = "";
            PortraitText.color = Color.white;
            Image.color = Color.white;
            Image.enabled = true;
            Image.GetComponent<RectTransform>().anchoredPosition = startingPosition;
            tip.Reset();

            this.StopAllCoroutines();

            // Not a class because its just a gameobject things get parented to
            PooledBehaviour[] ces = effectsHolder.GetComponentsInChildren<PooledBehaviour>();
            for (int i = 0; i < ces.Length; i++) {
                ObjectPoolManager.Instance.Return(ces[i]);
            }
        }

        private void Update() {
        }
    }
}