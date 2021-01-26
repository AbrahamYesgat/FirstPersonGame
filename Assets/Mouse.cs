using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mouse : MonoBehaviour
{
    float TimeStep;
    Vector3 velocity;
    float distanceToCenter = 1f;
    float radius = 1f;
    float angleOfRotation = Mathf.PI;
    float testingTimer = 0.0f;
    float repulsion = 20f;
    LineRenderer lineRenderer, lineRenderer2;
    List<GameObject> obstacles;
    Vector3 steering;
    // Start is called before the first frame update
    void Start()
    {
        TimeStep = TimeStep += Time.deltaTime * 0.09f;
        velocity = new Vector3(Random.RandomRange(-5, 5), 0 , Random.RandomRange(-5, 5));
        lineRenderer = new GameObject("angleLine").AddComponent<LineRenderer>();
        lineRenderer2 = new GameObject("wanderForce").AddComponent<LineRenderer>();
        GameObject[] rockObstacles = GameObject.FindGameObjectsWithTag("ObstacleRock");
        GameObject[] crateObstacles = GameObject.FindGameObjectsWithTag("ObstacleCrate");
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        GameObject monster = GameObject.FindGameObjectWithTag("Monster");
        GameObject player = GameObject.FindGameObjectWithTag("Player");


        obstacles = new List<GameObject>();
        obstacles.AddRange(rockObstacles);
        obstacles.AddRange(crateObstacles);
        obstacles.AddRange(walls);
        obstacles.Add(monster);
        obstacles.Add(player);



    }

    // Update is called once per frame
    //FOR the implementation of the steering behaviours with steering obstacle avoidance the following links were used, I did not come up with this solution. 
    //https://gamedevelopment.tutsplus.com/tutorials/understanding-steering-behaviors-seek--gamedev-849
    //https://gamedevelopment.tutsplus.com/tutorials/understanding-steering-behaviors-collision-avoidance--gamedev-7777
    //https://gamedevelopment.tutsplus.com/tutorials/understanding-steering-behaviors-wander--gamedev-1624
    void Update()
    {

        for (int i = 0; i < obstacles.Count; i++) {
            if (obstacles[i] == null) obstacles.Remove(obstacles[i]);
        }
        steering = wander();
        steering = wander() + avoid();
        //max out the steering to a threshold
        steering = Vector3.ClampMagnitude(steering, 30);
        //max out the velocity when adding the steering
        velocity = Vector3.ClampMagnitude(velocity + steering, 20);
        float speed = 0.005f;
        transform.position = Vector3.Lerp(transform.position, transform.position + velocity , speed);


        //To view the lines on which the mouse decides to wander please uncomment the following line!
        //myDraw();


        testingTimer = 0;

    }

     void myDraw()
    {

        lineRenderer.startWidth = 0.03f;
        lineRenderer.endWidth = 0.03f;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.SetPosition(1, new Vector3((transform.position + (velocity).normalized * 10f).x, 0.45f, (transform.position + (velocity).normalized * 10f).z));
        lineRenderer.SetPosition(0, new Vector3(transform.position.x, 0.45f, transform.position.z)); ;

        //Color c1 = Color.blue;
        //lineRenderer2.startColor = (c1);
        //lineRenderer2.endColor = (c1); ;
        //lineRenderer2.startWidth = 0.03f;
        //lineRenderer2.endWidth = 0.03f;
        //lineRenderer2.positionCount = 2;
        //lineRenderer2.useWorldSpace = true;
        //lineRenderer2.SetPosition(1, new Vector3(transform.position.x + steering.x, 0.45f, transform.position.z + steering.z));
        //lineRenderer2.SetPosition(0, new Vector3(transform.position.x, 0.45f, transform.position.z)); ;

    }


    //https://gamedevelopment.tutsplus.com/tutorials/understanding-steering-behaviors-wander--gamedev-1624
    Vector3 wander() {
        //the center of our circle is represented by the current velocity
        Vector3 circleCenter = new Vector3(velocity.x, 0, velocity.z) ;
        circleCenter = circleCenter.normalized * distanceToCenter;
        //if we normalize that direction, then extend it by the amount we want our distance to circle to reach, the tip of our arrow is ahead of us by distanceTocenter amount

        Vector3 swerveOffset = new Vector3(0,0,1);
        swerveOffset = swerveOffset.normalized * radius;
        //change the swerve offset based on the prev angle of rotation
        float len = Vector3.Magnitude(swerveOffset);
        swerveOffset.x = len* Mathf.Cos(angleOfRotation);
        swerveOffset.z = len*Mathf.Sin(angleOfRotation);

        //here we slightly modify the previous angle so that turning isnt too rough
        angleOfRotation += (Random.RandomRange(0f,1f) * (2 * Mathf.PI)) - ((2 * Mathf.PI) * .5f);
        return circleCenter + swerveOffset;
    }



 void OnCollisionEnter(Collision collision){
        if (collision.gameObject.tag == "Monster")
        {
            Destroy(this, 0);
        }


 }

 //https://gamedevelopment.tutsplus.com/tutorials/understanding-steering-behaviors-collision-avoidance--gamedev-7777
 Vector3 avoid(){
    Vector3 longArm = transform.position + (velocity).normalized * 10f;
    Vector3 shortArm = transform.position + (velocity).normalized * 10f * 0.5f;
    GameObject mostThreatening = biggestThreat(longArm, shortArm);
 
    if (mostThreatening == null) {
            return new Vector3(0, 0, 0);
            //no avoidance is required, simply wander, 0 contrib from here
    } else {
     Vector3 swerveAvoid = new Vector3(0, 0, 0);

     //if we should avoid then we set the avoidance force based on the difference of the long arm and the threat, if we shouldnt avoid then we set the avoidance to 0 contrib.
     swerveAvoid.x = longArm.x - mostThreatening.transform.position.x;
     swerveAvoid.z = longArm.z - mostThreatening.transform.position.z;

    swerveAvoid = swerveAvoid.normalized * repulsion;
    //return the contribution of avoidance and add it late to the wander
    return swerveAvoid;
    }
 }

 //https://gamedevelopment.tutsplus.com/tutorials/understanding-steering-behaviors-collision-avoidance--gamedev-7777
GameObject biggestThreat(Vector3 longArm, Vector3 shortArm)
{
    //only look at mice within a radius of 50
    List<GameObject> neiboringMice =  getNeighboringMice(50);
    GameObject max = null;
    neiboringMice.AddRange(obstacles);
    int count = 0;
    foreach (GameObject potentialDanger in neiboringMice) {
        //if we check ourselves, continue, otherwise this will by default be the closest
        if(potentialDanger == transform.gameObject) continue;
        Vector3 obstacleTransformPos = potentialDanger.GetComponent<Collider>().bounds.ClosestPoint(transform.position);
        if (obstacleTransformPos == null) continue;
        //get the exact point of collision on the bounds of the game object
        Vector3 mostThreatningPoint = max == null ?
                Vector3.positiveInfinity :
                max.GetComponent<Collider>().bounds.ClosestPoint(transform.position);
        count++;
        if (rayCollisionCircle(longArm, shortArm, potentialDanger)) {
          if ((Vector3.Distance(transform.position, obstacleTransformPos) < Vector3.Distance(transform.position, mostThreatningPoint) || count == 0))
           {
             //if we found a new closest danger, update
             max = potentialDanger;
             count++;
           }else {
              count++;
              continue;
           }
        }
      count++;
    }
    //Debug.Log(count);
    return max;
}

//get mice within dist
List<GameObject> getNeighboringMice(float dist) {
        List<GameObject> neighbors = new List<GameObject>();
        //get all mice in the scene
        GameObject[] allGameObjectsWithspecificTag = GameObject.FindGameObjectsWithTag("Mouse");
        for (int i = 0; i < allGameObjectsWithspecificTag.Length; i++)
        {
            GameObject contender = allGameObjectsWithspecificTag[i];
            float thisDistance = Vector3.Distance(contender.transform.position, transform.position);
            if (thisDistance < dist)
            {
                //if within dist, add
                if(contender !=null) neighbors.Add(contender);
            }
        }
        //returns list of mice neighbors within distance
        return neighbors;

}

Vector3 getCenterOfGroup(List<GameObject> group) {
        Vector3 center = new Vector3(0,0,0);
        for (int i = 0; i < group.Count; i++) {


            center += group[i].transform.position;
        }

        if (group.Count > 0) center /= group.Count;

        return center;

}
//https://gamedevelopment.tutsplus.com/tutorials/understanding-steering-behaviors-collision-avoidance--gamedev-777
bool rayCollisionCircle(Vector3 longArm, Vector3 shortArm, GameObject obstacle){
        //shoot a ray and if there is an intersection,
        //meaning that the ray is within the distance of 10 of the center of the object, then return danger flag.
        Vector3 obstacleTransformPos = obstacle.GetComponent<Collider>().bounds.ClosestPoint(transform.position);
        if (Vector3.Distance(obstacleTransformPos, longArm) <= 10) {
            return true;
        } else if (Vector3.Distance(obstacleTransformPos, shortArm) <= 10) {
            return true;
        }
        return false;
    }

}
