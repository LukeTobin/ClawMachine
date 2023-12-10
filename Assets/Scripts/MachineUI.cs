using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MachineUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button controlButton;
    [SerializeField] private TMP_Text buttonText;
    
    private void Awake()
    {
        buttonText.text = "Waiting for Claw Machine";
        controlButton.interactable = false;
        
        controlButton.onClick.AddListener(ControlButtonPressed);
    }

    private void ControlButtonPressed()
    {
        EventManager.Invoke(EventManager.Event.OnControlButtonClick);
    }

    private void EnableStandbyUI()
    {
        buttonText.text = "Start";
        controlButton.interactable = true;
    }

    private void ShowControllableUI()
    {
        buttonText.text = "Move Claw";
    }

    private void ShowReturningUI()
    {
        buttonText.text = "Returning";
        controlButton.interactable = false;
    }

    private void OnEnable()
    {
        EventManager.Subscribe(EventManager.Event.OnClawStandby, EnableStandbyUI);
        EventManager.Subscribe(EventManager.Event.OnClawControllable, ShowControllableUI);
        EventManager.Subscribe(EventManager.Event.OnClawReturning, ShowReturningUI);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe(EventManager.Event.OnClawStandby, EnableStandbyUI);
        EventManager.Unsubscribe(EventManager.Event.OnClawControllable, ShowControllableUI);
        EventManager.Unsubscribe(EventManager.Event.OnClawReturning, ShowReturningUI);
    }
}
