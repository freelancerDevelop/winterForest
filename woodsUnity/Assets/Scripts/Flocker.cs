using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class Flocker : Vehicle {

	private Vector3 steeringForce;
    public Flock flock;
	public float seekWeight;
    public float avoidWeight ;
	public float separate ;
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
	

	protected override void CalcSteeringForces()
	{
        Vector3 temp = Vector3.zero;
		//create zero vector to represent the seek force
		steeringForce = new Vector3();
        //obstacle avoidance and staying in bounds gets priority
        temp = steeringForce += avoid() * avoidWeight;
        Debug.Log(temp);
        //stay in bounds comes next
        temp += stayInBounds(50.0f, new Vector3(170, 0, 118)) * boundsWeight;
        if(temp == Vector3.zero) //only if we're not trying to stay in bounds
          steeringForce += wander() * wanderWeight;
        steeringForce += temp;
		//call flocking forces
        //temp = Vector3.zero;

        //both of these are only non-zero conditionally
        temp = separation(separate) * separationWeight;
        temp += cohesion(flock.Centroid) * cohesionWeight; 
        steeringForce += alignment(flock.FlockDirection) * alignmentWeight; //applied every time called
        
        steeringForce += temp;
        
		//limit the steering force
		steeringForce = Vector3.ClampMagnitude (steeringForce, maxForce);
		//apply the force to acceleration
		applyForce (steeringForce);
	}

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
            if (Vector3.Dot(vecToCenter, this.transform.right) > 0)
                desired += this.transform.right.normalized * -1 * maxSpeed;
            else
                desired += this.transform.right.normalized * maxSpeed;
        }
        if (desired != Vector3.zero)
            return (desired.normalized * maxSpeed - this.velocity);
        else
            return desired;
    }

    public Vector3 alignment(Vector3 alignVector)
    {

        return (alignVector - this.velocity); //the flock's velocity is our desired velocity so don't need
                                                //more than this
    }
    public Vector3 cohesion(Vector3 cohesionVector)
    {
        if ((this.transform.position - flock.Centroid).sqrMagnitude > flockRadius * flockRadius)
            return seek(cohesionVector).normalized; //pretty much the same as boundary checking but
                                                    //with a moving center point
        else
            return Vector3.zero;
    }
    public Vector3 stayInBounds(float radius, Vector3 center)
    {
        if ((this.transform.position - center).sqrMagnitude > radius * radius)
        {   
           
            return seek(center); //could be more sophisticated, but... meh
           
        }
        else
            return Vector3.zero;
    }
}
