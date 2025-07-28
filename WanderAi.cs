using UnityEngine;
using UnityEngine.AI;
using Controller;

public class WanderAI : MonoBehaviour
{
    public enum AIType { Follow, Flee }
    public AIType aiType = AIType.Follow;

    public Transform player;
    public float triggerDistance = 15f;      // Distance to start following/fleeing
    public float wanderRadius = 20f;         // Grazing/wandering area
    public float wanderTimer = 6f;           // Time between wander points
    public float wanderSpeed = 1.2f;         // Grazing/wandering speed
    public float triggerSpeed = 6f;          // Follow/Flee speed
    public float fleeRange = 25f;            // How far to flee (for Flee mode)

    private NavMeshAgent agent;
    private CreatureMover mover;
    private float timer;
    private Vector3 wanderTarget;
    private bool isTriggered = false;
    private float triggerCooldown = 2f;
    private float triggerTimer = 0f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        mover = GetComponent<CreatureMover>();
        timer = wanderTimer;
        SetNewWanderTarget();
        agent.speed = wanderSpeed;
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Handle cooldown after trigger (for Flee)
        if (isTriggered && aiType == AIType.Flee)
        {
            triggerTimer += Time.deltaTime;
            if (triggerTimer >= triggerCooldown)
            {
                isTriggered = false;
                triggerTimer = 0f;
            }
            else
            {
                AnimateAgent();
                return;
            }
        }

        if (distanceToPlayer <= triggerDistance)
        {
            agent.speed = triggerSpeed;
            if (aiType == AIType.Follow)
            {
                // Follow the player
                agent.SetDestination(player.position);
            }
            else // Flee
            {
                Vector3 fleeDirection = (transform.position - player.position).normalized;
                Vector3 fleeTarget = transform.position + fleeDirection * fleeRange;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(fleeTarget, out hit, fleeRange, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
                isTriggered = true;
            }
        }
        else
        {
            agent.speed = wanderSpeed;
            timer += Time.deltaTime;
            if (timer >= wanderTimer || agent.remainingDistance < 1f)
            {
                SetNewWanderTarget();
                timer = 0;
            }
        }

        AnimateAgent();
    }

    void SetNewWanderTarget()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            wanderTarget = hit.position;
            agent.SetDestination(wanderTarget);
        }
    }

    void AnimateAgent()
    {
        // Convert agent's desired velocity to local movement axis for CreatureMover
        Vector3 worldVel = agent.desiredVelocity;
        Vector3 localVel = transform.InverseTransformDirection(worldVel);
        Vector2 axis = new Vector2(localVel.x, localVel.z);

        // Determine if running (triggered) or walking (wandering)
        bool isRun = agent.speed > wanderSpeed + 0.1f;

        // Pass to CreatureMover for animation and movement
        if (mover != null)
            mover.SetInput(axis.normalized, player.position, isRun, false);
    }
}