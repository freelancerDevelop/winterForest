using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Deer script inherits from Flocker and controls the behavior of deer.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class deerScript : Flocker {

    public enum DeerState { GRAZE, FLEE, SEARCH}; 
    public DeerState state = DeerState.GRAZE;
    public float walkMaxSpeed;
    public float runMaxSpeed;
    public float flowWeight;
    
    public int id;
   
    

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
                    if (anim.GetCurrentAnimatorStateInfo(0).nameHash == runHash)
                        anim.SetTrigger("walkTrigger");
                    maxSpeed = walkMaxSpeed;

                    if ((this.transform.position - flock.seekpoints[flock.seekindex]).sqrMagnitude < 4)
                    {
                        int currIndex = flock.seekindex;
                        do
                            flock.seekindex = Random.Range(0, flock.seekpoints.Length);
                        while (flock.seekindex == currIndex);
                    }

                    steeringForce += wander() * wanderWeight;
                    steeringForce += seek(flock.seekpoints[flock.seekindex]) * seekWeight;
                    steeringForce += this.transform.forward;
                   
                    temp += cohesion(flock.Centroid) * cohesionWeight; //only non-zero conditionally
                    temp += separation(separateDistance) * separationWeight; //also only non-zero conditionally   
                    steeringForce += seek(flock.seekpoints[flock.seekindex]) * seekWeight;
                    temp += alignment(flock.FlockDirection) * alignmentWeight; //will always apply a force if called

                    foreach (GameObject obstacle in gm.Obstacles)
                    {
                        temp += avoid(obstacle) * avoidWeight;
                    }

                    steeringForce += temp;

                    steeringForce += flowFollow() * flowWeight;
                    int index = getNearest(gm.Wolves.Flockers);
                    if (index > -1)
                    {
                        //Debug.Log("checking for wolves");
                        Flocker nearestWolf = gm.Wolves.Flockers[index];
                        if ((this.transform.position - nearestWolf.transform.position).sqrMagnitude < fleedistance * fleedistance)
                        {
                            Debug.Log("Change State to flee");
                            foreach (Flocker f in flock.Flockers)
                                if (f != null)
                                    f.GetComponent<deerScript>().state = DeerState.FLEE;
                            if (gm.herds.Contains(flock))
                                gm.herds.Remove(flock); //should get garbage collected if we remove our way of getting it
                            flock = null;
                        }
                    }
                    break;
                }
		//fleeing
            case DeerState.FLEE:
            {
                if (anim.GetCurrentAnimatorStateInfo(0).nameHash == walkHash)
                    anim.SetTrigger("runTrigger");
                maxSpeed = runMaxSpeed;
                bool safe = true;
                foreach(Flocker wolf in gm.Wolves.Flockers)
                {
                    steeringForce += flee(wolf.transform.position);
                    if ((wolf.transform.position - this.transform.position).sqrMagnitude < fleedistance * fleedistance)
                        safe = false;
                }
                steeringForce += separation(separateDistance);
                if (safe)
                    state = DeerState.SEARCH;
                break;
            }
		//regroup
            case DeerState.SEARCH:
            {
                maxSpeed = walkMaxSpeed;
                temp += regroup();
                if (temp == Vector3.zero) //we found a group!
                    state = DeerState.GRAZE;
                break;
            }
    }
        steeringForce += stayInBounds() * boundsWeight;
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
		
		if(nearestDeer != null && (nearestDeer.transform.position - this.transform.position).sqrMagnitude < regroupRadius*regroupRadius)
        {
            //if that deer has a flock, join it
            if(nearestDeer.flock != null)
            {
                this.flock = nearestDeer.flock;
                this.flock.addFlocker(this);
            }
		    //else make a new one with just the two of you
			else if(this.flock == null) //the extra check is just in case they're trying to make new flocks for each other at the same time, dunno if Unity threads
            {
                gm.herds.Add (new Flock(this)); //will assign it to us within the constructor
				flock.addFlocker (nearestDeer);
				nearestDeer.flock = flock;
                flock.seekpoints = gm.getSeekPoints();
                flock.seekindex = 0;
            }
            return Vector3.zero; //flag
        }
		
		//else if too far away to join a new flock
        return arrive(nearestDeer.transform.position);
        
    }

    
}
