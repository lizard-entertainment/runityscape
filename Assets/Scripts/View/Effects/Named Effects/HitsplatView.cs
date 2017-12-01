using Scripts.View.ObjectPool;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.View.Effects {

    /// <summary>
    /// View object representing a hitsplat on a portrait.
    /// </summary>
    /// <seealso cref="Scripts.View.ObjectPool.PooledBehaviour" />
    public class HitsplatView : PooledBehaviour {
        private const float ACCEL_RATE = 3.0f;

        private const float TIME_BEFORE_DECAY = 0.75f;

        private const float FADE_OUT_TIME = 0.75f;

        private const float TIME_UPSIZED = 1.0f;

        private static readonly Vector2 FINAL_SIZE = new Vector2(1f, 1f);

        private static readonly Vector2 INITIAL_SIZE = new Vector2(3f, 3f);

        [SerializeField]
        private Text text;

        [SerializeField]
        private Image image;

        /// <summary>
        /// The hitsplat animation coroutine.
        /// </summary>
        /// <param name="parent">The parent object the view is a child of</param>
        /// <param name="splatText">The text on the splat.</param>
        /// <param name="splatColor">The color of the splat.</param>
        /// <param name="splatSprite">The sprite of the splat.</param>
        /// <returns>Coroutine</returns>
        public IEnumerator Animation(Action<GameObject> parentFunc, string splatText, Color splatColor, Sprite splatSprite) {
            this.image.sprite = splatSprite;
            image.gameObject.SetActive(splatSprite != null);
            this.text.color = splatColor;
            this.text.text = splatText;
            image.color = splatColor;

            float timer = 0;
            float accel = 0;

            parentFunc(this.gameObject);

            // Shrink
            while ((timer += Time.deltaTime * accel) < TIME_UPSIZED) {
                Vector2 scale = Vector2.Lerp(INITIAL_SIZE, FINAL_SIZE, timer / TIME_UPSIZED);
                this.text.transform.localScale = scale;
                image.transform.localScale = scale;
                accel += ACCEL_RATE;
                yield return null;
            }

            // Set to final size
            this.text.transform.localScale = FINAL_SIZE;
            image.transform.localScale = FINAL_SIZE;

            float timer2 = 0;

            // Wait until decay
            while ((timer2 += Time.deltaTime) < TIME_BEFORE_DECAY) {
                yield return null;
            }

            float timer3 = 0;

            // Fade out
            while ((timer3 += Time.deltaTime) < FADE_OUT_TIME) {
                float alpha = Mathf.Lerp(1, 0, timer3 / FADE_OUT_TIME);
                Util.SetTextAlpha(this.text, alpha);
                Util.SetImageAlpha(image, alpha);
                yield return null;
            }
            ObjectPoolManager.Instance.Return(this);
            yield break;
        }

        /// <summary>
        /// Gets the total duration for the animation to occur.
        /// </summary>
        /// <value>
        /// The total duration.
        /// </value>
        public static float TOTAL_DURATION {
            get {
                return TIME_UPSIZED + TIME_BEFORE_DECAY + FADE_OUT_TIME;
            }
        }

        /// <summary>
        /// Reset the state of this MonoBehavior.
        /// </summary>
        public override void Reset() {
            text.text = string.Empty;
            text.transform.localScale = INITIAL_SIZE;
            image.transform.localScale = INITIAL_SIZE;
            text.color = Color.white;
            image.color = Color.white;
            image.sprite = null;
        }
    }
}