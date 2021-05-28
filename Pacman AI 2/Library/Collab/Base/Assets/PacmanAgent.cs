using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
public class PacmanAgent : Agent
{
    public float rayDistance = 1f;
    public GameObject initialBoard, curBoard, pelletGroup;
    public int pelletsInScene = 0, curTimer = 6000, originalTimer = 6000; //1 min
    public Vector3 pacmanStartPos;

    public override void OnEpisodeBegin()
    {
        //Destroy(curBoard);
        curBoard = Instantiate(initialBoard);

        pelletsInScene = pelletGroup.transform.GetChildCount();

        curTimer = originalTimer;
        Debug.Log("On Episode Start Pellets: " + pelletsInScene);

        transform.position = pacmanStartPos;        
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
    }

    public override void OnActionReceived(float[] vectorAction)
    {

        Debug.Log("On Action Recived");
        //Debug.Log("Actions: "+vectorAction.ToString());
        //Debug.Log("len: " + vectorAction.Length);
        /*
        for(int i = 0; i < vectorAction.Length; i++)
        {
            Debug.Log(vectorAction[i]);
        }
        */
        if (vectorAction[0] == 0) //+x
        {
            if (emmitRayCast(new Vector3(1, 0, 0)) != "wall")
            {
                transform.position += new Vector3(1, 0, 0);
                checkWin();
            }
        }
        else if (vectorAction[0] == 1) //-x
        {
            if (emmitRayCast(new Vector3(-1, 0, 0)) != "wall")
            {
                transform.position += new Vector3(-1, 0, 0);
                checkWin();
            }
        }
        else if (vectorAction[0] == 2) //-z
        {
            if (emmitRayCast(new Vector3(0, 0, -1)) != "wall")
            {
                transform.position += new Vector3(0, 0, -1);
                checkWin();
            }
        }
        else if (vectorAction[0] == 3) //+z
        {
            if (emmitRayCast(new Vector3(0, 0, 1)) != "wall")
            {
                transform.position += new Vector3(0, 0, 1);
                checkWin();
            }
        }
        curTimer--;
        if(curTimer <= 0)
        {
            SetReward(-5f);
            Destroy(curBoard);

            EndEpisode();
        }
        //check to make sure it's not in one spot
        SetReward(-.01f);
    }

    public void checkWin()
    {
        if (GameObject.FindGameObjectsWithTag("pelet").Length == 0)
            Debug.Log("Win");
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
                    return "wall";
                    break;
                case "pelet":
                    GameObject.Destroy(myHitdata.collider.gameObject);
                    SetReward(1f);
                    pelletsInScene--;
                    Debug.Log("Emmit Ray Cast: " + pelletsInScene);
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
