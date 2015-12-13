using UnityEngine;
using System.Collections;

public class testScript : Flocker {


    protected override void calcSteeringForces()
    {
        steeringForce += wander();
        base.calcSteeringForces();
    }
	
}
