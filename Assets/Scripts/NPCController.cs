using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NPCController : MonoBehaviour
{
    public PlayerController playerController;
    //Simple FSM (FiniteStateMachine)
    public enum NPCState {
        Idle,
        Patrolling,
        Chasing,
        Attacking
    };

    public Transform patrolPath;
    public float CloseEnoughDistance = 4;
    //To be able to simulate different states

    public NPCState currentState = NPCState.Idle;
    public bool bCanSeePlayer;
    [SerializeField]
    GameObject goPlayer;

    //New as of Sep.20th
    GameObject[] Waypoints;
    int CurrentWaypointIndex = 0;
    public float speed = 0.2f; //1 m/s
    //--------------------------
    //New as of Sep.22th
    public float angularSpeedDegPerSec = 60; //    Deg/sec
    float angularSpeedRadPerSec; // Rad/

    public float chase = 16;


    //float closenessToWaypoint=.1; //<.1 m => we are at waypoint
    void ChangeState(NPCState newState)
    {
        currentState = newState;
    }
    // Start is called before the first frame update
    float Deg2Rad(float deg)
    {
        //return deg * Mathf.Deg2Rad;
        return deg/180f * Mathf.PI ;

    }
    float Rad2Deg(float rad)
    {
        //return rad * Mathf.Rad2Deg;
        return rad/Mathf.PI* 180f;

    }
    void Start()
    {
        goPlayer = GameObject.FindGameObjectWithTag("Player");
        //First method: use tag Waypoint
        Waypoints = GameObject.FindGameObjectsWithTag("Waypoint");

        //Mathf.Deg2Rad
        //Mathf.Rad2Deg

        // 360 Deg= 2* PI Rad
        // => 1 Rad = 180/PI Deg
        // Also => 1 Deg = PI/180 Rad
        angularSpeedRadPerSec = Deg2Rad(angularSpeedDegPerSec);
        //Second method: get Waypoints' parent and then get all children (Waypoints)
    }

    // Update is called once per frame
    void Update()
    {
        angularSpeedRadPerSec = Deg2Rad(angularSpeedDegPerSec);
        HandleFSM();
        
    }
    void HandleFSM()
    {
        switch (currentState)
        {
            case NPCState.Idle:
                HandleIdleState();
                break;
            case NPCState.Patrolling:
                HandlePatrollingState();
                break;
            case NPCState.Chasing:
                HandleChasingState();
                break;
            case NPCState.Attacking:
                HandleAttackingState();
                break;
            
            default:
                break;
        }
    }

    private void HandleIdleState()
    {
        //Debug.Log("In NPCController.HandleIdleState");
        bCanSeePlayer = false;
        //transform to the patrolling path
        if(this.transform.position != patrolPath.position)
        {
            Vector3 movement = MyMoveTowards(this.transform.position, patrolPath.position, speed * Time.deltaTime);
            this.transform.position = movement;
        }
        else
        {
            transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }
        if (PlayerController.onTheFloor == true)
        {
            ChangeState(NPCState.Patrolling);
        }
    }


    private void HandleAttackingState()
    {
        //Debug.Log("In NPCController.HandleAttackingState");
        //if player dies => patrol
        //else if d(this, target) > attack_dist =>
        //       if see target => chase
        //       else patrol


        //bool playerAlive = goPlayer.GetComponent<PlayerController>().IsAlive;
        if(PlayerController.onTheFloor == false)
        {
            bCanSeePlayer = false;
            ChangeState(NPCState.Idle);
            
        }
        
        else if(PlayerController.onTheFloor == true)
        {
            speed = 8;
            //Debug.Log(speed);
            this.transform.position = MyMoveTowards(this.transform.position, goPlayer.transform.position, speed * Time.deltaTime);
            
        }



    }

    private void HandleChasingState()
    {
        //Debug.Log("In NPCController.HandleChasingState");


        if (!bCanSeePlayer)
        {
            ChangeState(NPCState.Patrolling);
        }

        else if (Vector3.Distance(this.transform.position, goPlayer.transform.position) <= this.CloseEnoughDistance)
        {
            ChangeState(NPCState.Attacking);
        }
        else
        {
            speed = 4;
            this.transform.position = MyMoveTowards(this.transform.position, goPlayer.transform.position, speed * Time.deltaTime);
        }
        if (PlayerController.onTheFloor == false)
        {
            ChangeState(NPCState.Idle);
        }

    }

    private void HandlePatrollingState()
    {
        //Debug.Log("In NPCController.HandlePatrollingState");
        speed = 4;
        if (bCanSeePlayer)
        {
            Vector3 playerPos = goPlayer.transform.position;
            float dist = Vector3.Distance(this.transform.position, playerPos);
            //Debug.Log(dist + "distance");
            if (dist < chase)
            {
                ChangeState(NPCState.Chasing);
            }
            //ChangeState(NPCState.Chasing);

        }
        if(PlayerController.onTheFloor == false)
        {
            ChangeState(NPCState.Idle);
        }

        FollowPatrolingPath();
    }

    private void FollowPatrolingPath()
    {
        
        Vector3 target = Waypoints[CurrentWaypointIndex].transform.position;
        if (Vector3.Distance(this.transform.position, target) < .1)
        {
            CurrentWaypointIndex = CalculateNextWaypointIndex();
            target=Waypoints[CurrentWaypointIndex].transform.position;
        }
        // speed m/s
        // d=v*dt  [m/s]*[s] => [m]
        //Vector3 movement = Vector3.MoveTowards(this.transform.position, target, speed * Time.deltaTime);
        Vector3 movement = MyMoveTowards(this.transform.position, target, speed * Time.deltaTime);
        this.transform.position = movement;
        
    }

    Vector3 MyMoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)
    {
        Vector3 C2T = target - current;

        
        Quaternion qtargetrotation =Quaternion.LookRotation(C2T);
        //this.transform.rotation = qtargetrotation;  //Abrupt2: this is a too abrupt rotation; not very beleivable; to try, uncomment this an comment out the line below
        
        // This is the smothest rotation; more beleivable
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, qtargetrotation, Time.deltaTime*angularSpeedRadPerSec);
        //-------- End of Changes related to Rotation -------------

        Vector3 movement = current + C2T.normalized * maxDistanceDelta;
        return movement;

    }

    private int CalculateNextWaypointIndex()
    {
        //Strategy 1 - follow in order
        return CurrentWaypointIndex=(CurrentWaypointIndex+1) % Waypoints.Length;



 
        //throw new NotImplementedException();
    }

    private void CanSeeAdvesary()
    {
        //throw new NotImplementedException();

        //GameObject goPlayer = GameObject.FindGameObjectWithTag("Player");
        //
        Vector3 playerPos = goPlayer.transform.position;
        Vector3 E2P_Heading = playerPos - this.transform.position;
        float cosAngleE2P = Vector3.Dot(this.transform.forward, E2P_Heading) / E2P_Heading.magnitude;
        //float cosAngleE2P = Vector3.Dot(this.transform.forward, E2P_Heading) ; //we need only the sign of cosAngle, so no need to devide by a >0 size (a small optimization)
        bCanSeePlayer = (cosAngleE2P > 0); //we are assuming FoV=180 degrees; if 
        //Debug.Log(cosAngleE2P);

        float angle = Vector3.Angle(this.transform.forward, E2P_Heading);
        // Debug.Log("angle=" + angle);
        
    }

    private void FixedUpdate()
    {
        CanSeeAdvesary();
    }

    private void OnDrawGizmos()
    {   //Path
        Gizmos.color = Color.green;
        if (Waypoints != null && Waypoints.Length > 0) {
            for (int i = 0; i < Waypoints.Length; i++)
            {
                Vector3 from = Waypoints[i].transform.position;
                Vector3 to = Waypoints[(i + 1) % Waypoints.Length].transform.position;
                Gizmos.DrawLine(from, to);
            }
        }

    }

   

}
