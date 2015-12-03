using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Wolf script inherits from Flocker and controls the behavior of wolves.
/// </summary>
public class wolfScript:Flocker {

    public bool isHerder; //does this wolf herd or hunt when deer are found?
    enum WolfState {TRACK, HUNT, EAT};
    WolfState state;

	//hunting fields
	private Flock huntFlock;

	//eating fields
	private Vector3 downDeer;
	private float time;
	public float feedtime; //the amount of time to spend eating, inspector field
    public float followDistance;
    public float herdDistance; //how far to let deer get away from their centroid
    public float herdBuffer; //how far away to stay from the deer when herding
    /// <summary>
    /// the calcSteeringForces method used by wolves.
    /// </summary>
    protected override void calcSteeringForces()
    {
        switch (state) {
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
					steeringForce += followLeader(flock.leader,followDistance);
					steeringForce += separation(separateDistance)*separationWeight;
				}
				break;
			}
		case WolfState.HUNT:
			{
				if(isHerder) //if you're supposed to be containing, contain!
					steeringForce += herd();
				else //otherwise, try to catch the nearest deer
					pursue(huntFlock.Flockers[getNearest(huntFlock.Flockers)]);
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
				time += Time.deltaTime;
				if(time >= feedtime)
				{	
					downDeer = Vector3.zero;
					state = WolfState.HUNT;
				}
                break;
			}
		}
        base.calcSteeringForces();
    }	

    private Vector3 herd()
    {
        foreach(Flocker deer in huntFlock.Flockers)
        {
            //if deer is straying too far from the center of the flock..
            if((deer.transform.position - huntFlock.Centroid).sqrMagnitude > herdDistance*herdDistance)
            {
                Vector3 seekPoint = deer.transform.position + (deer.transform.position - huntFlock.Centroid)*herdBuffer;
	            return seek(seekPoint);
            }
        }
        

        //if we get here, then nobody's trying to leave, strangely..
        return Vector3.zero;
    }

}
