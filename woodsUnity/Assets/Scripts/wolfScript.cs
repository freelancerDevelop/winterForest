using UnityEngine;
using System.Collections;


/// <summary>
/// Wolf script inherits from Flocker and controls the behavior of wolves.
/// </summary>
public class wolfScript:Flocker {

    private bool isHerder; //does this wolf herd or hunt when deer are found?
    enum WolfState {TRACK, HUNT, EAT};
    WolfState state;

    /// <summary>
    /// the calcSteeringForces method used by wolves.
    /// </summary>
    protected override void calcSteeringForces()
    {
        //switch(state)
        //tracking behavior
        //wander, use normalized flow fields to try to find the clearings, but not as quickly as deer leave the woods
        //huntingbehavior
        //if(isherder)
        //herd
        //else
        //pursue(nearestdeer)
        //detect deer collisions
        //eating behavior
        //feed()
        //{
 	        //arrive(downDeer);
	        //time += deltaTime
	        //if(time >= feedtime)
                //downDeer == null;
	        //back to hunting
//}

        base.calcSteeringForces();
    }	

    private Vector3 herd()
    {
        
            //if(deer is too far away from its herd centroid)
            //{
	        //pursue(point one deer.safedistance away from the vector from the centroid to the escaping deer);
	        //avoid(deer, deer.safedistance); //so they don’t get spooked by us going after them

            //}

        


        return Vector3.zero;
    }
}
