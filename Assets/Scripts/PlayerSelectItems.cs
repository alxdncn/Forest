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

    // Shared state
    [SerializeField]
    float spherecastRadius = 1.0f;

    [SerializeField]
    LayerMask spherecastMask;

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
        int hitLayer = CheckSpherecastHitLayer(spherecastRadius, spherecastMask);

        // Check if we're over garbage or animatronic
        if (hitLayer != -1)
        {
            string layerName = LayerMask.LayerToName(hitLayer);
            if(layerName == "Garbage")
                ChangeState(State.Garbage);
            else if(layerName == "Animatronic")
                ChangeState(State.Animatronic);

            Debug.Log("Running State Spherecast hit layer: " + layerName);
        }

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

        // Raycast
        int hitLayer = CheckSpherecastHitLayer(spherecastRadius, spherecastMask);

        // Check if we're still over garbage
        if (hitLayer != -1)
        {
            string layerName = LayerMask.LayerToName(hitLayer);
            if(layerName == "Garbage")
                return;
            else if(layerName == "Animatronic")
                ChangeState(State.Animatronic);
            else
                ChangeState(State.Default);

            Debug.Log("Running State Spherecast hit layer: " + layerName);
        } else{
            ChangeState(State.Default);
        }
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

        // Raycast
        int hitLayer = CheckSpherecastHitLayer(spherecastRadius, spherecastMask);

        // Check if we're still over an animatronic
        if (hitLayer != -1)
        {
            string layerName = LayerMask.LayerToName(hitLayer);
            if(layerName == "Animatronic")
                return;
            else if(layerName == "Garbage")
                ChangeState(State.Garbage);
            else
                ChangeState(State.Default);
                
            Debug.Log("Running State Spherecast hit layer: " + layerName);
        } else{
            ChangeState(State.Default);
        }
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

    // NOTE: This is an imperfect implementation because it doesn't actually return the first hit object.
    // It returns the object most in the direction I'm looking.
    // There are cases where this is not desired, but might not cause many issues for the time being.
    
    /// <summary>
    /// Performs a spherecast with an adjustable radius and returns the layer of the first hit object.
    /// </summary>
    /// <param name="radius">The radius of the spherecast.</param>
    /// <returns>The layer of the hit object, or -1 if nothing is hit.</returns>
    private int CheckSpherecastHitLayer(float radius, LayerMask layerMask)
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        float maxDistance = 1000f; // You can set this to a specific value if needed

        // Perform the spherecast and get all hits
        RaycastHit[] hits = Physics.SphereCastAll(origin, radius, direction, maxDistance, layerMask);

        if (hits.Length > 0)
        {
            RaycastHit bestHit = hits[0];
            float highestDotProduct = -1f; // Initialize with -1 since dot product ranges from -1 to 1

            foreach (RaycastHit hit in hits)
            {
                Vector3 toHit = (hit.point - origin).normalized;
                float dotProduct = Vector3.Dot(toHit, direction);

                if (dotProduct > highestDotProduct)
                {
                    highestDotProduct = dotProduct;
                    bestHit = hit;
                }
            }

            int hitLayer = bestHit.collider.gameObject.layer;
            Debug.Log(hitLayer);
            return hitLayer;
        }
        else
        {
            // No hit detected
            return -1;
        }
    }

    #endregion
}