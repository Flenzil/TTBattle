using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraMovement : MonoBehaviour
{
    private float cameraSpeed = 8.0f;
    private float rotateSpeed = 150f;
    private float zoomSpeed = 120f;
    [SerializeField] private GameObject gamePlane;
    Vector3 rotateOrigin;
    // Start is called before the first frame update
    void Start()
    {
        rotateOrigin = transform.position + Vector3.forward * 5f;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 projectedBack = Vector3.ProjectOnPlane(-transform.forward, Vector3.up).normalized;
        

        if (Input.GetKey(KeyCode.W)){
            transform.Translate(projectedForward * Time.deltaTime * cameraSpeed, Space.World);
            // rotateOrigin = Vector3.ProjectOnPlane(transform.position + Vector3.forward * 5f, Vector3.up);
       } 
        if (Input.GetKey(KeyCode.S)){
            transform.Translate(projectedBack * Time.deltaTime * cameraSpeed, Space.World);
            // rotateOrigin = Vector3.ProjectOnPlane(transform.position + Vector3.forward * 5f, Vector3.up);
       }
        if (Input.GetKey(KeyCode.A)){
            transform.Translate(Vector3.left * Time.deltaTime * cameraSpeed);
            // rotateOrigin = Vector3.ProjectOnPlane(transform.position + Vector3.forward * 5f, Vector3.up);
       } 
        if (Input.GetKey(KeyCode.D)){
            transform.Translate(Vector3.right * Time.deltaTime * cameraSpeed);
            // rotateOrigin = Vector3.ProjectOnPlane(transform.position + Vector3.forward * 5f, Vector3.up);
       } 
        if (Input.GetKey(KeyCode.Q)){
            transform.Rotate(Vector3.up * Time.deltaTime * rotateSpeed, Space.World);
            //transform.RotateAround(rotateOrigin, Vector3.up, rotateSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.E)){
            transform.Rotate(-Vector3.up * Time.deltaTime * rotateSpeed, Space.World);
            //transform.RotateAround(rotateOrigin, Vector3.up, -rotateSpeed * Time.deltaTime);
        }
        
        if (Input.GetAxis("Mouse ScrollWheel") < 0){
            transform.Translate(-Vector3.forward * Time.deltaTime * zoomSpeed);
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0){
            transform.Translate(Vector3.forward * Time.deltaTime * zoomSpeed);
        }
    }
}
