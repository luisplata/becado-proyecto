using UnityEngine;

public class EnemyAnimFunctions : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public void AttackingFalse()
    {
        animator.SetBool("isAttacking", false);
    }
}
