using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectItems : MonoBehaviour
{
    // Define the possible states using an enum
    public enum State
    {
        Default,
        Garbage,
        Animatronic
    }

    // The current state of the state machine
    private State currentState;

    // Delegate for the update function
    private delegate void UpdateDelegate();
    private UpdateDelegate updateFunction;

    void Start()
    {
        // Initialize with a default state
        ChangeState(State.Default);
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
            case State.Default:
                updateFunction = DefaultUpdate;
                DefaultInit();
                break;

            case State.Garbage:
                updateFunction = GarbageUpdate;
                GarbageInit();
                break;

            case State.Animatronic:
                updateFunction = AnimatronicUpdate;
                AnimatronicInit();
                break;

            default:
                updateFunction = null;
                Debug.LogWarning("State set to non-existent state");
                break;
        }
    }

    #region State: Default

    public Image selectionUIElement;
    [SerializeField] float selectionUIFadeSpeed = 1f;

    /// <summary>
    /// Initialization logic for the Default state.
    /// </summary>
    private void DefaultInit()
    {
        
    }

    /// <summary>
    /// Update logic for the Idle state.
    /// </summary>
    private void DefaultUpdate()
    {
        // Fade out the UI element if it's not already at 0
        if (selectionUIElement != null && selectionUIElement.color.a > 0){
            Color uiColor = selectionUIElement.color;
            uiColor.a -= selectionUIFadeSpeed * Time.deltaTime;
            selectionUIElement.color = uiColor;
        }

        // Raycast

        // Check if we're over garbage or animatronic

    }

    #endregion

    #region State: Garbage

    /// <summary>
    /// Initialization logic for the Garbage state.
    /// </summary>
    private void GarbageInit()
    {
        Debug.Log("Entering Walking State");
        // Add initialization code for Walking state here
    }

    /// <summary>
    /// Update logic for the Walking state.
    /// </summary>
    private void GarbageUpdate()
    {
        FadeInSelectionUI();
    }

    #endregion

    #region State: Animatronic

    /// <summary>
    /// Initialization logic for the Running state.
    /// </summary>
    private void AnimatronicInit()
    {
        Debug.Log("Entering Running State");
        // Add initialization code for Running state here
    }

    /// <summary>
    /// Update logic for the Running state.
    /// </summary>
    private void AnimatronicUpdate()
    {
        FadeInSelectionUI();
    }

    #endregion

    #region Shared Functions

    void FadeInSelectionUI(){
        // Fade in the UI element if it's not already at 1
        if (selectionUIElement != null && selectionUIElement.color.a < 1.0f){
            Color uiColor = selectionUIElement.color;
            uiColor.a += selectionUIFadeSpeed * Time.deltaTime;
            selectionUIElement.color = uiColor;
        }
    }

    #endregion
}