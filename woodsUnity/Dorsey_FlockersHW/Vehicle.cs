using UnityEngine;
using System.Collections;

//use the Generic system here to make use of a Flocker list later on
using System.Collections.Generic;

/// <summary>
/// Vehicle Class is the base class for all autonomous agents. 
/// It holds the very basic behaviors, like seek, flee, evade, pursue, arrival,
/// and obstacle avoidance.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(BoxCollider))]

abstract public class Vehicle : MonoBehaviour {


	//Class Fields
	protected Vector3 acceleration;
	protected Vector3 velocity;
	//position,forward, right will be accessed through Transform component
    public float safeDistance = 5.0f;

	//flocking algorithms will need the velocity
	public Vector3 Velocity{
		get{return velocity;}
	}

    //lets them be accessed through the inspector for weighting
	public float maxSpeed;
	public float maxForce;
	public float mass;
	public float radius;

    protected GameManager gm;

	//access to character controller to move the model
	CharacterController charControl;

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

	
	// Update is called once per frame
	protected void Update () {

		CalcSteeringForces ();

		//add acceleration to velocity
		velocity += acceleration * Time.deltaTime;
		//limit velocity to max speed
		velocity.y = 0;
		velocity = Vector3.ClampMagnitude (velocity, maxSpeed);

        if (this.transform.position.y > 1)
            velocity.y = -1;
        else
            velocity.y = 0;
        
		//move the character based on velocity
		charControl.Move (velocity * Time.deltaTime);
		//change forward and right(not with Unity)
		//reset acceleration to 0
		acceleration = Vector3.zero;

        transform.forward = velocity.normalized;
	}

	abstract protected void CalcSteeringForces();

	protected void applyForce(Vector3 force)
	{
		this.acceleration = this.acceleration + force / mass;
	}

	protected Vector3 seek (Vector3 targetPosition)
	{
		Vector3 seekVector = targetPosition - this.transform.position;
		
		seekVector = seekVector.normalized*maxSpeed;
		seekVector.y = 0;
		return (seekVector - this.velocity);
	}

    //my implementation of avoid simply handles the whole array of objects on its own.
    //in the future this may change
	protected Vector3 avoid()
	{
		GameObject[] obstacles = gm.Obstacles;
		Vector3 vecToCenter = Vector3.zero;
		float rad;
		Vector3 desired = Vector3.zero;
		for (int i = 0; i < obstacles.Length; i++ )
		{
			vecToCenter = obstacles[i].GetComponent<ObstacleScript>().Position - this.transform.position;
			rad = obstacles[i].GetComponent<ObstacleScript>().Radius;
			if (vecToCenter.sqrMagnitude > Mathf.Pow(radius + rad + safeDistance,2))
				continue;
			if (Vector3.Dot(vecToCenter,this.transform.forward) < 0)
				continue;
            if (Mathf.Abs(Vector3.Dot(vecToCenter, this.transform.right)) > this.radius + rad + 4) //give ourselves a bit of a buffer
                continue;

            Debug.Log("collision firing");
            if (Vector3.Dot(vecToCenter, this.transform.right) > 0)
                desired += this.transform.right * -1 * maxSpeed * safeDistance * safeDistance / vecToCenter.sqrMagnitude;
            else 
                desired += this.transform.right * maxSpeed * safeDistance * safeDistance / vecToCenter.sqrMagnitude;
            
		}

        return desired*maxSpeed;
        
	}
    //kind of a brusque implementation of wander but it works
    protected Vector3 wander()
    {
        float wanderAngle = Random.Range(0, Mathf.PI*2); //any angle
        Vector3 force = new Vector3(Mathf.Cos(wanderAngle), 0, Mathf.Sin(wanderAngle))*5; //vector in new direction of length 5
        return force + 5*this.transform.forward - this.velocity; //move it 5 units in front of us, then use as a desired velocity
    }
	
	

}
