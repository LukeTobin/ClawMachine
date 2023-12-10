using System;
using System.Collections;
using UnityEngine;

public class ClawMachine : MonoBehaviour
{
    private enum ClawState
    {
        Standby,
        MovingForward,
        MovingRight,
        MovingDown,
        MovingUp,
        MovingLeft,
        MovingBack
    }
    
    [Header("References")] 
    [SerializeField] private Transform clawObject;
    [SerializeField] private Transform clawConnection;
    [SerializeField] private Transform clawHandle1, clawHandle2, clawHandle3;
    [SerializeField] private Animator clawAnimator;
    
    [Header("Settings")]
    [SerializeField] private float positionLimitZX = 0.12f;
    [SerializeField] private Vector2 positionLimitY = new Vector2(0.12f, -0.02f);
    [SerializeField] private float movementSpeed, dropRiseSpeed, rotationSpeed;
    
    [SerializeField] private ClawState state;
    private IEnumerator movementEnumerator;
    private Vector3 originClawPosition;
    private bool clawsOpen;

    private const string OPEN_ANIMATION = "open";
    private const string CLOSE_ANIMATION = "close";
    private const string RELEASE_ANIMATION = "release";

    private void Start()
    {
        originClawPosition = clawObject.localPosition;
        
        // Start in final state, so it transitions to Standby
        clawsOpen = true;
        state = ClawState.MovingBack;
        TransitionState();
    }
    
    private void TransitionState()
    {
        if(movementEnumerator != null) 
            StopCoroutine(movementEnumerator);
        
        switch (state)
        {
            case ClawState.Standby:
                EventManager.Invoke(EventManager.Event.OnClawControllable);
                state = ClawState.MovingForward;
                movementEnumerator = MoveClawObject(Vector3.back);
                StartCoroutine(movementEnumerator);
                break;
            
            case ClawState.MovingForward:
                state = ClawState.MovingRight;
                movementEnumerator = MoveClawObject(Vector3.left);
                StartCoroutine(movementEnumerator);
                break;
            
            case ClawState.MovingRight:
                EventManager.Invoke(EventManager.Event.OnClawReturning);
                state = ClawState.MovingDown;
                movementEnumerator = MoveClawConnector(Vector3.down);
                StartCoroutine( OpenClaws(OPEN_ANIMATION, () => StartCoroutine(movementEnumerator)) );
                break;
            
            case ClawState.MovingDown:
                state = ClawState.MovingUp;
                movementEnumerator = MoveClawConnector(Vector3.up);
                StartCoroutine( OpenClaws(CLOSE_ANIMATION, () => StartCoroutine(movementEnumerator)) );
                
                break;
            
            case ClawState.MovingUp:
                state = ClawState.MovingLeft;
                movementEnumerator = MoveClawObject(Vector3.right);
                StartCoroutine(movementEnumerator);
                break;
            
            case ClawState.MovingLeft:
                state = ClawState.MovingBack;
                movementEnumerator = MoveClawObject(Vector3.forward);
                StartCoroutine(movementEnumerator);
                break;
            
            case ClawState.MovingBack:
                StartCoroutine( OpenClaws(RELEASE_ANIMATION) );

                clawObject.localPosition = originClawPosition;
                
                state = ClawState.Standby;
                EventManager.Invoke(EventManager.Event.OnClawStandby);
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void TryTransitionState()
    {
        switch (state)
        {
            case ClawState.Standby:
                TransitionState();
                break;
            
            case ClawState.MovingForward:
                TransitionState();
                break;
            
            case ClawState.MovingRight:
                TransitionState();
                break;
        }
    }

    private IEnumerator MoveClawObject(Vector3 moveDirection)
    {
        while (true)
        {
            clawObject.Translate(moveDirection * (movementSpeed * Time.deltaTime));
            
            // Get value based on current state
            float value = state is ClawState.MovingForward or ClawState.MovingBack
                ? Mathf.Abs(clawObject.localPosition.z)
                : Mathf.Abs(clawObject.localPosition.x);
            
            if (value >= positionLimitZX)
            {
                TransitionState();
                break;
            }

            yield return null;
        }
    }
    
    private IEnumerator MoveClawConnector(Vector3 moveDirection)
    {
        while (true)
        {
            clawConnection.Translate(moveDirection * movementSpeed * Time.deltaTime, Space.World);
            
            if (state == ClawState.MovingDown)
            {
                if (clawConnection.localPosition.y <= positionLimitY.x)
                {
                    TransitionState();
                    break;
                }
            }
            else if (state == ClawState.MovingUp)
            {
                if (clawConnection.localPosition.y >= positionLimitY.y)
                {
                    TransitionState();
                    break;
                }
            }

            yield return null;
        }
    }

    private IEnumerator OpenClaws(string animation, Action onCompleteAction = null)
    {
        clawAnimator.SetTrigger(animation);
        
        yield return new WaitForSecondsRealtime(1f);
        
        onCompleteAction?.Invoke();
        
        yield return null;
    }

    private void OnEnable()
    {
        EventManager.Subscribe(EventManager.Event.OnControlButtonClick, TryTransitionState);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe(EventManager.Event.OnControlButtonClick, TryTransitionState);
    }
}