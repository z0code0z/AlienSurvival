using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{

    Rigidbody rb;

    public float Gravity = 200;
    Vector3 GravityDirection = Vector3.down;
    Transform GravityTransform;


    float Horizontal;
    float Vertical;

    float Xvel;
    float Yvel;
    float Zvel;

    bool OnSlope = false;

    public float Speed;
    float FinalSpeed;
    public float JumpForce;

    bool Jumped;

    public GameObject GroundPoint;
    public LayerMask ExcludePlayer;

    bool FixedGrounded = false;

    public Volume PPV;
    public Vignette V;
    public float Health = 10;

    float PrevVValue = 0.25f;
    float NextVValue = 0.25f;

    bool dead = false;

    public GameObject WeaponSystem;

    public void GotHit()
    {
        if (Health > 0)
        {
            Health--;
            if (Health == 9)
            {


                NextVValue += 0.075f;
            }
            else if (Health > 0)
            {
                PrevVValue += 0.075f;
                NextVValue += 0.075f;
            }

        }

        if (Health == 0)
        {

            dead = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            Destroy(Camera.main);
            WeaponSystem.GetComponent<WeaponSystem>().Death();
            StopAllCoroutines();

        }
        else
        {
            
        }

    }

    void Start()
    {
        PPV.sharedProfile.TryGet<Vignette>(out V);
        V.intensity.value = 0.25f;
        FinalSpeed = Speed;
        GravityTransform = transform;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (dead)
        {

            
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);

            

            return;
        }

        V.intensity.value = PrevVValue + (Mathf.PingPong(Time.deltaTime * 12, 1) * (NextVValue - PrevVValue));



        Horizontal = Input.GetAxis("Horizontal") * 0.85f;
        Vertical = Input.GetAxis("Vertical");
        
        if (Input.GetButtonDown("Jump"))
        {
            if (!Jumped)
            {
                Jumped = true;
            }
        }

    }

    
    private void FixedUpdate()
    {

        if (dead)
        {
            return;
        }

        Vector3 Movement;

        FixedGrounded = isGrounded();

        if (FixedGrounded && Jumped)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(JumpForce * transform.up, ForceMode.Impulse);
            Jumped = false;
            FixedGrounded = false;
        }
        else if(!FixedGrounded && Jumped)
        {
            Jumped = false;
        }


        if (!FixedGrounded)
        {
            FinalSpeed = Speed / 6f;
        }
        else
        {
            FinalSpeed = Speed;
        }



        Movement = Vector3.ClampMagnitude(((transform.forward * Vertical) + (transform.right * Horizontal)), 1f);
        Vector3 FinalMove = Vector3.ProjectOnPlane(Movement, GravityDirection).normalized * Movement.magnitude * FinalSpeed;

        rb.AddForce(FinalMove, ForceMode.Force);


        rb.AddForce(GravityDirection * Gravity, ForceMode.Acceleration);

        Vector3 AdjustedVelocity;

        if (FixedGrounded)
        {

            //can just apply to everything since were not in the air... thats the purpose of this is to only apply to x and z when in air.. but otherwise why not apply everywhere?
            //tada new bug fixed

            //AdjustedVelocity = GravityTransform.InverseTransformDirection(rb.velocity);
            //AdjustedVelocity = new Vector3(AdjustedVelocity.x * 0.965f, AdjustedVelocity.y, AdjustedVelocity.z * 0.965f); //this has a bug inherent when moving up and down slopes -- need to multiply .y by the same factor but we already fixed by simply multiplying the whole thing
            //AdjustedVelocity = GravityTransform.TransformDirection(AdjustedVelocity);
            AdjustedVelocity = rb.velocity * 0.965f;
            rb.velocity = AdjustedVelocity;

        }
        else
        {
            //I believe this one is for mid air
            AdjustedVelocity = rb.velocity;
            AdjustedVelocity = new Vector3(AdjustedVelocity.x * 0.9905f, AdjustedVelocity.y, AdjustedVelocity.z * 0.9905f);

            rb.velocity = AdjustedVelocity;

        }



    }

    /// <summary>
    /// 
    /// Moving left/right is slower than forward (and prob back) which is good but what about diagonal? We have the same max speed achieved by holding w, but is it facing more towards the front or perfect 45 degree? the ideal would be more to front!
    /// 
    /// </summary>


    bool timerstarted;

    bool isGrounded()
    {
        RaycastHit hit;

        if (Physics.SphereCast(GroundPoint.transform.position, 0.4875f, -transform.up, out hit, 0.055f, ExcludePlayer, QueryTriggerInteraction.Ignore))
        {

            if(hit.normal != Vector3.up && hit.normal != Vector3.right)
            {
                //we can expand on this later if need be but this should be fine for now!
                OnSlope = true;
                GravityDirection = -hit.normal;
                GravityTransform = hit.transform;
            }
            else
            {
                OnSlope = false;
                GravityDirection = -Vector3.up;
                GravityTransform = transform;
            }

            return true;

        }

        //temporary data debugging!
        if (!timerstarted)
        {
            StartCoroutine(TimeJump());
        }

        OnSlope = false;
        GravityDirection = -Vector3.up;
        GravityTransform = transform;
        return false;

    }


    IEnumerator TimeJump()
    {
        timerstarted = true;
        float TimeInAir = 0;

        while (!isGrounded())
        {
            TimeInAir += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        //Debug.Log("We spent " + TimeInAir + " in the air!");
        timerstarted = false;

    }

    

}
