using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class PacmanAgent : Agent
{
    public float rayDistance = 1f;
    public GameObject initialBoard, curBoard;
    public int pelletsInScene = 0, curTimer = 6000, originalTimer = 6000; //1 min

    //new vars added
    public int initialNumberPeletsWanted;
    public int spawnRadius;
    public GameObject originalPelet;
    public int limitToStopInfiniteLoop = 10000;

    public int stage = 1;
    public GameObject stage4PeletPositions;
    public Vector3 stage4PacmanSpawnPosition;
    Transform[] pelets;


    public override void OnEpisodeBegin()
    {
        Time.timeScale = 1;
        Destroy(curBoard);
        curBoard = Instantiate(initialBoard, transform.parent);
        Transform peletGroup = curBoard.transform.GetChild(1).transform;
        if (stage == 1)
        {
            transform.localPosition = getPacmanRandomPos();
            stage1(peletGroup);
        }

        else if (stage == 2 || stage == 3)
        {
            transform.localPosition = getPacmanRandomPos();
            stage2or3(peletGroup);
        }

        else if (stage == 4)
        {
            transform.localPosition = stage4PacmanSpawnPosition;
            stage4();
        }

        pelletsInScene = curBoard.transform.GetChild(1).transform.childCount;
        curTimer = originalTimer;

    }
    public Vector3 getPacmanRandomPos()
    {
        Vector3 possibleSpawnPosition;
        int limiter = 0; //to stop any infinte loops
        do
        {
            float xRand = Random.Range(-2.5f, 5.5f);
            float possibleSpawnX = ((int)xRand) + .5f;
            float zRand = Random.Range(-8.5f, 8.5f);
            float possibleSpawnZ = ((int)zRand) + .5f;
            possibleSpawnPosition = new Vector3(possibleSpawnX, .5f, possibleSpawnZ);
            limiter++;
        } while (limiter < limitToStopInfiniteLoop && (Physics.CheckSphere(possibleSpawnPosition, .3f)));

        if (limiter < limitToStopInfiniteLoop)
        {
            return possibleSpawnPosition;
        }
        else
        {
            return stage4PacmanSpawnPosition; //if the limiter is reached, than pacman will just spawn in the middle
        }
    }
    public void stage1(Transform peletGroup)
    {
        //first 2 pellets spawn bw 1 and 2 blocks from agent
        randomPelletSpawning(peletGroup, 2, 2);

        //next 3 pellets spawn bw 3 and 5 blocks from agent
        randomPelletSpawning(peletGroup, 5, 5);
    }
    public void stage4()
    {
        GameObject.Destroy(curBoard.transform.GetChild(1).gameObject);
        GameObject.Instantiate(stage4PeletPositions, curBoard.transform, false);
    }
    public void stage2or3(Transform peletGroup)
    {
        //dependent on whatever the initial vars passed in are
        randomPelletSpawning(peletGroup, spawnRadius, initialNumberPeletsWanted);
    }
    public void randomPelletSpawning(Transform peletGroup, int spawnR, int initialPeletsWanted)
    {
        float curBoardHeightMin = curBoard.transform.position.x - 2.5f;
        float curBoardHeightMax = curBoard.transform.position.x + 5.5f;
        float curBoardWidthMin = curBoard.transform.position.z - 8.5f;
        float curBoardWidthMax = curBoard.transform.position.z + 8.5f;

        pelletsInScene = peletGroup.childCount; //=0 most of the time, =2 when running second loop for stage 1
        while (pelletsInScene < initialPeletsWanted)
        {
            float pacmanSpawnX = transform.position.x;
            float pacmanSpawnZ = transform.position.z;
            Vector3 possibleSpawnPosition;
            bool inBoard;
            int limiter = 0; //to stop any infinite loops
            do
            {
                float xRand = Random.Range(-spawnR, spawnR);
                float possibleSpawnX = ((int)xRand) + pacmanSpawnX;
                float zRand = Random.Range(-spawnR, spawnR);
                float possibleSpawnZ = ((int)zRand) + pacmanSpawnZ;
                possibleSpawnPosition = new Vector3(possibleSpawnX, originalPelet.transform.position.y, possibleSpawnZ);
                inBoard = true;

                if (possibleSpawnPosition == transform.position || possibleSpawnX < curBoardHeightMin || possibleSpawnX > curBoardHeightMax || possibleSpawnZ < curBoardWidthMin || possibleSpawnZ > curBoardWidthMax)
                {
                    inBoard = false;
                }
                limiter++;
            } while (limiter < limitToStopInfiniteLoop && (!inBoard || Physics.CheckSphere(possibleSpawnPosition, .3f)));      //checkSphere returns true if something is in the postion that it is given
            //dont make the radius of the above line more than .5f, if u do, it will cause an infinite loop and crash
            if (limiter < limitToStopInfiniteLoop)
                Instantiate(originalPelet, possibleSpawnPosition, originalPelet.transform.rotation, peletGroup);
            pelletsInScene++;  //this will make the outside loop not have an infinite loop. The outside loop will run exactly initialNumberPeletsWanted of times even though it might not generate all the initialNumberPeletsWanted
        }
    }

    public override void Heuristic(float[] actionsOut)
    {
        if (Input.GetKey(KeyCode.RightArrow))
            actionsOut[0] = 2;
        else if (Input.GetKey(KeyCode.LeftArrow))
            actionsOut[0] = 3;
        else if (Input.GetKey(KeyCode.UpArrow))
            actionsOut[0] = 0;
        else if (Input.GetKey(KeyCode.DownArrow))
            actionsOut[0] = 1;
        else
            actionsOut[0] = -1;//dont move if I dont press any key
        //the actionsOut array gets passed to OnActionRecive() by mlagents
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition); //location of pacman

        Transform peletGroup = curBoard.transform.GetChild(1).transform;
        pelets = new Transform[peletGroup.childCount];
        for (int i = 0; i < pelets.Length; i++)
        {
            pelets[i] = peletGroup.GetChild(i);
        }
        //Debug.Log("Pacman Positi: " + transform.localPosition);
        //Debug.Log("Closset Pelet: "+ (pelets.Length != 0 ? getClosestPellet(pelets).localPosition : Vector3.zero));
        sensor.AddObservation(pelets.Length!=0 ? getClosestPellet(pelets).localPosition : Vector3.zero);
        
    }
    public Transform getClosestPellet(Transform[] pelets)
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        foreach (Transform potentialTarget in pelets)
        {
            Vector3 directionToTarget = potentialTarget.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = potentialTarget;
            }
        }
        return bestTarget;
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        base.OnActionReceived(actions);
        int movement = actions.DiscreteActions[0];
        Debug.Log("MOve: " + movement);
        if (movement == 0) //+x
        {
            if (emmitRayCast(new Vector3(1, 0, 0)) != "wall")
            {
                transform.position += new Vector3(1, 0, 0);
            }
        }
        else if (movement == 1) //-x
        {
            if (emmitRayCast(new Vector3(-1, 0, 0)) != "wall")
            {
                transform.position += new Vector3(-1, 0, 0);
            }
        }
        else if (movement == 2) //-z
        {
            if (emmitRayCast(new Vector3(0, 0, -1)) != "wall")
            {
                transform.position += new Vector3(0, 0, -1);
            }
        }
        else if (movement == 3) //+z
        {
            if (emmitRayCast(new Vector3(0, 0, 1)) != "wall")
            {
                transform.position += new Vector3(0, 0, 1);
            }
        }
        curTimer--;
        if (curTimer <= 0)
        {
            SetReward(-initialNumberPeletsWanted); //harsher reward makes it learn faster, makes it so that agent must collect as many pellets as possible to mitigate the punishment
            EndEpisode();
        }
        checkWin();

    }
   
    public void checkWin()
    {

        if (curBoard.transform.GetChild(1).transform.childCount == 0)
        {
            Debug.Log("Win");
            SetReward(5f);

            EndEpisode();
        }

    }
    public string emmitRayCast(Vector3 direction)
    {
        Debug.DrawRay(transform.position, direction * rayDistance, Color.red, 1);
        Ray ray = new Ray(transform.position, direction);
        RaycastHit myHitdata;

        if (Physics.Raycast(ray, out myHitdata, rayDistance))
        {
            switch (myHitdata.collider.tag)
            {
                case "wall":
                    SetReward(-.3f); //incentivizes it to not run into walls
                    return "wall";
                    break;
                case "pelet":
                    GameObject.Destroy(myHitdata.collider.gameObject);
                    SetReward(1f);
                    return "pelet";
                    break;
                default:
                    return "space";
                    break;
            }
        }
        return "nothing";
    }
}
/* Curriculum Learning
 * Stage 1:
 *  -pacman spawns in random pos
 *  -pellets spawns in random pos
 *      -first 2 would be 1 or 2 blocks away
 *      -next 3 would be 3 to 5 blocks away
 *  -# of pellets spawn = 5
 *  -curiosity = .15
 *  -
 * 
 * Stage 2:
 *  -pacman spawns in random pos
 *  -pellets spawn in random pos, rad = 6
 *  -# of pellets spawn = 12
 *  -curiosity = .15
 *  
 * Stage 3:
 *  -pacman spawns in random pos
 *  -pellets spawn in random pos, rad = 15
 *  -# of pellets = 25
 *  -curiosity = .10
 *  
 * Stage 4:
 *  -pacman spawns in random pos
 *  -pellets spawn in random pos in whole board
 *  -# of pellets = max, use the pellet prefab
 *  -curiosity = .05
 */
