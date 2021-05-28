using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Extensions.Sensors;

public class CollectObservations : GridSensor
{

    protected override float[] GetObjectData(GameObject currentColliderGo, float type_index, float normalized_distance)
    {
        float[] channelValues = new float[ChannelDepth.Length]; // ChannelDepth.Length = 1 in this example
        channelValues[0] = type_index; //0, 1, 2

        if (channelValues[0] != 0 || channelValues[0] != 1)
        {
            channelValues[0] = 2;
        }
        Debug.Log("Value Override" + channelValues[0]);
        return channelValues;
    }
}
