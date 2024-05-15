using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Animator anim;
    public LayerMask triggerLayer; 
    public GameObject ragdoll;
    public GameObject bloodEffect;
    public GameObject BloodAttach;
    public Vector3 direction;
    public Rigidbody rb;
    public Slider healthBar;
    public float knockback;
    public Light DirLight;

    public float moveSpeed;
    public float rotationSpeed;
    public Transform target;

    private bool isHit;
    private float hitCount;
    public float hitTimer = 1f;

    public float health = 5;
    public float range = 1;

    public enum State
    {
        Advancing,
        Attacking,
        Dodging
    }

    public State enemyState = State.Advancing;

    public float attackDelay = 2;
    private float attackTimer = 0;

    public bool stunned;
    private float stunCount;
    public float stunTimer = 1f;

    public Vector3 knockbackVelocity;


    void Start(){
        rb = GetComponent<Rigidbody>();
        // foreach(Rigidbody rb in ragdoll.GetComponentsInChildren<Rigidbody>()){
        //     rb.isKinematic = true;
        // }
        anim.enabled = true;
    }

    Transform GetNearestObject(Transform hit, Vector3 hitPos)
    {
        var closestPos = 100f;
        Transform closestBone = null;
        var childs = hit.GetComponentsInChildren<Transform>();

        foreach (var child in childs)
        {
            var dist = Vector3.Distance(child.position, hitPos);
            if (dist < closestPos)
            {
                closestPos = dist;
                closestBone = child;
            }
        }

        var distRoot = Vector3.Distance(hit.position, hitPos);
        if (distRoot < closestPos)
        {
            closestPos = distRoot;
            closestBone = hit;
        }
        return closestBone;
    }

    private void OnTriggerEnter(Collider other)
    {
        RaycastHit hit;
        // Check if the entering collider is on the specified layer
        if (((1 << other.gameObject.layer) & triggerLayer) != 0)
        {
            // Calculate the direction from the impact point to the trigger position
            Vector3 impactDirection = (transform.position - other.ClosestPoint(transform.position)).normalized;

            // Calculate the velocity change
            Vector3 velocityChange = impactDirection * knockback;

            // Apply the velocity change to the Rigidbody
            knockbackVelocity = velocityChange;

            rb.AddForce(knockbackVelocity, ForceMode.Impulse);
            
            if (Physics.Raycast(transform.position, other.transform.position - transform.position, out hit))
            {
                float angle = Mathf.Atan2(hit.normal.x, hit.normal.z) * Mathf.Rad2Deg + 180;

                // var effectIdx = Random.Range(0, BloodFX.Length);
                // if (effectIdx == BloodFX.Length) effectIdx = 0;

                var instance = Instantiate(bloodEffect, hit.point, Quaternion.Euler(0, angle + 90, 0));
                // effectIdx++;
                // activeBloods++;
                var settings = instance.GetComponent<BFX_BloodSettings>();
                settings.LightIntensityMultiplier = DirLight.intensity;
                Destroy(instance, 31);

                // var nearestBone = GetNearestObject(transform.root, hit.point);
                // if(nearestBone != null)
                // {
                //         var attachBloodInstance = Instantiate(BloodAttach);
                //         var bloodT = attachBloodInstance.transform;
                //         bloodT.position = hit.point;
                //         bloodT.localRotation = Quaternion.identity;
                //         bloodT.localScale = Vector3.one * Random.Range(0.75f, 1.2f);
                //         bloodT.LookAt(hit.point + hit.normal, direction);
                //         bloodT.Rotate(90, 0, 0);
                //         bloodT.transform.parent = nearestBone;
                //         //Destroy(attachBloodInstance, 20);
                // }

                // Play animation if available
                if (anim != null)
                {
                    if (health > 1)
                    {
                        if (!isHit)
                        {
                            isHit = true;
                            stunned = true;
                            anim.SetTrigger("Hit");
                            health -= 1;
                        }
                    }
                    else
                    {
                        if(!isHit){
                            Instantiate(ragdoll, transform.position, Quaternion.identity);
                            Destroy(this.gameObject);
                        }
                    }
                }
            }
        }
    }

    void Update(){
        attackTimer += Time.deltaTime;
        healthBar.value = health;

        if(target!=null){
            switch (enemyState)
            {
                case State.Advancing:
                    if(target!=null){
                        anim.SetFloat("Forward", 10);
                        RotateTowardsTarget();
                        MoveTowardsTarget();
                    }

                    if(Vector3.Distance(target.transform.position, transform.position) < range){
                        enemyState = State.Attacking;
                    }
                    break;
                case State.Attacking:
                    RotateTowardsTarget();
                    anim.SetFloat("Forward", 0);
                    if(attackTimer>attackDelay){
                        if(!stunned){
                            anim.SetTrigger("LeftJab");
                        }
                        attackTimer = 0;
                    }
                    if(Vector3.Distance(target.transform.position, transform.position) > range){
                        enemyState = State.Advancing;
                    }
                    break;
                default:
                    break;
            }
        }
    }


    void FixedUpdate(){
        //rb.velocity = velocity;
        //ragdoll.transform.position = transform.position;

        hitCount+=Time.deltaTime;
        stunCount += Time.deltaTime;

        if(hitCount>hitTimer){
            hitCount=0;
            isHit = false;
        }

        if(stunCount>stunTimer){
            stunCount = 0;
            stunned = false;
        }
    }

    void RotateTowardsTarget()
    {
        // Get the direction to the target
        Vector3 direction = (target.position - transform.position).normalized;

        // Project the direction onto the XZ plane (horizontal plane)
        direction.y = 0;

        // Calculate the rotation needed to look at the target
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        // Smoothly rotate only around the y-axis towards the target
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, lookRotation.eulerAngles.y, 0), rotationSpeed * Time.deltaTime);
    }

    void MoveTowardsTarget()
    {
        // Get the direction to move towards the target
        Vector3 moveDirection = (target.position - transform.position).normalized;

        // Move towards the target
        rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);
        //Debug.Log("moveDirection*moveSpeed: " +(moveDirection * moveSpeed* Time.deltaTime).ToString());
    }
}
