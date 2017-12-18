using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.View.StartScreen {

    public class CreditsSorter : MonoBehaviour {

        [SerializeField]
        private GameObject[] credits;

        private void Awake() {
            foreach (GameObject go in credits) {
                go.transform.SetParent(null);
            }
            credits = credits.OrderBy(go => go.GetComponentInChildren<Text>().text).ToArray();
            foreach (GameObject go in credits) {
                go.transform.SetParent(this.gameObject.transform);
            }
        }
    }
}