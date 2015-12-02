using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Wolf script inherits from Flocker and controls the behavior of wolves.
/// </summary>
public class wolfScript:Flocker {

    private bool isHerder; //does this wolf herd or hunt when deer are found?
    enum WolfState {TRACK, HUNT, EAT};
    WolfState state;

	//hunting fields
	private Flock huntFlock;

	//eating fields
	private Vector3 downDeer;
	private float time;
	public float feedtime; //the amount of time to spend eating, inspector field
    /// <summary>
    /// the calcSteeringForces method used by wolves.
    /// </summary>
    protected override void calcSteeringForces()
    {
        /*switch (state) {
		case WolfState.TRACK:
			{
				//only the leader needs to steer
				//tracking behavior
				//wander, use normalized flow fields to try to find the clearings, but not as quickly as deer leave the woods
				if(flock.leader == this)
				{
					steeringForce+= wander()*wanderWeight;
					steeringForce+= flowFollow ().normalized;
				}	
				//leader following
				else
				{
					steeringForce += followLeader (flock.leader);
					steeringForce += separation(separateDistance)*separationWeight;
				}
				break;
			}
		case WolfState.HUNT:
			{
				if(isHerder) //if you're supposed to be containing, contain!
					steeringForce += herd();
				else //otherwise, try to catch the nearest deer
					pursue(getNearest(huntFlock.Flockers));
				//detect deer collisions
					//if(collidingwithdeer)
					//{//state = WolfState.EAT;
					//downDeer = deer we collided with
						//start timer
						time = 0.0f;

					//}
				break;
			}
		case WolfState.EAT:
			{

				//eating behavior
				//if more than certain distance away..? possibly later
				steeringForce += arrive(downDeer);
				time += Time.deltaTime();
				if(time >= feedtime)
				{	
					downDeer = null;
					state = WolfState.HUNT;
				}	
				
			}
		}	*/
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
