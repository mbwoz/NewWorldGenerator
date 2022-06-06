using UnityEngine;
using UnityEngine.UI;

public class PopUpOverlay : MonoBehaviour {
    
    private Text popUpText;

    void Start() {
        popUpText = gameObject.GetComponent<Text>();
    }

    void Update() {
        if (popUpText.color.a > 0.0f) {
            Color color = Color.white;
            color.a = popUpText.color.a - 0.02f;
            popUpText.color = color;
        }
    }

    public void SetPopUp(string _text) {
        popUpText.text = _text;
        popUpText.color = Color.white;
    }
}
