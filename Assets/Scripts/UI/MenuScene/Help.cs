using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Help : MonoBehaviour
{
    [System.Serializable]
    public class HelpButton
    {
        public Button button;
        public GameObject panel;
    }
    
    public List<HelpButton> buttons = new List<HelpButton>();

    private void Awake()
    {
        InitEventButton();
        this.gameObject.SetActive(false);
    }

    private void InitEventButton()
    {
        foreach (var button in buttons)
        {
            button.button.onClick.AddListener(() => ActiveselfPanel(button.panel));
        }
    }

    public void ActiveselfPanel(GameObject go)
    {
        go.SetActive(!go.activeSelf);
    }

    public void Close()
    {
        this.gameObject.SetActive(false);
    }
}