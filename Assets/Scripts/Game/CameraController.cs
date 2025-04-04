using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;
    
    float movementSpeed = 12f;
    float mouseDragSpeed = 30f;

    Transform targetpos;
    float zpos;
    public float camspeed; 

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    private void Start()
    {
        zpos = transform.position.z;
     //   lockcam = false;
    }

    public void setcampos(Transform pos)
    {
       
       
       targetpos= pos;
       
        Debug.Log("camset");
    }
    public void EndTutorial()
    {
        SceneManager.LoadScene("Main");
    }
    public bool lockcam = false;
    void Update()
    {
        if ((TutorialSystem.TutorialActive==true || InteractiveTutorial.TutorialActive==true) && lockcam)
        {
            Vector3 temppos=targetpos.position;
            temppos.z = zpos;
            Vector3 pos= Vector2.MoveTowards(transform.position, temppos, camspeed*Time.deltaTime);
            pos.z = zpos;
            transform.position = pos;
            Camera.main.orthographicSize = 7f;
        }
        else
        {


            Vector3 direction = CalculateDirection();
            transform.Translate(direction * movementSpeed * Time.deltaTime);

            if (Input.GetMouseButton(0))
            {
                if (transform.position.x >= -5f && transform.position.x <= 27f && transform.position.y <= 13 && transform.position.y >= -17f)
                {
                    float speed = mouseDragSpeed * Time.deltaTime;
                    transform.position -= new Vector3(Input.GetAxis("Mouse X") * speed, Input.GetAxis("Mouse Y") * speed, 0);
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (transform.position.x < -5f)
                {
                    transform.position = new Vector3(-5f, transform.position.y, transform.position.z);
                }
                else if (transform.position.x > 27f)
                {
                    transform.position = new Vector3(27f, transform.position.y, transform.position.z);
                }
                else if (transform.position.y < -17f)
                {
                    transform.position = new Vector3(transform.position.x, -16.9f, transform.position.z);
                }
                else if (transform.position.y > 13f)
                {
                    transform.position = new Vector3(transform.position.x, 13f, transform.position.z);
                }




          
            }

            //------------------Code for Zooming Out--------------------
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                if (Camera.main.fieldOfView <= 30f)
                {
                    Camera.main.fieldOfView += 2;
                }

                if (Camera.main.orthographicSize <= 16)
                {
                    Camera.main.orthographicSize += 0.5f;
                }
            }

            //----------------Code for Zooming In-----------------------
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                if (Camera.main.fieldOfView > 20f)
                {
                    Camera.main.fieldOfView -= 2;
                }

                if (Camera.main.orthographicSize >= 4)
                {
                    Camera.main.orthographicSize -= 0.5f;
                }
            }
        }
    }

    public Vector3 CalculateDirection()
    {
        Vector3 direction = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            direction.y += 1.0f;         
        }

        if (Input.GetKey(KeyCode.A))
        {
            direction.x -= 1.0f;            
        }

        if (Input.GetKey(KeyCode.S))
        {
            direction.y -= 1.0f;          
        }

        if (Input.GetKey(KeyCode.D))
        {
            direction.x += 1.0f;          
        }       

        return direction.normalized;
    }
}
