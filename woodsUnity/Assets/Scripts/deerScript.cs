using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Deer script inherits from Flocker and controls the behavior of deer.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class deerScript : Flocker {

    enum DeerState { GRAZE, FLEE, SEARCH}; 
    DeerState state = DeerState.GRAZE;

    //**********
    // Inspector Variables
    //**********
    public float regroupRadius;
    public float fleedistance;

    /// <summary>
    /// The particular calcSteeringForces used by
    /// deer.
    /// </summary>
    protected override void calcSteeringForces()
    {
	
        Vector3 temp = Vector3.zero; //for prioritizing
        steeringForce = Vector3.zero;
		switch(state)
        {
            case DeerState.GRAZE:
                {
                    steeringForce += wander() * wanderWeight;
                    foreach (GameObject obstacle in gm.Obstacles)
                    {
                        temp += avoid(obstacle) * avoidWeight;
                    }
                    temp += stayInBounds(gm.BOUNDS_RADIUS, gm.BOUNDS_CENTER) * boundsWeight;
                    steeringForce += temp;
                    if (temp == Vector3.zero)
                    {
                        temp += cohesion(flock.Centroid) * cohesionWeight; //only non-zero conditionally
                        if (temp == Vector3.zero)
                            temp += separation(separateDistance) * separationWeight; //also only non-zero conditionally
                        //if(temp == Vector3.zero)
                        temp += alignment(flock.FlockDirection) * alignmentWeight; //will always apply a force if called

                    }
                    steeringForce += temp;
                    steeringForce += flowFollow();
                    Flocker nearestWolf = gm.Wolves.Flockers[getNearest(gm.Wolves.Flockers)];
                    if((this.transform.position - nearestWolf.transform.position).sqrMagnitude < fleedistance*fleedistance)
                        state = DeerState.FLEE;
                    break;
                }
		//fleeing
            case DeerState.FLEE:
            {
                Flocker nearest = gm.Wolves.Flockers[getNearest(gm.Wolves.Flockers)];
		        steeringForce += evade(nearest);
		        if((nearest.transform.position - this.transform.position).sqrMagnitude > fleedistance*fleedistance)
		            state = DeerState.SEARCH;
                break;
            }
		//regroup
            case DeerState.SEARCH:
            {
                steeringForce += stayInBounds(gm.BOUNDS_RADIUS, gm.BOUNDS_CENTER);
                temp += regroup();
                if (temp == Vector3.zero) //we found a group!
                    state = DeerState.GRAZE;
                break;
            }
    }
        base.calcSteeringForces();
    }	

    /// <summary>
    /// After being separated by flee, the deer will try to find each other again and form
    /// new herds. Will return a force to the nearest deer, and if there is a deer within the regroup
    /// radius, will form a new flock with that deer and change state.
    /// </summary>
    /// <returns>A force that will direct the deer to the nearest detected deer, or 0 if a new flock has been formed.</returns>
    protected Vector3 regroup()
    {
		//find the nearest deer
        //need masterlist of all deer
        GameObject[] deer = GameObject.FindGameObjectsWithTag("deer");
        List<Flocker> deerFlockers = new List<Flocker>();
        foreach(GameObject d in deer)
        {
            deerFlockers.Add(d.GetComponent<Flocker>());
        }
        Flocker nearestDeer = deerFlockers[getNearest(deerFlockers)];
		
		if((nearestDeer.transform.position - this.transform.position).sqrMagnitude < regroupRadius*regroupRadius)
        {
            //if that deer has a flock, join it
            if(nearestDeer.flock != null)
            {
                this.flock = nearestDeer.flock;
                this.flock.addFlocker(this);
            }
		    //else make a new one with just the two of you
            else
            {
                Flock newFlock = new Flock();
            }
            return Vector3.zero; //flag
        }
		
		//else if too far away to join a new flock
        return arrive(nearestDeer.transform.position);
        
    }
}
