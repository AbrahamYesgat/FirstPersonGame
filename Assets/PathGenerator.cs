using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathGenerator : MonoBehaviour
{
    //large portion of this class, and possibly other classes were reused from my assignment 1 and other previous assignments
    public int startingPlatFormCounts;
    public int nextPlatformDirection;
    public Random a = new Random();
    public Transform currentPlatform = null;
    public Transform starter = null;
    public Transform mouse;
    List<Vector3> platforms = new List<Vector3>();
    public GameObject crate;

    public Transform player = null;
    public Text countText;
    float numberOfLives = 2;

    public GameObject platFormPrefab = null;
    public Vector3 endPosition;
    bool alive;
    bool gameWon = false;
    Vector3 finalPosition = new Vector3();
    bool treasureAcquired = false;
    float shieldTimeLeft = 10.0f;
    public static bool shieldIsActive = false;
    float testingTimer = 1.0f;
    public PathGenerator()
    {
    }
    // Start is called before the first frame update
    void Start()
    {
        this.alive = true;
        startingPlatFormCounts = 40;
        generateStartingPlatforms();
        countText.text = "Count: ";
        sprinkleSomeObstacles(10);
        randomizeMice(20);

    }

    void randomizeMice(int numberOfmice) {
        int count = 0;
        while (numberOfmice > 0) {
            Vector3 randPosition = new Vector3(Random.Range(5, 190), 0.5f, Random.Range(5, 125));
            float a = findDistanceToClosestGameObjectWithTag(randPosition, "ObstacleRock");
            float b = findDistanceToClosestGameObjectWithTag(randPosition, "ObstacleRock");
            float c = findDistanceToClosestGameObjectWithTag(randPosition, "Mouse");

            if (a > 10 && b > 10 && c >10)
            {
                Instantiate(mouse, randPosition, Quaternion.identity);
                numberOfmice--;

            }
            else
            {

                continue;
            }
        } 



    }
    private void sprinkleSomeObstacles(int numOfObstacles) {
        do {
            Vector3 randPosition = new Vector3(Random.Range(5, 190), 1, Random.Range(5, 125));
            float a = findDistanceToClosestGameObjectWithTag(randPosition, "ObstacleRock");
            float b = findDistanceToClosestGameObjectWithTag(randPosition, "ObstacleRock");
            if (a > 10 && b >10)
            {
                int cubeOrRock = Random.Range(0, 2);
                if (cubeOrRock == 1) Instantiate(platFormPrefab, randPosition, Quaternion.identity);
                else
                {
                    Instantiate(crate, randPosition, Quaternion.identity);
                }
                numOfObstacles--;

            }
            else {

                continue;
            }
        }while(numOfObstacles > 0);


    }

    //taken from my Assig3
    public float findDistanceToClosestGameObjectWithTag(Vector3 point, string nameTag)
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
        if (nearestNeighbor == null) return Mathf.Infinity;
        return Vector3.Distance(transform.position , nearestNeighbor.transform.position);
    }




    private void Update()
    {
        if (GameObject.FindGameObjectWithTag("Treasure") == null) treasureAcquired = true;
        SetCountText();
        numberOfLives = HTN.state[1];
        alive = numberOfLives > 0;
        if (treasureAcquired && Camera.main.transform.position.x < 0)
        {
            finalPosition = Camera.main.transform.position;
            gameWon = true;
        }
        if (Input.GetKeyDown(KeyCode.Space)) {
            shieldIsActive = !shieldIsActive;
        }
        if (shieldIsActive && shieldTimeLeft > 0) {

            testingTimer += Time.deltaTime;
            if (testingTimer >= 1.0f)
            {
                shieldTimeLeft--;
                testingTimer = 0.0f;
            }
        }

    }

    void updateShield() {


    }

    void SetCountText()
    {
        if (gameWon)
        {
            countText.text = "You Win!";
            Camera.main.transform.position = finalPosition;

        }
        else if (alive) countText.text = "Shield Time Left: " + shieldTimeLeft.ToString()+"\n Lives: "+ numberOfLives;
        else if(HTN.state[1] < 1)
        {
            countText.text = "Game Over";
            Camera.main.transform.position = finalPosition;
            GameObject[] x = GameObject.FindGameObjectsWithTag("Mouse");

            for (int i = 0; i < x.Length; i++) {

                Destroy(x[i], 0);
            }
        };
    }






    // generateStartingPlatforms is called once per frame //used from my A1
    void generateStartingPlatforms()
    {
        platforms.Add(currentPlatform.position);
        Vector3 tentativePlatformPosition = currentPlatform.position;
        for (int i = 0; i < startingPlatFormCounts-1 && tentativePlatformPosition.z > 1; i++)
        {
            while (currentPlatform.position.x < 99)
            {
                nextPlatformDirection = Random.Range(0, 3);
                if (nextPlatformDirection == 0) tentativePlatformPosition = currentPlatform.position + Vector3.right * 3;
                //else if (nextPlatformDirection == 1) tentativePlatformPosition = currentPlatform.position + Vector3.left * 3;
                else tentativePlatformPosition = currentPlatform.position + Vector3.forward * 3;

                if (tentativePlatformPosition.x < 0 || tentativePlatformPosition.x > 190 || tentativePlatformPosition.z < 5 || tentativePlatformPosition.z >55 || tentativePlatformPosition.z > 90) continue;


                if (!platforms.Contains(tentativePlatformPosition))
                {
                    //add previous left and rights
                    if (nextPlatformDirection != 0) platforms.Add(currentPlatform.position + Vector3.right * 3);
                    if (nextPlatformDirection != 1) platforms.Add(currentPlatform.position + Vector3.left * 3);
                    if (nextPlatformDirection != 2) platforms.Add(currentPlatform.position - Vector3.forward * 3);

                    int cubeOrRock = Random.Range(0, 2);

                    if (cubeOrRock == 1) {
  
                        currentPlatform = Instantiate(platFormPrefab, tentativePlatformPosition, Quaternion.identity).transform;
                    } 
                    else
                    {

                        currentPlatform = Instantiate(crate, tentativePlatformPosition, Quaternion.identity).transform;
                    }
                    platforms.Add(currentPlatform.position);
                    endPosition = currentPlatform.position;
                    break;

                }
            }



        }

        //currentPlatform = Instantiate(starter, new Vector3(endPosition.x + 1, endPosition.y, endPosition.z + 1), Quaternion.identity).transform;


    }
}
