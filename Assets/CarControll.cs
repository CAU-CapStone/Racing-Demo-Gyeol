using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class CarControll : Agent
{
    public GameObject mainCamera;
    private GameObject car;
    private Rigidbody rb;

    public List<GameObject> checkPoints;
    private GameObject nowCheckPoint;
    private int checkPointIndex = 0;
    
    public float speed = 0.1f;

    public Material green;
    
    // Start is called before the first frame update
    void Start()
    {
        car = gameObject;
        rb = car.GetComponent<Rigidbody>();
        mainCamera.transform.position = car.transform.position - new Vector3(0, 0.5f, 0);

        nowCheckPoint = checkPoints[0];
        nowCheckPoint.tag = "CHECKPOINT";
        nowCheckPoint.transform.GetChild(0).gameObject.tag = "CHECKPOINT";
        nowCheckPoint.layer = 7;
        nowCheckPoint.transform.GetChild(0).gameObject.layer = 7;
        
        StartCoroutine(CheckPointTriggerCorutine());
    }

    // Update is called once per frame
    void Update()
    {
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        AddReward(-1f);
        
        float accelerator = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float handle = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        
        rb.AddTorque(car.transform.up * handle * 30);
        rb.AddForce(car.transform.forward * accelerator * 300);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(car.transform.position.x);
        sensor.AddObservation(car.transform.position.z);
        
        sensor.AddObservation(rb.velocity.x);
        sensor.AddObservation(rb.velocity.z);
        
        sensor.AddObservation(rb.rotation.y);
        sensor.AddObservation(rb.angularVelocity.y);
        
        sensor.AddObservation(nowCheckPoint.transform.position.x - car.transform.position.x);
        sensor.AddObservation(nowCheckPoint.transform.position.z - car.transform.position.z);
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;
        
        if (Input.GetKey(KeyCode.W))
        {
            actions[0] = -1f;
            
            //rb.AddForce(-car.transform.forward * speed);
            //car.transform.position += speed * Time.deltaTime * -car.transform.forward;
        }

        if (Input.GetKey(KeyCode.S))
        {
            actions[0] = 1f;
            
            //rb.AddForce(car.transform.forward * speed);
            //car.transform.position -= speed * Time.deltaTime * -car.transform.forward;
        }

        if (Input.GetKey(KeyCode.A))
        {
            actions[1] = -1f;

            //rb.AddTorque(-car.transform.up * 3);
            //car.transform.rotation *= Quaternion.Euler(0, -Time.deltaTime * 90, 0);
        }

        if (Input.GetKey(KeyCode.D))
        {
            actions[1] = 1f;

            //rb.AddTorque(car.transform.up * 3);
            //car.transform.rotation *= Quaternion.Euler(0, Time.deltaTime * 90, 0);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("WALL"))
        {
            AddReward(-100f);

            StartCoroutine(WallCollisionCorutine());
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("CHECKPOINT"))
        {
            AddReward(100f);
            if (checkPointIndex < checkPoints.Count - 1)
            {
                Debug.Log($"Checkpoint {checkPointIndex}");
                
                nowCheckPoint.GetComponent<MeshRenderer>().material = green;
                nowCheckPoint.tag = "Untagged";
                nowCheckPoint.transform.GetChild(0).gameObject.tag = "Untagged";
                nowCheckPoint.layer = 0;
                nowCheckPoint.transform.GetChild(0).gameObject.layer = 0;
                
                checkPointIndex++;
                nowCheckPoint = checkPoints[checkPointIndex];
                nowCheckPoint.tag = "CHECKPOINT";
                nowCheckPoint.transform.GetChild(0).gameObject.tag = "CHECKPOINT";
                nowCheckPoint.layer = 7;
                nowCheckPoint.transform.GetChild(0).gameObject.layer = 7;

                StartCoroutine(CheckPointTriggerCorutine());
            }
            else
            {
                EndEpisode();
                SceneManager.LoadScene("SampleScene");
            }
        }
    }

    private IEnumerator WallCollisionCorutine()
    {
        for (float i = 0; i < 30; i += 0.1f)
        {
            yield return new WaitForSeconds(0.1f);
            if (rb.velocity.magnitude > 0.1f)
                yield break;
        }
        
        Debug.Log("Collision Timeout");
        EndEpisode();
        SceneManager.LoadScene("SampleScene");
    }

    private IEnumerator CheckPointTriggerCorutine()
    {
        int tempIndex = checkPointIndex;
        
        yield return new WaitForSeconds(100f);

        if (tempIndex == checkPointIndex)
        {
            Debug.Log("No Trigger Timeout");
            EndEpisode();
            SceneManager.LoadScene("SampleScene");
        }
    }
}
