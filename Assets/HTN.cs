using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class HTN : MonoBehaviour
{
    List<HTNNode> plan, savedPlan;
    public static float[] state, savedState;
    HTNNode savedM, savedT;
    NavMeshAgent agent;
    string COMPOUND_TYPE = "COMPOUND_TYPE";
    string PRIMITIVE_TYPE = "PRIMITIVE_TYPE";
    float testingTimer = 0;
    bool runPlan = true;
    bool navigated = true;
    GameObject toThrow = null;

    bool zeroDone = false;
    bool oneDone = false;
    bool twoDone = false;
    // Start is called before the first frame update
    void Start()
    {
        plan = new List<HTNNode>();
        //

        savedM = new HTNNode();
        savedT = new HTNNode();
        savedState = new float[5];
        state = new float[5];
        state[0] = GameObject.FindGameObjectsWithTag("ObstacleRock").Length;
        state[1] = 2; //number of lives;

        state[2] = GameObject.FindGameObjectsWithTag("ObstacleCrate").Length;

        state[3] = PathGenerator.shieldIsActive? 1 : 0;
        state[4] = 1; ;
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        GameObject[] rocks = GameObject.FindGameObjectsWithTag("ObstacleRock");
        GameObject[] crates = GameObject.FindGameObjectsWithTag("ObstacleCrate");
        List<GameObject> obstacles = new List<GameObject>();
        obstacles.AddRange(rocks);
        obstacles.AddRange(crates);
        var random = new System.Random();
        GameObject m = obstacles[random.Next(obstacles.Count)];
        toThrow = m;

        


    }

    // Update is called once per frame
    float timer = 0;
    float wanderTimer = 0;
    bool newPlan = true;
    bool toMove = true;
    void Update()
    {
        state[3] = PathGenerator.shieldIsActive ? 1 : 0;
        state[0] = GameObject.FindGameObjectsWithTag("ObstacleRock").Length;
        state[2] = GameObject.FindGameObjectsWithTag("ObstacleCrate").Length;

        if (newPlan && Camera.main.transform.position.x > 5) {
            plan.Clear();
            plan = HTNPlanner();

            GameObject[] rocks = GameObject.FindGameObjectsWithTag("ObstacleRock");
            GameObject[] crates = GameObject.FindGameObjectsWithTag("ObstacleCrate");
            List<GameObject> obstacles = new List<GameObject>();
            obstacles.AddRange(rocks);
            obstacles.AddRange(crates);
            var random = new System.Random();
            GameObject m = obstacles[random.Next(obstacles.Count)];
            if (plan.Count > 2)
            {
                state[4] = 0;
                if (plan[1].getName().Equals("throwRock")) toThrow = rocks[random.Next(rocks.Length)];
                else
                {
                    toThrow = crates[random.Next(crates.Length)];
                }
                agent.SetDestination(toThrow.transform.position);

            }
            else {
                //wander
                state[4] = 1;
                agent.SetDestination(new Vector3(Random.Range(10, 180), 1, Random.Range(10, 115)));
            }
        }
        newPlan = false;
        if (plan.Count > 1)
        {
            if (timer < 10.0f) {
                timer += Time.deltaTime;
            } else{
                toMove = true;
                timer = 0.0f;
                if (toMove && toThrow!= null &&toThrow.tag.Contains("Rock"))
                {
                    throwRock(toThrow, Camera.main.transform.position);
                    newPlan = true;
                } 
                else if(toMove && toThrow!=null &&toThrow.tag.Contains("Crate")) {

                    kickCrate(toThrow, Camera.main.transform.position);
                    if (toThrow != null) Destroy(toThrow, 1);
                    newPlan = true;
                }
                toMove = false;
                
            }
        } else {

            if (wanderTimer < 5) {
                wanderTimer += Time.deltaTime;
            }
            else
            {
                wanderTimer = 0;
                newPlan = true;
            }
        }


        
    }

    //class notes
    List<HTNNode> HTNPlanner() {
        runPlan = false;
        HTNNode wait = new HTNNode(PRIMITIVE_TYPE, nameof(wait));
        HTNNode navToObstacle = new HTNNode(PRIMITIVE_TYPE, nameof(navToObstacle));
        HTNNode kickCrate = new HTNNode(PRIMITIVE_TYPE,nameof(kickCrate));
        HTNNode throwRock = new HTNNode(PRIMITIVE_TYPE, nameof(throwRock));
        HTNNode moveAround = new HTNNode(PRIMITIVE_TYPE, nameof(moveAround));

        HTNNode attack = new HTNNode(COMPOUND_TYPE, nameof(attack));
        HTNNode perform = new HTNNode(COMPOUND_TYPE, nameof(perform));
        HTNNode idle = new HTNNode(COMPOUND_TYPE, nameof(idle));
        HTNNode beAMonster = new HTNNode(COMPOUND_TYPE, nameof(beAMonster));


        //set children 
        List<HTNNode> childrenOfPerform = new List<HTNNode>();
        childrenOfPerform.Add(kickCrate);
        childrenOfPerform.Add(throwRock);
        perform.setChildren(childrenOfPerform);

        List<HTNNode> childrenOfIdle = new List<HTNNode>();
        childrenOfIdle.Add(moveAround);
        idle.setChildren(new List<HTNNode>(childrenOfIdle));

        //set subtasks
        List<HTNNode> subtasksOfNavToObstacle = new List<HTNNode>();
        subtasksOfNavToObstacle.Add(perform);
        subtasksOfNavToObstacle.Add(wait);
        navToObstacle.setSubtasks(subtasksOfNavToObstacle);

        //set children:
        List<HTNNode> childrenOfAttack = new List<HTNNode>();
        childrenOfAttack.Add(navToObstacle);
        attack.setChildren(childrenOfAttack);

        List<HTNNode> childrenOfBeAMonster = new List<HTNNode>();
        childrenOfBeAMonster.Add(idle);
        childrenOfBeAMonster.Add(attack);
        beAMonster.setChildren(childrenOfBeAMonster);

        Stack<string> tasks = new Stack<string>();
        Stack<HTNNode> tasksHTNNodes = new Stack<HTNNode>();

        tasksHTNNodes.Push(beAMonster);


        //from class lecture
        while (tasksHTNNodes.Count > 0)
        {
            HTNNode t = tasksHTNNodes.Pop();
            if (t.getType().Equals(COMPOUND_TYPE))
            {
                var random = new System.Random();
                if ( t.getChildren().Count > 0)
                {
                    HTNNode m = t.getChildren()[random.Next(t.getChildren().Count)];
                    saveState(t, plan, m, state);
                    if (m.getSubtasks() != null) {
                        for (int i = m.getSubtasks().Count-1 ; i >= 0 ; i--) {
                            tasksHTNNodes.Push(m.getSubtasks()[i]);
                        }

                    }
                    tasksHTNNodes.Push(m);

                }
                else
                {
                    restoreSavedState(t,plan, state);
                }
            }
            else if (t.getType().Equals(PRIMITIVE_TYPE))
            {
                if (preconditions(t, state))
                {
                    apply(t.getName(),state);//update state based on post conditions
                    plan.Add(t);
                }
                else
                {
                    restoreSavedState(t,plan, state);

                }
            }




        }

        for (int i = 0; i < plan.Count; i++)
        {
            Debug.Log(i + ": " + plan[i].getName());
        }
        return plan;
        //Debug.Log(plan.Count);

        //Debug.Log(plan.ToArray().ToString());
    }

    void saveState(HTNNode t, List<HTNNode> plan, HTNNode m, float[] state) {
        savedState = state;
        savedT = t;
        savedM = m;
        savedPlan = plan;

    }


    void apply(string t, float[] state) {
        if (t.Equals("throwRock"))
        {
            state[0]--; //decrease the number of rocks
            if (state[3] != 1.0f)
            { //if shield is not active 
                //check intersection on player and obstacle,
                //decrease number of Lives happens in the RockMover class
            }

        }
        else if (t.Equals("kickCrate"))
        {
            //post condition decrease number of crates
            if (state[3] != 1.0f)
            { //if shield is not active 
                //check intersection on player and obstacle,
                state[2]--; //decrease number of crates
               //decrease number of Lives happens in the RockMover class

            }

        }
        else if (t.Equals("navToObstacle"))
        {
            state[4] = 0;

        }
        else if (t.Equals("wait"))
        {
            //toggle attack mode
            state[4] = 1;
            //wait
        }
        else if (t.Equals("moveAround")) {
            //toggle attack mode
            state[4] = 1;
        };

    }

    void wander() {
        Vector3 randPosition = new Vector3(UnityEngine.Random.Range(5, 190), 1, UnityEngine.Random.Range(5, 125));

        agent.SetDestination(new Vector3(randPosition.x, transform.position.y, randPosition.z));

    }

    void restoreSavedState(HTNNode t, List<HTNNode> plan, float[] state) {
        state = savedState;
        t = savedT;
        plan = savedPlan;
        
    }


    void navToObstacle(Vector3 obst) {
        agent.SetDestination(obst);
    }

    void kickCrate(GameObject m, Vector3 finalpos) {
       Vector3 pos = Camera.main.transform.position;
       CubeMover cb = m.GetComponent<CubeMover>();
       cb.setToMove(finalpos);
     }

    void throwRock(GameObject m,Vector3 finalpos)
    {
         Vector3 pos = Camera.main.transform.position;
         RockMover cb = m.GetComponent<RockMover>();
         cb.setToMove(pos);
    }

    void wait() {
        while (true) {
            testingTimer += Time.deltaTime;
            if (testingTimer >1) {
                testingTimer = 0;
                break;
            }
        }
        return;

    }


    bool preconditions(HTNNode t, float []state) {
        GameObject a = findClosestGameObjectWithTag(transform.position, "ObstacleRock");
        GameObject b = findClosestGameObjectWithTag(transform.position, "ObstacleCrate");
        if (state[1] == 0) return false;//no more lives player dead
        if (t.getName().Equals("throwRock"))
        {
            if (b != null && state[0] > 0&&//means we still have rocks remaining
                Vector3.Distance(transform.position, b.transform.position) < Vector3.Distance(transform.position, a.transform.position))
            {
                return true;
            }

        }
        else if (t.getName().Equals("kickCrate"))
        {

            if (a != null && //means we still have crates remaining
                Vector3.Distance(transform.position, b.transform.position) > Vector3.Distance(transform.position, a.transform.position))
            {
                return true;
            }


        }
        else if (t.getName().Equals("navToObstacle"))
        {
            return true;
        }
        else if (t.getName().Equals("wait")) {
            return true;
        }

        return true;

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