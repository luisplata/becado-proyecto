using System;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    public static UiManager instance;

    public GameObject dialogPanel;
    public TMPro.TextMeshProUGUI nameText;
    public TMPro.TextMeshProUGUI dialogText;
    public GameObject interactPrompt;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        HideAll();
    }

    private void HideAll()
    {
        HideDialogPanel();
        HideInteractPrompt();
    }

    public void ShowDialogPanel(string npcName, string line)
    {
        HideAll();
        if (dialogPanel != null)
            dialogPanel.SetActive(true);
        if (nameText != null)
            nameText.text = npcName;
        if (dialogText != null)
            dialogText.text = line;
    }

    public void HideDialogPanel()
    {
        if (dialogPanel != null)
            dialogPanel.SetActive(false);
    }

    public void ShowInteractPrompt()
    {
        HideAll();
        if (interactPrompt != null)
            interactPrompt.SetActive(true);
    }

    public void HideInteractPrompt()
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }
}