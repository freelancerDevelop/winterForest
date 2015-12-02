using UnityEngine;
using System.Collections;

public class testScript : Flocker {

    public bool isLeader;
    public Vehicle leader;
    public float followDistance;
    public GameObject seektarget;

    protected override void calcSteeringForces()
    {
        steeringForce = Vector3.zero;
        if (isLeader)
        {
            steeringForce += wander();
        }
        else
            steeringForce += followLeader(leader, followDistance);
        steeringForce += stayInBounds(25, new Vector3(25, 0, 25));
        steeringForce += separation(separateDistance) * separationWeight;
        base.calcSteeringForces();
    }
}
