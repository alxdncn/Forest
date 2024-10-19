using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyController : MonoBehaviour
{
    // States
    private enum State { Calm, Suspicious, SeesPlayer }
    private State currentState;

    // Delegate for state updates
    private delegate void StateUpdate();
    private StateUpdate stateUpdate;

    // Serialized Fields
    [SerializeField] private float viewAngle = 60f;
    [SerializeField] [Range(0,1)] private float viewEdgePortion = 0.2f;
    [SerializeField] private float viewRange = 10f;
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField] private bool showDebugView = true;
    [SerializeField] private Transform[] pointsOfInterest;
    [SerializeField] private float suspiciousSpeed = 2f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float calmSpeed = 3.5f;
    [SerializeField] private float pauseDuration = 2f;
    [SerializeField] private float rotationSpeed = 120f; // Degrees per second
    [SerializeField] [Range(0,1)] private float crouchingDetectionMultiplier = 0.4f; // Adjusts detection chance when player is crouching

    // Private variables
    private NavMeshAgent agent;
    private Transform player;
    private int currentPoint = 0;
    private bool isPaused = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = calmSpeed;
        player = GameObject.FindWithTag("Player").transform;

        InitializeState(State.Calm);

        if (showDebugView)
        {
            for(int i = 0; i < transform.childCount; i++){
                transform.GetChild(i).gameObject.SetActive(true);
                transform.GetChild(i).GetComponent<DebugCharacterView>().obstacleLayers = obstacleLayers;
                if(i == 0)
                    transform.GetChild(i).GetComponent<DebugCharacterView>().drawAngle = viewAngle * (1f - viewEdgePortion);
                else{
                    transform.GetChild(i).GetComponent<DebugCharacterView>().drawAngle = viewAngle;
                }
            }
        } else
        {
            for(int i = 0; i < transform.childCount; i++){
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        stateUpdate?.Invoke();
        DetectPlayer();

        
    }

    private void InitializeState(State newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case State.Calm:
                stateUpdate = CalmUpdate;
                InitCalm();
                break;
            case State.Suspicious:
                stateUpdate = SuspiciousUpdate;
                InitSuspicious();
                break;
            case State.SeesPlayer:
                stateUpdate = SeesPlayerUpdate;
                InitSeesPlayer();
                break;
        }
    }

    #region Calm State
    private void InitCalm()
    {
        agent.speed = calmSpeed;
    }

    private void CalmUpdate()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            MoveToNextPoint();
        }
    }
    #endregion

    #region Suspicious State
    private void InitSuspicious()
    {
        agent.speed = suspiciousSpeed;
    }

    private void SuspiciousUpdate()
    {
        if (!isPaused)
        {
            StartCoroutine(PauseAndLookAround());
        }
    }
    #endregion

    #region SeesPlayer State
    private void InitSeesPlayer()
    {
        agent.speed = chaseSpeed;
    }

    private void SeesPlayerUpdate()
    {
        agent.SetDestination(player.position);
    }
    #endregion

    private void DetectPlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if (distanceToPlayer < viewRange && angleToPlayer < viewAngle / 2)
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position, directionToPlayer.normalized, out hit, distanceToPlayer))
            {
                // Raycast hit something
                if (hit.transform == player)
                {
                    // We can see the player directly
                    HandlePlayerDetection(angleToPlayer);
                }
                else
                {
                    // Ray hit something else before reaching the player
                    ObjectDensity densityObj = hit.transform.GetComponent<ObjectDensity>();
                    if (densityObj != null)
                    {
                        // Handle detection based on density
                        float density = Mathf.Clamp01(densityObj.Density);
                        PlayerBehavior playerBehavior = player.GetComponent<PlayerBehavior>();

                        if (playerBehavior != null)
                        {
                            float detectionChance = CalculateDetectionChance(density, playerBehavior.IsCrouching);

                            if (Random.value < detectionChance)
                            {
                                InitializeState(State.Suspicious);
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Player script missing!");
                        }
                    }
                    else
                    {
                        // Hit an object without ObjectDensity, treat as solid obstacle, cannot see player
                        // Do nothing, the enemy does not see the player
                    }
                }
            }
            else
            {
                // No obstacle between enemy and player, can see the player directly
                HandlePlayerDetection(angleToPlayer);
            }
        }
        else if (currentState != State.Calm)
        {
            InitializeState(State.Calm);
        }
    }

    private void HandlePlayerDetection(float angleToPlayer)
    {
        PlayerBehavior playerBehavior = player.GetComponent<PlayerBehavior>();
        if (playerBehavior != null)
        {
            if (angleToPlayer > (viewAngle / 2) * Mathf.Clamp01(1f - viewEdgePortion))
            {
                if (playerBehavior.IsCrouching)
                {
                    InitializeState(State.Suspicious);
                }
                else
                {
                    InitializeState(State.SeesPlayer);
                }
            }
            else
            {
                InitializeState(State.SeesPlayer);
            }
        }
    }

    private float CalculateDetectionChance(float density, bool isPlayerCrouching)
    {
        float detectionChance = 1f - density;

        if (isPlayerCrouching)
        {
            detectionChance *= crouchingDetectionMultiplier;
        }

        detectionChance = Mathf.Clamp01(detectionChance);

        return detectionChance;
    }

    private void MoveToNextPoint()
    {
        if (pointsOfInterest.Length == 0)
            return;

        // Randomly choose the next point
        currentPoint = Random.Range(0, pointsOfInterest.Length);
        agent.destination = pointsOfInterest[currentPoint].position;

        // Add variation in movement
        Vector3 randomOffset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        agent.destination += randomOffset;
    }

    private System.Collections.IEnumerator PauseAndLookAround()
    {
        isPaused = true;
        agent.isStopped = true;

        float elapsedTime = 0f;
        while (elapsedTime < pauseDuration)
        {
            // Rotate to look around
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        agent.isStopped = false;
        agent.SetDestination(player.position);
        isPaused = false;
    }
}
