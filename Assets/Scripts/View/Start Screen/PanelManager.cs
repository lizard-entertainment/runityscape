using UnityEngine;

using UnityEngine.UI;

namespace Scripts.View.StartScreen {

    public class PanelManager : MonoBehaviour {

        [SerializeField]
        private string panelName;

        [SerializeField]
        private Text textToOverride;

        [SerializeField]
        private Button[] buttonsToDisable;

        private string originalText;

        private void Start() {
            this.originalText = textToOverride.text;
            gameObject.SetActive(false);
        }

        public void ClosePanel() {
            gameObject.SetActive(false);
            foreach (Button button in buttonsToDisable) {
                button.gameObject.SetActive(true);
            }
            textToOverride.text = originalText;
        }

        public void OpenPanel() {
            PanelManager[] allPanels = FindObjectsOfType<PanelManager>();
            foreach (PanelManager panel in allPanels) {
                panel.ClosePanel();
            }
            foreach (Button button in buttonsToDisable) {
                button.gameObject.SetActive(false);
            }
            textToOverride.text = panelName;
            gameObject.SetActive(true);
        }
    }
}