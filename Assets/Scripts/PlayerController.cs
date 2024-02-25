using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    //W => Forward, AD -> rotate Left/Right
    public bool IsAlive = true;

    public float playerSpeed = 5f;
    private float playerAngle = 100f;

    Vector3 startingPoint = new Vector3(-5, 1, -16);


    public static bool onTheFloor = false;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    { 
        float horizMovement = Input.GetAxis("Horizontal") * playerAngle * Time.deltaTime;
        this.transform.Rotate(Vector3.up * horizMovement);

        float vertiMovement = Input.GetAxis("Vertical") * playerSpeed * Time.deltaTime;
        Vector3 movement = transform.forward * vertiMovement;
        this.transform.position += movement;
    }
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.name == "Floor")
        {
            //Debug.Log("HERE");
            onTheFloor = true;
        }
        if(other.gameObject.name == "Enemy")
        {
            
            this.transform.position = startingPoint;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.name == "Floor")
        {
            //Debug.Log("Out");
            onTheFloor = false;
        }
    }
}