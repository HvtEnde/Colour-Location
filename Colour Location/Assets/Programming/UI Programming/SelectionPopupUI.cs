using UnityEngine;
using TMPro; 

public class SelectionPopupUI : MonoBehaviour
{
    public GameObject popupPanel; // Assign the Panel or just the Text object
    public TMP_Text popupText;    // Assign the TMP text component

    public void Show(string objectName, string tag)
    {
        popupText.text = $"Selected: {objectName}\nTag: {tag}";
        popupPanel.SetActive(true);
    }

    public void Hide()
    {
        popupPanel.SetActive(false);
    }
}
