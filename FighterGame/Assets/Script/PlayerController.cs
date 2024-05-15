using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public Camera cam;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed=5;
    [SerializeField] private float mouseSens=0.01f;
    [SerializeField] private float jumpHeight;
    public float groundCheckDistance = 0.1f; // Distance to check for ground
    public LayerMask groundMask;
    [SerializeField] private Animator anim;
    [SerializeField] private Transform mesh;

    private Vector3 moveVector;
    private Vector2 lookRot;

    public InputActionReference move;
    public InputActionReference look;
    public InputActionReference jab;
    public InputActionReference kick;
    public InputActionReference jump;
    public InputActionReference crouch;

    public GameObject leftFistCol;
    public GameObject rightFootCol;

    private bool crouching = false;

    public LayerMask triggerLayer;
    public float knockback;
    public GameObject bloodEffect;
    public Light DirLight;

    public bool isHit;
    private float hitCount;
    public float hitTimer = 1f;

    public bool stunned;
    private float stunCount;
    public float stunTimer = 1f;

    public float health;

    private Vector3 knockbackVel;

    public GameObject ragdoll;

    private void OnTriggerEnter(Collider other)
    {
        RaycastHit hit;
        // Check if the entering collider is on the specified layer
        if (((1 << other.gameObject.layer) & triggerLayer) != 0)
        {
            // Calculate the direction from the impact point to the trigger position
            Vector3 impactDirection = (transform.position - other.ClosestPoint(transform.position)).normalized;

            // Calculate the velocity change
            knockbackVel = impactDirection * knockback;

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
                            anim.SetTrigger("Hit");
                            stunned = true;
                            health -= 1;
                        }
                    }
                    else
                    {
                        Instantiate(ragdoll, transform.position, Quaternion.identity);
                        Destroy(this.gameObject);
                    }
                }
            }
        }
    }

    bool IsGrounded()
    {
        // Raycast downwards to check for ground
        return Physics.Raycast(transform.position, -Vector3.up, groundCheckDistance, groundMask);
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        leftFistCol.SetActive(false);

        // Lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        //transform.position = new Vector3(0, 0, 1);
    }

    void Update(){

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

        moveVector = move.action.ReadValue<Vector3>();
        lookRot = look.action.ReadValue<Vector2>();

        if (lookRot.x != 0 || lookRot.y != 0) // Check if there's any input from the controller
        {
            // Calculate the rotation direction based on the input
            float rotationDirection = Mathf.Sign(lookRot.x);

            // Apply rotation based on the direction
            transform.Rotate(Vector3.up, rotationDirection * rotationSpeed * Time.deltaTime);
        }
        else // If there's no input from the controller, use mouse input
        {
            RotateWithMouse();
        }


        if(crouch.action.ReadValue<float>()>0){
            if(!crouching){
                crouching = true;
                anim.SetBool("crouching", true);
            }
        }else{
            crouching =false;
            anim.SetBool("crouching", false);
        }

        anim.SetFloat("Forward", moveVector.z*2);
        anim.SetFloat("Left", moveVector.x*-2);
        
        // if(rb.velocity.z>1){
        //     anim.SetBool("forward", true);
        //     anim.SetBool("backward", false);
        // }else if(rb.velocity.z<-1){
        //     anim.SetBool("forward", false);
        //     anim.SetBool("backward", true);
        // }else{
        //     anim.SetBool("backward", false);
        //     anim.SetBool("forward", false);
        // }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        mesh.position = transform.position;

        // Calculate movement direction based on the player's forward direction
        Vector3 moveDirection = transform.forward * moveVector.z + transform.right * moveVector.x;
        //moveDirection.Normalize();

        // Apply movement velocity to the Rigidbody in local space
        if(stunned){
            moveDirection = Vector3.zero;
        }

        if(knockbackVel!= null){
            rb.velocity = new Vector3 (moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z*moveSpeed) + knockbackVel;
        }else{
            rb.velocity = new Vector3 (moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z*moveSpeed);
        }


        //rb.velocity = new Vector3(moveVector.x*moveSpeed*Time.deltaTime, rb.velocity.y, moveVector.z*moveSpeed*Time.deltaTime);

        if(IsGrounded()){
            anim.SetBool("falling",false);
        }else{
            anim.SetBool("falling",true);
        }
    }

    void RotateWithMouse()
    {
        // Check if the mouse position has changed
        if (Mouse.current.delta.IsActuated())
        {
            // Get the mouse X position using the new Input System
            float mouseX = Mouse.current.delta.x.ReadValue();

            // Rotate the GameObject based on mouse movement on the X-axis
            transform.Rotate(Vector3.up, mouseX * mouseSens * Time.deltaTime);
        }
    }

    private void OnEnable(){
        jab.action.performed += Jab;
        jump.action.performed += Jump;
        kick.action.performed += Kick;
    }
    private void OnDisable(){
        jab.action.performed -= Jab;
        jump.action.performed -= Jump;
        kick.action.performed -= Kick;
    }

    void Jab(InputAction.CallbackContext obj){
        // // Get the current state information
        // AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            
        // // Get the length of the current animation clip
        // float animationLength = stateInfo.length;
        if(!stunned){
            leftFistCol.SetActive(true);
            anim.SetTrigger("LeftJab");
            StartCoroutine(DeactivateAfterDelay(leftFistCol, 0.5f));
        }
    }

    void Kick(InputAction.CallbackContext obj){
        if(!stunned){
            anim.SetTrigger("Kick");
            rightFootCol.SetActive(true);
            StartCoroutine(DeactivateAfterDelay(rightFootCol, 1f));
        }
    }

    void Jump(InputAction.CallbackContext obj){
        if(IsGrounded() && !stunned){
            anim.SetTrigger("Jump");
            //rb.AddForce(Vector3.up*jumpHeight, ForceMode.Impulse);
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y+jumpHeight, rb.velocity.z);
        }
    }

    IEnumerator DeactivateAfterDelay(GameObject obj, float delay){
        yield return new WaitForSeconds(delay);
        if(obj.activeSelf){
            obj.SetActive(false);
        }
    }
}
