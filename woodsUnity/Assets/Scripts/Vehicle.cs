using UnityEngine;
using System.Collections;

//use the Generic system here to make use of a Flocker list later on
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]

abstract public class Vehicle : MonoBehaviour {


	//Class Fields
	protected Vector3 acceleration;
	protected Vector3 velocity;
	//position,forward, right will be accessed through Transform component
	protected Vector3 desired;

	//flocking algorithms will need the velocity
	public Vector3 Velocity{
		get{return velocity;}
	}

	public float maxSpeed = 6;
	public float maxForce = 12;
	public float mass = 1;
	public float radius = 1;

    protected GameManager gm;

	//access to character controller to move the model
	CharacterController charControl;

	virtual public void Start(){
		acceleration = Vector3.zero;
		velocity = this.transform.forward;
		desired = Vector3.zero;
		charControl = this.GetComponent<CharacterController>();
        gm = GameObject.Find("gameManager").GetComponent<GameManager>();
	}

	
	// Update is called once per frame
	protected void Update () {

		CalcSteeringForces ();

		//add acceleration to velocity
		velocity += acceleration * Time.deltaTime;
		//limit velocity to max speed
		velocity.y = 0;
		velocity = Vector3.ClampMagnitude (velocity, maxSpeed);
        
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
		seekVector.Normalize ();
		seekVector = seekVector*maxSpeed;
		seekVector.y = 0;
		return (seekVector - this.velocity).normalized;
	}

	protected Vector3 avoid()
	{
		GameObject[] obstacles = gm.Obstacles;
		Vector3 vecToCenter;
		float rad;
		Vector3 desired = Vector3.zero;
		for (int i = 0; i < obstacles.Length; i++ )
		{
			vecToCenter = obstacles[i].transform.position - this.transform.position;
			rad = obstacles[i].GetComponent<ObstacleScript>().Radius;
			if (Vector3.SqrMagnitude(vecToCenter) > rad * rad)
				continue;
			if (Vector3.Dot(vecToCenter, this.transform.forward) < 0)
				continue;
			if (Mathf.Abs(Vector3.Dot(vecToCenter, this.transform.forward)) > this.radius + rad)
				continue;
			if(Vector3.Dot(vecToCenter,this.transform.right) > 0)
				desired+= this.transform.right*-1*maxSpeed;
			if(Vector3.Dot(vecToCenter,this.transform.right) < 0)
				desired+= this.transform.right*maxSpeed;
		}
		
		return desired.normalized;
	}

    protected Vector3 wander()
    {
        Vector3 circleCenter = this.transform.forward.normalized * 10;

        Vector3 displacement = new Vector3(0, 0, 1);
        displacement *= 10;
        float angle = Random.value * 2*Mathf.PI;
        displacement.Set(Mathf.Cos(angle) * 10, 0, Mathf.Sin(angle) * 10);

        Vector3 wanderForce = circleCenter + displacement;
        return wanderForce;
    }
	
	

}
