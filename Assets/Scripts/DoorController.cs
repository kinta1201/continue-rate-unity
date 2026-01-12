using UnityEngine;
using UnityEngine.UI;

public class DoorController : MonoBehaviour
{
    public Button doorButton;
    public DoorView doorView;

    private UIController uiController;
    private bool isProcessing = false;

    void Start()
    {
        uiController = FindObjectOfType<UIController>();
        
        if (doorButton != null)
        {
            doorButton.onClick.AddListener(OnDoorClicked);
        }
    }

    void OnDoorClicked()
    {
        if (isProcessing) return;

        isProcessing = true;
        doorButton.interactable = false;

        bool success = GameManager.Instance.TryOpenDoor();

        if (success)
        {
            doorView.PlayOpenAnimation(() =>
            {
                uiController.UpdateStreakText();
                doorView.PlayCloseAnimation(() =>
                {
                    isProcessing = false;
                    doorButton.interactable = true;
                });
            });
        }
        else
        {
            doorView.PlayFailAnimation(() =>
            {
                uiController.ShowResult();
            });
        }
    }

    public void ResetDoor()
    {
        isProcessing = false;
        doorButton.interactable = true;
        doorView.ResetView();
    }
}