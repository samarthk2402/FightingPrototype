using UnityEngine;

[CreateAssetMenu(fileName = "New Attack", menuName = "Attacks/Attack")]
public class Attack : ScriptableObject
{
    public int damage = 10; // Amount of damage the attack does
    public float knockbackForce = 5f; // Force applied for knockback
    public AnimationClip animationClip; // Animation to play during the attack

    // Method to execute the attack
    public void ExecuteAttack(Transform attacker, Transform target)
    {
        // Apply damage to the target (you might have a separate system for this)
        // Apply knockback force to the target
        // Play animation
        if (animationClip != null && attacker != null)
        {
            // Assuming the attacker has an Animator component
            Animator animator = attacker.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play(animationClip.name);
            }
        }
    }
}
