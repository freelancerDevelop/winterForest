
using UnityEngine;
using System.Collections;

//add using System.Collections.Generic; to use the generic list format
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

    public Camera myCamera;
    public int cameraID; //which flock are we smooth following?
	public Vector3[,] flowField;

    List<Flock> herds; //the multiple herds of deer
    Flock wolves; //the pack of wolves
	
    public int maxDeer; //the maximum number of deer in a herd
    public int minDeer; //the minimum number of deer in a herd
    public int numHerds; //number of herds of deer in the scene
    public int maxWolves; //maximum number of wolves in the pack
    public int minWolves; //minimum number of wolves in the pack
    public int numHerders; //number of wolves with herding behavior
    public int numHunters; //number of wolves with hunting behavior


    public GameObject treePrefab1; //our trees
    public GameObject treePrefab2;
    public GameObject treePrefab3;
    public GameObject deerPrefab; //for deer and wolves
    public GameObject wolfPrefab;
    public Terrain terrain;

    public GameObject obstaclePrefab; //will be taken out after HW5
	
	private GameObject[] obstacles;
    public GameObject[] Obstacles
    {
        get { return obstacles; }
    }
	void Start () {
		//create position for the noodle and make it
		Vector3 pos = Vector3.zero;
        herds = new List<Flock>();

        
        //wolves = new Flock((Random.Range(minWolves,maxWolves+1)),Vector3.zero,wolfPrefab);

        if (numHerds < 1) //makes it easier to handle the camera thing
            numHerds = 1;
        else if (numHerds > 9)
            numHerds = 9;

		for (int i = 0; i < numHerds; i++) {
			//make the herds
            herds.Add(new Flock(Random.Range(minDeer,maxDeer+1),new Vector3(170,0, 118),deerPrefab));

		}
		//set camera to follow the first googlyeye guy, for now
		myCamera.GetComponent<SmoothFollow> ().target = this.transform; 

        obstacles = new GameObject[20];
        for(int i = 0; i < 20; i++)
        {
            pos = new Vector3( 171 + Random.Range(-10, 10), 0.0f, 118 +  Random.Range(-50, 50));
            Quaternion rot = Quaternion.Euler(0.0f,Random.Range(0,180),0.0f);
            obstacles[i] = (GameObject)Instantiate(obstaclePrefab, pos, rot);
            obstacles[i].AddComponent<ObstacleScript>(); //so they have a radius
            
        }
	}
	

	void Update () {

        //wolves.CalcCentroid();
        //wolves.CalcFlockDirection();

        foreach (Flock flock in herds)
        {
            flock.CalcCentroid();
            flock.CalcFlockDirection();
        }

        //for switching cameras
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (myCamera.enabled)
            {
                myCamera.enabled = false;
                Camera.main.enabled = true;
            }
            else
            {
                myCamera.enabled = true;
                Camera.main.enabled = false;
            }
        }

        //follow the correct herd
        //if(cameraID == 0)
        //{
            //this.transform.position = wolves.Centroid;
            //this.transform.forward = wolves.FlockDirection;
        //}
        //else
        //{
            this.transform.position = herds[cameraID - 1].Centroid;
            this.transform.forward = herds[cameraID - 1].FlockDirection;
        //}
	}

	

	

}
