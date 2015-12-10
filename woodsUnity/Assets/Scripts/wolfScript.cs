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
	public float huntDistance; //how far away we have to be from a flock to start hunting them
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
					//having them all follow the same leader should pretty much give us cohesion and alignment without trying
					steeringForce += followLeader(flock.leader,followDistance);
					steeringForce += separation(separateDistance)*separationWeight;
				}

				foreach(Flock dflock in gm.Herds)
				{
					if((this.flock.Centroid - dflock.Centroid).sqrMagnitude > huntDistance * huntDistance)
					{
						huntFlock = dflock;
						state = WolfState.HUNT;
					}
					
				}
				break;
			}
		case WolfState.HUNT:
			{
				if(isHerder) //if you're supposed to be containing, contain!
					steeringForce += herd();
				else //otherwise, try to catch the nearest deer
                {
                    int nearestIndex = getNearest(huntFlock.Flockers);
                    if(nearestIndex > -1)
					    pursue(huntFlock.Flockers[nearestIndex]);
                }
					//deer collision detection is handled on onCollisionEnter
				break;
			}
		case WolfState.EAT:
			{

				//eating behavior
				steeringForce += arrive(downDeer);
				steeringForce += separation (separateDistance)*separationWeight;
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

	void OnCollisionEnter(Collision col)
	{
		if (col.gameObject.tag == "deer") {
			//if it's a deer we're hitting...
			state = WolfState.EAT;
			downDeer = col.gameObject.transform.position;
			//the deer shouldn't be in a flock anymore, so don't need to clean that up..
			//we dynamically grabbed the master list of deer so that's not an issue...
			//so theoretically it should be safe to just throw it out now.
			Destroy (col.gameObject);
			//possibly instantiate a static dead deer here if there's time, but for now it's ok for them to just disappear
			//start timer
			time = 0.0f;
		}
	}

}
