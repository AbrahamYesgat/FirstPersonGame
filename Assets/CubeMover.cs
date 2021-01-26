using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeMover : MonoBehaviour
{
    bool toMove = false;
    Vector3 pos;
    float speed = 0.09f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if (toMove) {
            transform.position = Vector3.Lerp(transform.position, pos, speed);
            GameObject closestMouse = findClosestGameObjectWithTag(transform.position, "Mouse");
            if (Vector3.Distance(closestMouse.transform.position, this.transform.position) < 3.0f) {
                Debug.Log("MOUSE DEAD");
                Destroy(closestMouse, 0);
            }

            if (Vector3.Distance(transform.position, Camera.main.transform.position) < 3f && !PathGenerator.shieldIsActive)
            {
                //reduce number of lives
                Destroy(this.gameObject, 1);
                HTN.state[1]--;
                toMove = false;

            }

        }
        if (transform.position == pos) toMove = false;

    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.tag);

    }

    public void setToMove(Vector3 pos) {
        this.pos = pos;
        toMove = true;
    }
    //taken from my Assig3
    public static GameObject findClosestGameObjectWithTag(Vector3 point, string nameTag)
    {
        float min = Mathf.Infinity;
        GameObject nearestNeighbor = null;

        GameObject[] allGameObjectsWithspecificTag = GameObject.FindGameObjectsWithTag(nameTag);
        Vector3 position = point;
        for (int i = 0; i < allGameObjectsWithspecificTag.Length; i++)
        {
            GameObject contender = allGameObjectsWithspecificTag[i];
            float thisDistance = Vector3.Distance(contender.transform.position, position);
            if (thisDistance < min && contender.transform.position != position)
            {
                //update nearest point in this situation
                nearestNeighbor = contender;
                min = thisDistance;
            }
        }
        //returns null if no game object found
        return nearestNeighbor;
    }


}
