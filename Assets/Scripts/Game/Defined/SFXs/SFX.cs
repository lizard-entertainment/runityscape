using Scripts.Model.Characters;
using Scripts.Model.Spells;
using Scripts.Presenter;
using Scripts.View.Effects;
using Scripts.View.ObjectPool;
using Scripts.View.Sounds;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.Game.Defined.SFXs {

    /// <summary>
    /// Various special effects for battle
    /// </summary>
    public static class SFX {
        public static readonly Vector3 DEFAULT_PORTRAIT_POS = new Vector3(0, -45, 0);

        /// <summary>
        /// Creates a hitsplat.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="splatText">The splat text.</param>
        /// <param name="splatColor">The splat color.</param>
        /// <param name="sprite">The sprite.</param>
        /// <returns></returns>
        public static IEnumerator DoHitSplat(IPortraitable parent, string splatText, Color splatColor, Sprite sprite = null) {
            HitsplatView hp = ObjectPoolManager.Instance.Get(EffectsManager.Instance.Hitsplat);
            return hp.Animation(go => parent.ParentToEffects(go), splatText, splatColor, sprite);
        }

        /// <summary>
        /// Plays the sound.
        /// </summary>
        /// <param name="soundLoc">The sound loc.</param>
        /// <returns></returns>
        public static IEnumerator PlaySound(string soundLoc) {
            Main.Instance.Sound.PlaySound(soundLoc);
            yield break;
        }

        public static IEnumerator LoopMusic(string soundLoc) {
            Main.Instance.Sound.LoopMusic(soundLoc);
            yield break;
        }

        /// <summary>
        /// Creates a hitsplat.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="splat">The splat.</param>
        /// <returns></returns>
        public static IEnumerator DoHitSplat(IPortraitable parent, SplatDetails splat) {
            return DoHitSplat(parent, splat.Text, splat.Color, splat.Sprite);
        }

        /// <summary>
        /// Waits for the specified duration.
        /// </summary>
        /// <param name="duration">The duration.</param>
        /// <returns></returns>
        public static IEnumerator Wait(float duration) {
            yield return new WaitForSeconds(duration);
        }

        /// <summary>
        /// Effect where portrait runs up to another portrait, hits it, and retreats
        /// </summary>
        /// <param name="mover">The moving portrait.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="duration">The duration.</param>
        /// <param name="soundLoc">The sound loc.</param>
        /// <returns></returns>
        public static IEnumerator DoMeleeEffect(IPortraitable mover, IPortraitable destination, float duration, string soundLoc, bool isHaveStars = false) {
            // Move mover to upper layer so it is on top of all elements
            int index = mover.RectTransform.GetSiblingIndex();
            GameObject parent = mover.RectTransform.parent.gameObject;
            Util.Parent(mover.RectTransform.gameObject, EffectsManager.Instance.Foreground);
            yield return MoveTowards(mover.RectTransform, destination.RectTransform, duration / 3);
            Presenter.Main.Instance.Sound.PlaySound(soundLoc);
            yield return Shake(destination.RectTransform, 100, duration / 3);
            Vector2 moverOriginalPos = mover.OriginalPos.position;
            yield return MoveBack(mover.RectTransform, moverOriginalPos, duration / 3);
            if (isHaveStars) {
                yield return DoStarEffect(destination);
            }
            Util.Parent(mover.RectTransform.gameObject, parent);
            mover.RectTransform.SetSiblingIndex(index);
        }

        /// <summary>
        /// Death effect for a portrait.
        /// </summary>
        /// <param name="portrait">The portrait.</param>
        /// <param name="effectHolder">The effect holder.</param>
        /// <param name="fadeDuration">Duration of the fade.</param>
        /// <returns></returns>
        public static IEnumerator DoDeathEffect(IPortraitable portrait, float fadeDuration) {
            Rect rect = portrait.RectTransform.rect;
            Vector2 dimensions = new Vector2(rect.width, rect.height);

            ExplosionView ev = ObjectPoolManager.Instance.Get(EffectsManager.Instance.Explosion);
            ev.Dimensions = dimensions;
            portrait.ParentToEffects(ev.gameObject);
            ev.Play();
            yield return DoHitSplat(portrait, "DEFEAT", Color.red, Util.GetSprite("skull-crossed-bones"));
            ObjectPoolManager.Instance.Return(ev);
        }

        public static IEnumerator DoStarEffect(IPortraitable portrait) {
            ExplosionView ev = ObjectPoolManager.Instance.Get(EffectsManager.Instance.Explosion);
            Rect rect = portrait.RectTransform.rect;
            ev.Dimensions = new Vector2(rect.width, rect.height);
            portrait.ParentToEffects(ev.gameObject);
            ev.Play();
            yield return new WaitUntil(() => ev.IsDone);
        }

        public static IEnumerator DoSteamEffect(IPortraitable portrait, Color color) {
            ExplosionView ev = ObjectPoolManager.Instance.Get(EffectsManager.Instance.SteamBurst);
            ev.Play();
            ev.Color = color;
            portrait.ParentToEffects(ev.gameObject);
            yield return new WaitUntil(() => portrait.RectTransform != null);
            bool isShakeDone = false;
            Main.Instance.StartCoroutine(Shake(portrait.RectTransform, 100, 0.5f, () => isShakeDone = true));
            yield return new WaitUntil(() => ev.IsDone && isShakeDone);
            ObjectPoolManager.Instance.Return(ev);
        }

        public static IEnumerator DoSteamEffect(IPortraitable portrait) {
            yield return DoSteamEffect(portrait, Color.white);
        }

        /// <summary>
        /// Moves mover towards a destination.
        /// </summary>
        /// <param name="mover">The mover.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="duration">The duration.</param>
        /// <returns></returns>
        private static IEnumerator MoveTowards(RectTransform mover, RectTransform destination, float duration) {
            float timer = 0;

            Vector3 destinationPos = destination.position;
            float width = destination.rect.width;

            // Do position corrections so we end up next to the icon instead of on top of it, but the mover needs to be on the inner side always

            // Mover ---- Destination,
            if (mover.position.x < destinationPos.x) {
                destinationPos -= new Vector3(width, 0, 0);
            } else { // Destination ---- Mover
                destinationPos += new Vector3(width, 0, 0);
            }
            while ((timer += Time.deltaTime) < duration) {
                mover.position = Vector2.Lerp(mover.position, destinationPos, Mathf.SmoothStep(0, 1, timer / duration));
                yield return null;
            }
        }

        /// <summary>
        /// Shakes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="maxIntensity">The maximum intensity.</param>
        /// <param name="duration">The duration.</param>
        /// <returns></returns>
        private static IEnumerator Shake(RectTransform item, float maxIntensity, float duration, Action callback = null) {
            callback = callback ?? (() => { });
            Vector2 original = item.position;
            Vector2 originalAnchor = item.anchoredPosition;
            float timer = 0;
            while ((timer += Time.deltaTime) < duration) {
                float intensity = Mathf.SmoothStep(maxIntensity, 0, timer / duration);
                item.transform.position = new Vector2(Mathf.Sin(UnityEngine.Random.value) * intensity + original.x, Mathf.Sin(UnityEngine.Random.value) * intensity + original.y);
                yield return null;
            }
            item.position = original;
            item.anchoredPosition = originalAnchor;
            callback();
        }

        /// <summary>
        /// Moves the mover back to a destination.
        /// </summary>
        /// <param name="mover">The mover.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="duration">The duration.</param>
        /// <returns></returns>
        private static IEnumerator MoveBack(Transform mover, Vector2 destination, float duration) {
            float timer = 0;

            // Move towards destination
            while ((timer += Time.deltaTime) < duration) {
                mover.position = Vector2.Lerp(mover.position, destination, Mathf.SmoothStep(0, 1, timer / duration));
                yield return null;
            }
            mover.position = destination;
        }
    }
}