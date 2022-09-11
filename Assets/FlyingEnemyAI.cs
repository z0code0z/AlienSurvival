using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class FlyingEnemyAI : MonoBehaviour
{

    public GameObject PlayerHitParticle;

    public Transform Player;
    Seeker seeker;

    Path path;
    public Animator AnimController;

    public Rigidbody rb;
    public float OrbitDistance = 8f;

    public float Speed = 5f;
    public float AttackSpeed = 25f;

    bool Pathing;


    public float NextWaypointDistance = 2.5f;
    int CurrentWaypointIndex;
    bool ReachedEndOfPath;
    Vector3 CurrentPointOfInterest;

    public float MaxLineOfSight = 20f;
    public LayerMask WhatBlocksOurVision;

    public float RunTime = 22.5f;
    bool running = false;

    bool TaggedPlayer = false;
    Vector3 RunDirection;

    Vector3 PositionToAttack;
    Vector3 OriginalAttackDirection;

    public int Health = 3;
    bool Dead = false;
    public GameObject EnemyDeathParticle;


    public GameObject WeaponSystem;

    private void OnCollisionEnter(Collision collision)
    {

        if (Dead)
        {

            if (collision.transform.tag == "Obstacle")
            {

                //POP
                Instantiate(EnemyDeathParticle, transform.position, Quaternion.identity);
                Destroy(gameObject);

            }
        }

        if (!running && !Dead)
        {
            if (collision.transform.tag == "Player" || collision.transform.tag == "Obstacle")
            {
                //we hit the player, do damage to them!!!!!
                //after hitting the player
                RunDirection = GetRunDirection();
                StartCoroutine(RunTimer());
                AnimController.SetTrigger("FlyFast");
                running = true;
                TaggedPlayer = false;
                Debug.Log("WE hit something");

                if (collision.transform.tag == "Player")
                {
                    Debug.Log("WE HIT PLAYER");
                    Instantiate(PlayerHitParticle, transform.position, Quaternion.identity);
                    Player.GetComponent<PlayerController>().GotHit();
                }


            }
        }

    }

    public void GotHit()
    {

        if (Health > 0)
        {
            Health--;
            WeaponSystem.GetComponent<WeaponSystem>().FireRateIncrease();
        }        
        
        if (Health == 0)
        {
            //we dead
            StopAllCoroutines();
            seeker.CancelCurrentPathRequest();
            AnimController.SetTrigger("Die");
            Dead = true;
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        seeker = GetComponent<Seeker>();

        //randomizer
        OrbitDistance += Random.Range(-1f, 4f);
        RunTime += Random.Range(-1f, 2f);
        MaxLineOfSight += Random.Range(-4f, 4f);
        Speed += Random.Range(-5f, 5f);
        AttackSpeed += Random.Range(-5f, 10f);



        //Spawn Sequence
        CurrentPointOfInterest = GetPointNearPlayer();
        seeker.StartPath(transform.position, CurrentPointOfInterest, OnPathComplete);


    }

    private void FixedUpdate()
    {
        if (Dead)
        {
            rb.AddForce(-Vector3.up * AttackSpeed, ForceMode.Acceleration);
            return;
        }

        if (!running)
        {
            RaycastHit hit;

            if (Physics.Raycast(transform.position, (Player.transform.position - transform.position).normalized, out hit, MaxLineOfSight, WhatBlocksOurVision))
            {
                if (hit.transform.tag == "Player")
                {
                    //we have line of sight
                    if (!TaggedPlayer)
                    {
                        TaggedPlayer = true;
                        AnimController.SetTrigger("Bite");
                        PositionToAttack = hit.point;
                        OriginalAttackDirection = (hit.point - transform.position).normalized;
                    }

                    if (Pathing)
                    {
                        StopAllCoroutines();
                        Pathing = false;
                    }
                    
                    

                    //attack code
                    Vector3 MoveDirection = (hit.point - transform.position).normalized;
                    transform.forward = Vector3.Slerp(transform.forward, MoveDirection, Time.fixedDeltaTime * Mathf.Clamp(rb.velocity.magnitude, 0.25f, 6f));

                    rb.AddForce(transform.forward * AttackSpeed, ForceMode.Acceleration);

                    
                    if(Vector3.Magnitude(hit.point - transform.position) < 0.75f)
                    {
                        Debug.Log("WE misssss");

                        //we missed
                        RunDirection = GetRunDirection();
                        StartCoroutine(RunTimer());
                        AnimController.SetTrigger("FlyFast");
                        running = true;
                        TaggedPlayer = false;
                    }

                }
                else
                {
                    //what we tagged wasnt the player move on
                    TaggedPlayer = false;
                }


            }
            else
            {

                TaggedPlayer = false;

            }
            //we don't have line of sight do normal path stuff!


            if (TaggedPlayer == false)
            {
                /*
                if(path == null)
                {
                    CurrentPointOfInterest = GetPointNearPlayer();
                    seeker.StartPath(transform.position, CurrentPointOfInterest, OnPathComplete);
                }
                */

                if (path != null)
                {

                    //begin path updating
                    if (!Pathing)
                    {
                        Pathing = true;
                        StartCoroutine(PathUpdater());
                        AnimController.SetTrigger("FlyFast");
                    }

                    ReachedEndOfPath = false;
                    //we have a path, continue
                    float DistanceToWaypoint;

                    while (Pathing)
                    {
                        DistanceToWaypoint = Vector3.Magnitude(path.vectorPath[CurrentWaypointIndex] - transform.position);
                        if (DistanceToWaypoint < NextWaypointDistance)
                        {

                            if (CurrentWaypointIndex + 1 < path.vectorPath.Count)
                            {
                                CurrentWaypointIndex++;
                                //we go onto next waypoint now
                            }
                            else
                            {
                                ReachedEndOfPath = true;
                                //this is where we need to do the other code for orbiting etc/shooting fighting attacking
                                break;
                            }

                        }
                        else
                        {

                            Vector3 MoveDirection = (path.vectorPath[CurrentWaypointIndex] - transform.position).normalized;


                            rb.AddForce(MoveDirection * Speed);

                            transform.forward = Vector3.Slerp(transform.forward, MoveDirection, Time.fixedDeltaTime * 2f);

                            break;
                        }


                    }



                }
            }

        }
        else
        {
            //WE ARE RUNNING FROM PLAYER

            

            rb.AddForce(RunDirection * AttackSpeed * 0.95f, ForceMode.Acceleration);

            transform.forward = Vector3.Slerp(transform.forward, RunDirection, Time.fixedDeltaTime * 10f);



        }


    }


    IEnumerator PathUpdater()
    {
        while (Pathing)
        {
            CurrentPointOfInterest = GetPointNearPlayer();
            seeker.StartPath(transform.position, CurrentPointOfInterest, OnPathComplete);
            yield return new WaitForSecondsRealtime(0.5f);

        }
        yield return null;
    }


    public void OnPathComplete(Path p)
    {

        if (!p.error)
        {
            path = p;
            CurrentWaypointIndex = 0;
        }

    }

    IEnumerator RunTimer()
    {
        yield return new WaitForSecondsRealtime(RunTime);
        //stop running
        running = false;

    }

    Vector3 GetPointNearPlayer()
    {

        Vector3 PointinSphere = Random.onUnitSphere;
        PointinSphere.y = Mathf.Abs(PointinSphere.y);
        Vector3 FinalPos = Player.position + (PointinSphere * OrbitDistance);
        return FinalPos;

    }

    Vector3 GetRunDirection()
    {
        /*
        Vector3 PlayerDirection = (Player.position - transform.position).normalized;
        Vector3 FinalPoint = ((2 * PlayerDirection) + Vector3.up + Vector3.up).normalized;
        */

        Vector3 FinalPoint = new Vector3(OriginalAttackDirection.x, -OriginalAttackDirection.y, OriginalAttackDirection.z).normalized;

        return FinalPoint;
    }

}