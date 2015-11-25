using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Vehicle Class is the base class for all autonomous agents. 
/// It holds the very basic behaviors, like seek, flee, evade, pursue, arrival,
/// and obstacle avoidance.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(BoxCollider))]

abstract public class Vehicle : MonoBehaviour {

    //basic physics attributes
    //position,forward, right will be accessed through Transform component
	protected Vector3 acceleration;
	protected Vector3 velocity;

    //values to be set in the inspector
	public float maxSpeed; //caps the magnitude of the velocity vector
	public float maxForce; //caps the applied force vector
	public float mass; //inversely affects the impact of force on acceleration
	public float radius; //radius of the theoretical bounding sphere on the object
    public float safeDistance; //used for obstacle avoidance - the amount of space desired between obstacles and the vehicle at all times
    public float arrivalDistance; //how far away should we start slowing down?
    public float pursueTimestep; //how many frames ahead will we project?
    public float evadeTimestep; //ditto above for evade


    protected GameManager gm;

	//access to character controller to move the model
	CharacterController charControl;

    /// <summary>
    /// Initializes the physics vectors, gets the character controller,
    /// and establishes the radius from the Mesh
    /// </summary>
	virtual public void Start(){
		acceleration = Vector3.zero;
		velocity = this.transform.forward;
		charControl = this.GetComponent<CharacterController>();
        gm = GameObject.Find("gameManager").GetComponent<GameManager>();

        //calculate the radius of the model by the dimensions of its box collider.
        float lenX = this.GetComponent<BoxCollider>().bounds.size.x;
        float lenZ = this.GetComponent<BoxCollider>().bounds.size.z;
        if (lenX < lenZ)
            radius = lenZ / 2;
        else
            radius = lenZ / 2;
	}

	
	/// <summary>
	/// Vehicle's update function calls calc steering forces and then
    /// updates velocity, the character controller, and the forward vector.
	/// </summary>
	protected void Update () {

		calcSteeringForces ();

		//numerically integrate velocity
		velocity += acceleration * Time.deltaTime;

		//limit velocity to max speed, remove y component
		velocity.y = 0;
		velocity = Vector3.ClampMagnitude (velocity, maxSpeed);

        //in case something strange happened, return to the ground
        if (this.transform.position.y > 1)
            velocity.y = -1;
        else
            velocity.y = 0;
        
		//move the character based on velocity
		charControl.Move (velocity * Time.deltaTime);

		
		//reset acceleration to 0
		acceleration = Vector3.zero;
        //update forward vector to match velocity
        transform.forward = velocity.normalized;
	}

    /// <summary>
    /// calcSteeringForces updates the acceleration of the 
    /// Vehicle using forces. Acceleration is changed during the method call.
    /// </summary>
	abstract protected void calcSteeringForces();

    /// <summary>
    /// applyForce changes the acceleration of the vehicle according to
    /// the provided force vector and the vehicle's mass.
    /// </summary>
    /// <param name="force">The force vector to be applied to the acceleration</param>
	protected void applyForce(Vector3 force)
	{
		this.acceleration = this.acceleration + force / mass;
	}
    /// <summary>
    /// seek returns a force vector that will compel the vehicle to move towards the target position.
    /// </summary>
    /// <param name="targetPosition">The position in 3d space that the vehicle will be drawn towards.</param>
    /// <returns>A force vector pushing the vehicle towards the target</returns>
	protected Vector3 seek (Vector3 targetPosition)
	{
		Vector3 seekVector = targetPosition - this.transform.position;
		
		seekVector = seekVector.normalized*maxSpeed;
		seekVector.y = 0;
		return (seekVector - this.velocity);
	}
    /// <summary>
    /// flee will produce a force vector that will compel the vehicle to move away from the target position
    /// </summary>
    /// <param name="threatPosition">The position in 3d space that the vehicle will be pushed away from.</param>
    /// <returns>A force vector pulling the vehicle away from the threat</returns>
    protected Vector3 flee(Vector3 threatPosition)
    {
        return seek(-threatPosition); //easy peasy, just go the other way
    }
    /// <summary>
    /// avoid returns the force needed to avoid the gameObject with an ObstacleScript that is passed in to a parameter.
    /// If no change of direction is necessary, it will return a 0 vector.
    /// </summary>
    /// <returns>A force vector for obstacle avoidance</returns>
    protected Vector3 avoid(GameObject obstacle)
    {

        Vector3 vecToCenter = Vector3.zero;
        float rad;
        Vector3 desired = Vector3.zero;

        vecToCenter = obstacle.transform.position - this.transform.position;
        rad = obstacle.GetComponent<ObstacleScript>().Radius;
        if (vecToCenter.sqrMagnitude > Mathf.Pow(radius + rad + safeDistance, 2))
            return desired;
        if (Vector3.Dot(vecToCenter, this.transform.forward) < 0)
            return desired;
        if (Mathf.Abs(Vector3.Dot(vecToCenter, this.transform.right)) > this.radius + rad + 1) //give ourselves a bit of a buffer
            return desired;

        //if we get to this point, we are on a collision course and need to turn
        if (Vector3.Dot(vecToCenter, this.transform.right) > 0)
            return this.transform.right * -1 * maxSpeed * safeDistance * safeDistance / vecToCenter.sqrMagnitude;
        else
            return this.transform.right * maxSpeed * safeDistance * safeDistance / vecToCenter.sqrMagnitude;

    }
    /// <summary>
    /// wander finds a slight random deviation from the current path, and produces a force vector that will push the
    /// vehicle along that deviation. 
    /// </summary>
    /// <returns>A force vector that will randomly change the direction of travel.</returns>
    protected Vector3 wander()
    {
        float wanderAngle = Random.Range(0, Mathf.PI*2); //any angle
        Vector3 force = new Vector3(Mathf.Cos(wanderAngle), 0, Mathf.Sin(wanderAngle))*5; //vector in new direction of length 5
        return force + 5*this.transform.forward - this.velocity; //move it 5 units in front of us, then use as a desired velocity
    }
	
    /// <summary>
    /// Arrive checks if the vehicle is within a certain distance of its target (parameter), and calculates a force
    /// that will smoothly slow down and stop the vehicle as it approaches the target when closer than that distance.
    /// </summary>
    /// <param name="destination">The destination to be arrived at</param>
    /// <returns>A force vector that will scale the velocity as necessary relative to the distance from the target.</returns>
	protected Vector3 arrive(Vector3 destination)
    {
        Vector3 desired = destination - this.transform.position; //the straight line between us and our target

        if(desired.sqrMagnitude > arrivalDistance*arrivalDistance) //we don't need no square roots!
        {
            float scale = map(desired.sqrMagnitude, 0, arrivalDistance * arrivalDistance, 0, maxSpeed);
            desired = desired.normalized * scale;
        }
        else
        {
            desired = desired.normalized * maxSpeed;
        }
        return desired;
    }
    /// <summary>
    /// evade uses the velocity of a threat to more intelligently flee from them by 
    /// predicting their future position. Please note that evade can only be used for vehicles,
    /// use flee for generic gameobjects.
    /// </summary>
    /// <param name="threat">The vehicle that should be evaded</param>
    /// <returns>A force vector that when applied will produce evasion behavior.</returns>
    protected Vector3 evade(Vehicle threat)
    {
        Vector3 projected = threat.transform.position + threat.velocity * evadeTimestep; //assume that the threat will continue along its current trajectory
        return flee(projected);
    }
    /// <summary>
    /// pursue uses the velocity of a target to more intelligently chase them by 
    /// predicting their future position. Please note that pursue can only be used for vehicles,
    /// use flee for generic gameobjects.
    /// </summary>
    /// <param name="threat">The gameobject that should be pursued</param>
    /// <returns>A force vector that when applied will produce pursuit behavior.</returns>
    protected Vector3 pursue(Vehicle target)
    {
        Vector3 projected = target.transform.position + target.velocity * evadeTimestep; //assume that the target will continue along its current trajectory
        return seek(projected);
    }

    /// <summary>
    /// map takes a value and it's current range, and returns a float indicating
    /// the corresponding value within a target range.
    /// </summary>
    /// <param name="value">the value to be mapped</param>
    /// <param name="ilbound">the lower bound of the starting range</param>
    /// <param name="iubound">the upper bound of the starting range</param>
    /// <param name="flbound">the lower bound of the target range</param>
    /// <param name="fubound">the upper bound of the target range</param>
    /// <returns>A float value within the target range that is equivalent to the value parameter within the starting range.</returns>
    private float map(float value, float ilbound, float iubound, float flbound, float fubound)
    {
        return ((value - ilbound) * (fubound - flbound) / (iubound - ilbound)) + flbound;

    }

    

}


