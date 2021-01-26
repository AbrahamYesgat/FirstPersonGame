using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseCol : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, GameObject.FindGameObjectWithTag("Monster").transform.position) < 6f) {
            Debug.Log("MOUSE DEAD");
            Destroy(transform.gameObject, 0);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.transform);
    }
}
