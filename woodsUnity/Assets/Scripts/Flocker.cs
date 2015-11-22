using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class Flocker : Vehicle {

	private Vector3 steeringForce;
    public Flock flock;
	public float seekWeight = 75.0f;
    public float avoidWeight = 100.0f;
	public float separate = 1.0f;
	public float separationWeight = 10.0f;
	public float cohesionWeight = 1.0f;
	public float alignmentWeight = 1.0f;
    public float boundsWeight = 1.0f;
    public float wanderWeight = 20.0f;
    public float flockRadius = 15.0f;
	// Call Inherited Start and then do our own
	override public void Start () {
		base.Start();
		steeringForce = Vector3.zero;
	}
	

	protected override void CalcSteeringForces()
	{
        Vector3 temp;
		//create zero vector to represent the seek force
		steeringForce = new Vector3();

        //stay in bounds
        temp = stayInBounds(50.0f, new Vector3(170, 0, 118)) * boundsWeight;
        if(temp == Vector3.zero) //only if we're not trying to stay in bounds
            steeringForce += wander() * wanderWeight;
        steeringForce += temp;
		//call flocking forces
        temp = Vector3.zero;
        //if(steeringForce == Vector3.zero) //not being redirected by the bounds
        
        temp = separation(separate) * separationWeight;
        if (temp == Vector3.zero)
        {
            temp += cohesion(flock.Centroid) * cohesionWeight;
        }
		if(temp == Vector3.zero)
            steeringForce += alignment(flock.FlockDirection) * alignmentWeight;
        steeringForce += temp;
        
		//obstacle avoidance - weight that
        //steeringForce += avoid() * avoidWeight;
		//apply each to the global steering force


		
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
        for (int i = 0; i < nearest.Count; i++)
        {
            Vector3 vecToCenter = nearest[i].transform.position - this.transform.position;
            if (Vector3.Dot(vecToCenter, this.transform.right) > 0)
                desired += this.transform.right.normalized * -1 * maxSpeed;
            if (Vector3.Dot(vecToCenter, this.transform.right) < 0)
                desired += this.transform.right.normalized * maxSpeed;
        }
        return (desired.normalized*maxSpeed - this.transform.forward).normalized;
    }

    public Vector3 alignment(Vector3 alignVector)
    {

        return (alignVector - this.transform.forward.normalized).normalized;
    }
    public Vector3 cohesion(Vector3 cohesionVector)
    {
        if ((this.transform.position - flock.Centroid).sqrMagnitude > flockRadius * flockRadius)
            return seek(cohesionVector).normalized;
        else
            return Vector3.zero;
    }
    public Vector3 stayInBounds(float radius, Vector3 center)
    {
        if ((this.transform.position + this.transform.forward*maxSpeed - center).sqrMagnitude > radius * radius)
        {   
           
            return this.transform.right.normalized;
           
        }
        else
            return Vector3.zero;
    }
}
