using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControll : MonoBehaviour
{
    public GameObject car;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = car.transform.position + car.transform.forward * 20f + car.transform.up * 5f;
        Vector3 dir = car.transform.position - transform.position;
        transform.rotation = Quaternion.LookRotation(dir);
    }
}
