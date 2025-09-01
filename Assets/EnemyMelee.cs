using UnityEngine;
using UnityEngine.AI;

public class EnemyMelee : MonoBehaviour
{
    public Transform player;
    public float attackRange = 2f;
    public float minDistance = 1.2f; // Distancia mínima para no pegarse
    public float attackCooldown = 1.5f;

    private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    private float lastAttackTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Mirar siempre hacia el jugador (solo en Y)
        Vector3 lookPos = player.position - transform.position;
        lookPos.y = 0;
        if (lookPos != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPos), 0.1f);

        if (distance > attackRange)
        {
            // Demasiado lejos → acercarse
            agent.isStopped = false;
            agent.SetDestination(player.position);
            animator.SetBool("isMoving", true);
        }
        else if (distance < minDistance)
        {
            // Demasiado cerca → retroceder
            agent.isStopped = false;

            // Calcular dirección opuesta al jugador
            Vector3 dirBack = transform.position - player.position;
            Vector3 newPos = transform.position + dirBack.normalized * 1.5f; // mover un poco hacia atrás

            agent.SetDestination(newPos);
            animator.SetBool("isMoving", true);

        }
        else
        {
            // En rango correcto → atacar
            agent.isStopped = true;
            animator.SetBool("isMoving", false);

            // Atacar SOLO si el enemigo ya está quieto
            if (agent.velocity.magnitude < 0.05f)
            {
                if (Time.time > lastAttackTime + attackCooldown)
                {
                    animator.SetTrigger("attack"); // Hook Punch
                    lastAttackTime = Time.time;
                }
            }
        }

    }
}
