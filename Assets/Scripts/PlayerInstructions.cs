using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInstructions : MonoBehaviour
{
    // Define the possible states using an enum
    public enum State
    {
        None,
        GarbageCollected,
        Hide
    }

    // The current state of the state machine
    private State currentState;

    // Delegate for the update function
    private delegate void UpdateDelegate();
    private UpdateDelegate updateFunction;

    void Start()
    {
        // Initialize with a default state
        ChangeState(State.None);
    }

    void Update()
    {
        // Call the current state's update function
        updateFunction?.Invoke();
    }

    /// <summary>
    /// Method to change the state of the state machine.
    /// </summary>
    /// <param name="newState">The new state to transition to.</param>
    public void ChangeState(State newState)
    {
        // Optionally, perform any cleanup or exit logic for the current state here

        // Update the current state
        currentState = newState;

        // Set the update function delegate and call the initialization function
        switch (currentState)
        {
            case State.None:
                updateFunction = NoneUpdate;
                NoneInit();
                break;

            case State.GarbageCollected:
                updateFunction = GarbageCollectedUpdate;
                GarbageCollectedInit();
                break;

            case State.Hide:
                updateFunction = HideUpdate;
                HideInit();
                break;

            default:
                updateFunction = null;
                break;
        }
    }

    #region State: None

    /// <summary>
    /// Initialization logic for the None state.
    /// </summary>
    private void NoneInit()
    {
        Debug.Log("Entering None State");
        // Add initialization code for None state here
    }

    /// <summary>
    /// Update logic for the None state.
    /// </summary>
    private void NoneUpdate()
    {
        
    }

    #endregion

    #region State: GarbageCollected

    /// <summary>
    /// Initialization logic for the GarbageCollected state.
    /// </summary>
    private void GarbageCollectedInit()
    {
        Debug.Log("Entering GarbageCollected State");
        // Add initialization code for GarbageCollected state here
    }

    /// <summary>
    /// Update logic for the GarbageCollected state.
    /// </summary>
    private void GarbageCollectedUpdate()
    {
        
    }

    #endregion

    #region State: Hide

    /// <summary>
    /// Initialization logic for the Hide state.
    /// </summary>
    private void HideInit()
    {
        Debug.Log("Entering Hide State");
        // Add initialization code for Hide state here
    }

    /// <summary>
    /// Update logic for the Hide state.
    /// </summary>
    private void HideUpdate()
    {
        
    }

    #endregion
}