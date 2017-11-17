using Scripts.View.ObjectPool;
using Scripts.View.Sound;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.View.Sounds {

    /// <summary>
    /// This class manages the sounds that play during the game.
    /// </summary>
    public class SoundView : MonoBehaviour {
        private const string MUSIC_PREFIX = "Music/";
        private const string SOUND_PREFIX = "Sounds/";

        public IDictionary<string, AudioClip> Sound { get; private set; }

        [SerializeField]
        private OneShotHolderView oneShotHolder;

        [SerializeField]
        private LoopHolderView loopHolder;

        [SerializeField]
        private ClipView clipPrefab;

        private void Start() {
            ObjectPoolManager.Instance.Register(clipPrefab);
        }

        /// <summary>
        /// Loops the music.
        /// </summary>
        /// <param name="resourceLocation">The resource location.</param>
        public void LoopMusic(string resourceLocation) {
            if (string.IsNullOrEmpty(resourceLocation)) {
                return;
            }
            string loc = MUSIC_PREFIX + resourceLocation;
            Util.Assert(Resources.Load<AudioClip>(loc) != null, "Music location does not exist!");
            if (!Sound.ContainsKey(resourceLocation)) {
                Sound[resourceLocation] = Resources.Load<AudioClip>(loc);
            }

            loopHolder.StopAllSounds();
            ClipView clip = ObjectPoolManager.Instance.Get<ClipView>(clipPrefab);
            clip.Clip = Sound[resourceLocation];
            clip.Volume = PlayerPrefs.GetFloat(VolumeControl.MUSIC_VOLUME_KEY, VolumeControl.DEFAULT_MUSIC_VOLUME);
            loopHolder.Add(clip);
        }

        /// <summary>
        /// Plays the sound.
        /// </summary>
        /// <param name="resourceLocation">The resource location.</param>
        public void PlaySound(string resourceLocation) {
            if (string.IsNullOrEmpty(resourceLocation)) {
                return;
            }
            string loc = SOUND_PREFIX + resourceLocation;
            Util.Assert(Resources.Load<AudioClip>(loc) != null, string.Format("Sound location \"{0}\" does not exist!", loc));
            if (!Sound.ContainsKey(resourceLocation)) {
                Sound[resourceLocation] = Resources.Load<AudioClip>(loc);
            }
            StartCoroutine(PlayThenDestroy(resourceLocation));
        }

        /// <summary>
        /// Stops all sounds.
        /// </summary>
        public void StopAllSounds() {
            oneShotHolder.StopAllSounds();
            loopHolder.StopAllSounds();
        }

        private void Awake() {
            this.Sound = new Dictionary<string, AudioClip>();
        }

        /// <summary>
        /// Plays the clip then destroys it.
        /// </summary>
        /// <param name="resourceLocation">The resource location.</param>
        /// <returns></returns>
        private IEnumerator PlayThenDestroy(string resourceLocation) {
            ClipView clip = ObjectPoolManager.Instance.Get<ClipView>(clipPrefab);
            clip.Clip = Sound[resourceLocation];
            clip.Volume = PlayerPrefs.GetFloat(VolumeControl.SFX_VOLUME_KEY, VolumeControl.DEFAULT_SFX_VOLUME);
            oneShotHolder.Add(clip);
            while (clip.IsPlaying) {
                yield return null;
            }
            oneShotHolder.Stop(clip.GetInstanceID());
            yield break;
        }
    }
}