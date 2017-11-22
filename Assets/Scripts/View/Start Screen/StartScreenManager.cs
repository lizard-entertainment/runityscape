using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.View.StartScreen {

    public class StartScreenManager : MonoBehaviour {

        [SerializeField]
        private PanelManager credits;

        [SerializeField]
        private PanelManager volume;

        private void Start() {
            credits.gameObject.SetActive(true);
            volume.gameObject.SetActive(true);
        }

        public void GoToMain() {
            SceneManager.LoadScene("Main");
        }

        public void GoToCredits() {
            credits.OpenPanel();
        }

        public void GoToVolume() {
            volume.OpenPanel();
        }

        public void QuitGame() {
            Application.Quit();
        }
    }
}