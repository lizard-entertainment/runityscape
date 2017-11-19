using Scripts.Presenter;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.View.Title {

    /// <summary>
    /// Represents a title transition with an
    /// image as well as subtitles.
    /// </summary>
    public class TitleView : MonoBehaviour {

        [SerializeField]
        private Image[] backgrounds;

        [SerializeField]
        private Image image;

        [SerializeField]
        private Text text;

        private const float DEFAULT_FADE_TIME = 0.5f;
        private const float DEFAULT_DURATION = 1f;

        public bool IsDone {
            get {
                return this.isDone;
            }
        }

        private bool isDone;

        private Coroutine routine;

        private float alpha {
            set {
                foreach (Image i in backgrounds) {
                    Util.SetImageAlpha(i, value);
                }
                Util.SetImageAlpha(image, value);
                Util.SetTextAlpha(text, value);
            }
        }

        public void Cancel() {
            this.isDone = true;
            StopAllCoroutines();
            alpha = 0;
        }

        public void Play(Sprite sprite, string text, Action callMiddle) {
            routine = StartCoroutine(TitleTransition(sprite, text, DEFAULT_FADE_TIME, DEFAULT_DURATION, callMiddle));
        }

        private IEnumerator TitleTransition(Sprite sprite, string text, float fadeTime, float duration, Action callMiddle) {
            this.isDone = false;

            image.sprite = sprite;
            image.enabled = (sprite != null);

            this.text.text = text;
            float timer = 0;

            Main.Instance.IsInputEnabled = false;

            // Fade in
            while ((timer += Time.deltaTime) < fadeTime) {
                alpha = Mathf.Lerp(0, 1, timer / fadeTime);
                yield return null;
            }
            alpha = 1;
            yield return new WaitForSeconds(duration);
            timer = 0;

            callMiddle();

            // Fade out
            while ((timer += Time.deltaTime) < fadeTime) {
                alpha = Mathf.Lerp(1, 0, timer / fadeTime);
                yield return null;
            }
            alpha = 0;
            this.isDone = true;

            Main.Instance.IsInputEnabled = true;
        }
    }
}