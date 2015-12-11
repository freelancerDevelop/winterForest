using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Wolf script inherits from Flocker and controls the behavior of wolves.
/// </summary>
public class wolfScript:Flocker {

    public bool isHerder; //does this wolf herd or hunt when deer are found?
    public int id;
    public enum WolfState {TRACK, HUNT, EAT};
    public WolfState state = WolfState.TRACK;

	//hunting fields
	private Flock huntFlock;

	//eating fields
	private Vector3 downDeer;
	private float time;
    public float followWeight;
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
				//wander, make wandering fight a little with finding the deer

                //Debug.Log(flock.leader);
				if(flock.leader == this.id)
				{
					steeringForce+= wander()*wanderWeight;

                    //head towards the closest deer
                    GameObject[] deer = GameObject.FindGameObjectsWithTag("deer");
                    List<Flocker> deerList = new List<Flocker>();
                    foreach(GameObject d in deer)
                    {
                        deerList.Add(d.GetComponent<Flocker>());
                    }
                    int index = getNearest(deerList);
                    //Debug.Log(deerList.Count);
                    if (index != -1)
                    {
                        //Debug.Log("index not -1");
                        steeringForce += seek(deerList[index].transform.position) * seekWeight;
                    }
				}	
				//leader following
				else
				{
					//having them all follow the same leader should pretty much give us cohesion and alignment without trying
                   // Debug.Log("calling follow");
					steeringForce += followLeader(flock.Flockers[0],followDistance)*followWeight;
					steeringForce += separation(separateDistance)*separationWeight;
				}
                //everyone avoids obstacles
                foreach (GameObject obstacle in gm.Obstacles)
                {
                    steeringForce += avoid(obstacle) * avoidWeight;
                }
				foreach(Flock dflock in gm.Herds)
				{
					if((this.flock.Centroid - dflock.Centroid).sqrMagnitude < huntDistance * huntDistance)
					{
						huntFlock = dflock;
						state = WolfState.HUNT;
                        Debug.Log("change state to hunt");
					}
					
				}
				break;
			}
		case WolfState.HUNT:
			{
                if (isHerder) //if you're supposed to be containing, contain!
                {
                    //Debug.Log("herding");
                    steeringForce += herd();
                }
                else //otherwise, try to catch the nearest deer
                {
                    //Debug.Log("hunting");

                    int nearestIndex;
                    if (huntFlock != null)
                        nearestIndex = getNearest(huntFlock.Flockers);
                    else
                        nearestIndex = -1;
                    if (nearestIndex > -1)
                    {
                        if(huntFlock.Flockers[nearestIndex] != null)
                            steeringForce+=pursue(huntFlock.Flockers[nearestIndex]);

                    }
                    else
                        Debug.Log("hunt index -1");
                }
					//deer collision detection 
                if(huntFlock != null)
                    foreach(Flocker f in huntFlock.Flockers)
                    {
                      if(f!=null)
                        if(checkCollide(f))
                        {
                            
                            foreach(Flocker w in flock.Flockers)
                                w.GetComponent<wolfScript>().state = WolfState.EAT;
                            huntFlock = null;
                            Debug.Log("change state to eat");
                            downDeer = f.transform.position;
                            //the deer shouldn't be in a flock anymore, so don't need to clean that up..
                            //we dynamically grabbed the master list of deer so that's not an issue...
                            //so theoretically it should be safe to just throw it out now.
                            f.kill();
                            //possibly instantiate a static dead deer here if there's time, but for now it's ok for them to just disappear
                            //start timer
                            time = 0.0f;
                            break;
                        }
                    }
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
            if(deer != null && (deer.transform.position - huntFlock.Centroid).sqrMagnitude > herdDistance*herdDistance)
            {
                Vector3 seekPoint = deer.transform.position + (deer.transform.position - huntFlock.Centroid)*herdBuffer;
	            return seek(seekPoint);
            }
        }
        

        //if we get here, then nobody's trying to leave, strangely..
        return Vector3.zero;
    }


}
