using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Flocker is the generic class of
/// all game objects with flocking behavior. All generic
/// flocking algorithms are implemented in this class.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class Flocker : Vehicle {

    //***************
    //Flock Attributes
    //***************
	protected Vector3 steeringForce;
    public Flock flock;

    //***************
    //Inspector variables
    //***************
	public float seekWeight;
    public float avoidWeight ;
	public float separateDistance;
	public float separationWeight ;
	public float cohesionWeight;
	public float alignmentWeight;
    public float boundsWeight;
    public float wanderWeight;
    public float flockRadius;


	// Call Inherited Start and then do our own
	override public void Start () {
		base.Start();
		steeringForce = Vector3.zero;
	}
	
    /// <summary>
    /// Flocker's calcSteeringForces method will be called at the end of 
    /// the calcSteeringForces methods of deerScript and wolfScript.
    /// It applies the steeringForce to the acceleration after clamping it.
    /// </summary>
	protected override void calcSteeringForces()
	{
        
		//limit the steering force
		steeringForce = Vector3.ClampMagnitude (steeringForce, maxForce);
		//apply the force to acceleration
		applyForce (steeringForce);
	}
    /// <summary>
    /// separation checks to see if any fellow flock memebers are too close
    /// to the flocker, and if so, calculates a force to move the flocker away from them.
    /// </summary>
    /// <param name="separationDistance">the minimum distance necessary for the flocker to not react to a nearby flockmate</param>
    /// <returns>a force vector that will produce the necessary separation behavior (may be 0)</returns>
    public Vector3 separation(float separationDistance)
    {
        
        if (flock == null || flock.NumFlockers == 0) //we don't have a flock
            return Vector3.zero;

        List<GameObject> nearest = new List<GameObject>(); //holds the neighbors that are too close
        for (int i = 0; i < flock.NumFlockers; i++)
        {
            if (flock.Flockers[i] == this) //don't steer away from yourself
                continue;
            if (Vector3.SqrMagnitude(this.transform.position - flock.Flockers[i].transform.position) < separationDistance * separationDistance)
                nearest.Add(flock.Flockers[i]);
        }
        Vector3 desired = Vector3.zero;

        //remarkably similar to obstacle avoidance, hmm..
        for (int i = 0; i < nearest.Count; i++)
        {
            Vector3 vecToCenter = nearest[i].transform.position - this.transform.position;
            if (Vector3.Dot(vecToCenter, this.transform.forward) > 0)
            {
                if (Vector3.Dot(vecToCenter, this.transform.right) > 0)
                    desired += this.transform.right.normalized * -1 * maxSpeed;
                else
                    desired += this.transform.right.normalized * maxSpeed;
            }
            else
            {
                desired += this.transform.forward * maxSpeed;
            }
        }
        if (desired != Vector3.zero)
            return (desired - this.velocity);
        else
            return desired;
    }
    /// <summary>
    /// alignment provides a force that will try to align the flocker's velocity
    /// with the given vector.
    /// </summary>
    /// <param name="alignVector">The vector with which the flocker's velocity should be aligned. Average flock velocity is recommended.</param>
    /// <returns>A force vector that will produce alignment behavior.</returns>
    public Vector3 alignment(Vector3 alignVector)
    {

        return (alignVector - this.velocity); //the flock's velocity is our desired velocity so don't need
                                                //more than this
    }
    /// <summary>
    /// cohesion checks to see if the flocker is outside of the allowable spread of the flock, and if so
    /// redirects them towards the provided vector (should be the centroid)
    /// </summary>
    /// <param name="cohesionVector">The vector the flocker should cohere to. Centroid recommended.</param>
    /// <returns>An appropriate force vector to produce cohesion (may be 0)</returns>
    public Vector3 cohesion(Vector3 cohesionVector)
    {
        if ((this.transform.position - flock.Centroid).sqrMagnitude > flockRadius * flockRadius)
            return seek(cohesionVector); //pretty much the same as boundary checking but
                                                    //with a moving center point
        else
            return Vector3.zero;
    }
    /// <summary>
    /// stayInBounds checks to see whether the flocker is outside of a given radius
    /// and if so redirects them to the center point.
    /// </summary>
    /// <param name="radius">The radius of the bounding circle</param>
    /// <param name="center">The center of the bounding circle</param>
    /// <returns>A force vector towards the center point</returns>
    public Vector3 stayInBounds(float radius, Vector3 center)
    {
        if ((this.transform.position - center).sqrMagnitude > radius * radius)
        {   
           
            return seek(center); //could be more sophisticated, but... meh
           
        }
        else
            return Vector3.zero;
    }
    /// <summary>
    /// flowFollow will find the appropriate vector in the flow field for the flocker's current position, and
    /// return it.
    /// </summary>
    /// <returns>the vector from the flow field corresponding to the flocker's current position</returns>
    public Vector3 flowFollow()
    {
        return gm.getFlow((int)this.transform.position.x, (int)this.transform.position.z);
    }
    /// <summary>
    /// followLeader calculates a point behind the flock's leader that they will all try to converge upon. it will
    /// also attempt to move them out of the way of the leader if they are in the leader's path.
    /// </summary>
    /// <param name="leader">The vehicle being followed as a leader</param>
    /// <returns>the force vector that will produce leader following behavior</returns>
    public Vector3 followLeader(Vehicle leader, float followDistance)
    {
        if (this == leader) //don't follow yourself
            return Vector3.zero;
        Vector3 desired = Vector3.zero;

        if((leader.transform.position - leader.transform.forward * followDistance).sqrMagnitude < followDistance*followDistance)
        {
            desired += evade(leader);
        }
        else
        //we're not in the way, so follow
        {
            desired += arrive(leader.transform.position - leader.transform.forward * followDistance);
            //desired += separation(separateDistance)*separationWeight;
        }

        return desired;
    }

	protected GameObject getNearest(List<GameObject> flock)
	{
		return null;
	}
}
