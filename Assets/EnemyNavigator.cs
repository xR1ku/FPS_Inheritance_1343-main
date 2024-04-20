using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNavigator : MonoBehaviour
{
    NavMeshAgent agent;

    [SerializeField] float wanderRange;
    [SerializeField] float trackingRange;
    [SerializeField] float attackRange;
    [SerializeField] float attackCooldown = 3f;
    [SerializeField] LayerMask playerLayer; // Define the layer where the player objects are placed
    Transform targetPlayer;
    Vector3 startingLocation;
    bool isAttacking = false;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        startingLocation = transform.position;

        // Start the coroutine to repeat the action
        StartCoroutine(EnemyBehavior());
    }

    // Coroutine to handle enemy behavior
    IEnumerator EnemyBehavior()
    {
        while (true) // This will repeat indefinitely
        {
            if (!isAttacking)
            {
                FindNearestPlayer();
                yield return new WaitForSeconds(1f); // Check for player every 1 second
            }
            else
            {
                yield return new WaitForSeconds(attackCooldown);
                isAttacking = false;
                // Reset agent's speed after the attack cooldown
                agent.speed = 3.5f; // Reset to default speed, adjust as needed
            }
        }
    }

    void FindNearestPlayer()
    {
        Collider[] players = Physics.OverlapSphere(transform.position, trackingRange, playerLayer);

        if (players.Length > 0)
        {
            float closestDistance = Mathf.Infinity;
            Transform closestPlayer = null;

            foreach (Collider player in players)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player.transform;
                }
            }

            if (closestPlayer != null)
            {
                targetPlayer = closestPlayer;
                float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);
                if (distanceToPlayer <= attackRange)
                {
                    AttackPlayer();
                }
                else
                {
                    GoToTargetPlayer();
                }
            }
        }
        else
        {
            // If no player is found, resume wandering behavior
            GoToRandomPoint();
        }
    }

    void AttackPlayer()
    {
        isAttacking = true;
        // Calculate direction towards the player
        Vector3 direction = (targetPlayer.position - transform.position).normalized;
        // Calculate the target position where the enemy will ram into
        Vector3 ramPosition = targetPlayer.position - direction * 2f; // Adjust the distance as needed

        // Set the agent's destination to the ram position
        agent.SetDestination(ramPosition);

        // Increase the agent's speed for ramming effect
        agent.speed = 10f; // Adjust speed as needed
    }

    void GoToTargetPlayer()
    {
        if (targetPlayer != null)
        {
            // Check if the target player's position is reachable
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(targetPlayer.position, path))
            {
                agent.SetDestination(targetPlayer.position);
            }
            else
            {
                // If the target position is unreachable, reset the target player
                targetPlayer = null;
            }
        }
    }

    void GoToRandomPoint()
    {
        agent.SetDestination(GetRandomPointInRange());
    }

    Vector3 GetRandomPointInRange()
    {
        Vector3 offset = new Vector3(Random.Range(-wanderRange, wanderRange),
                                0,
                                Random.Range(-wanderRange, wanderRange));

        NavMeshHit hit;

        bool gotPoint = NavMesh.SamplePosition(startingLocation + offset, out hit, 1, NavMesh.AllAreas);

        if (gotPoint)
            return hit.position;

        return Vector3.zero;
    }

    // Detect collisions with player
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Debug.Log("Enemy collided with player!");
        }
    }

    // Visualize the tracking range in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, trackingRange);
    }
}
