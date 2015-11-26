
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The game manager initializes and builds everything necessary for the simulator to run,
/// handles the camera, and creates both the flow field and the obstacles array.
/// </summary>
public class GameManager : MonoBehaviour {
    
    //*************************
    //Camera Fields
    //*************************

    public Camera myCamera;
    public int cameraID; //which flock are we smooth following?

    //*************************
    //Flow Field
    //*************************
	private Vector3[,] flowField;
    public int flowFieldDiv; //how many divisions in our flow field?
    private float terrainWidth; //need the dimensions of the terrain to make the flow field
    private float terrainHeight;

    //*************************
    //Flocks
    //*************************
    private List<Flock> herds; //the multiple herds of deer
    private Flock wolves; //the pack of wolves
    private Vector3 wolfStart;
    private List<Vector3> deerStart;


    //*************************
    //Inspector Variables
    //*************************
    public int maxDeer; //the maximum number of deer in a herd
    public int minDeer; //the minimum number of deer in a herd
    public int numHerds; //number of herds of deer in the scene
    public int maxWolves; //maximum number of wolves in the pack
    public int minWolves; //minimum number of wolves in the pack
    public int numHerders; //number of wolves with herding behavior
    public int numHunters; //number of wolves with hunting behavior
    public int numTrees; //number of trees that will be generated in the scene. 
    public float treeWeight1; //how many of the trees should be of type 1,2,3? Weighted relative to the sum of all 3 weight values.
    public float treeWeight2;
    public float treeWeight3;

    //*************************
    //Prefabs
    //*************************
    public GameObject treePrefab1; //our trees
    public GameObject treePrefab2;
    public GameObject treePrefab3;
    public GameObject deerPrefab; //for deer and wolves
    public GameObject wolfPrefab;
    public Terrain terrain;
    public GameObject obstaclePrefab; //will be taken out after HW5

    //*************************
    //Obstacles
    //*************************
	private GameObject[] obstacles;
    public GameObject[] Obstacles
    {
        get { return obstacles; }
    }

	void Start () {
        //input validation for number of herds
        if (numHerds < 1)
            numHerds = 1;
        else if (numHerds > 9) //limiting the maximum number of herds to 9 makes it easier to handle input for the camera
            numHerds = 9;

        //initializing everything
        deerStart = new List<Vector3>();
        flowField = new Vector3[flowFieldDiv, flowFieldDiv];
        createFlowField();
        determineStartingLocations();
        placeTrees();
        wolves = new Flock(Random.Range(minWolves, maxWolves+1), wolfStart, wolfPrefab);
        herds = new List<Flock>();      

		for (int i = 0; i < numHerds; i++) {
			//make the herds
            herds.Add(new Flock(Random.Range(minDeer,maxDeer+1),Vector3.zero,deerPrefab));

		}

		//set camera to follow the game manager
		myCamera.GetComponent<SmoothFollow>().target = this.transform;
        myCamera.enabled = true;

        //only here for testing purposes
        Vector3 pos; //no need to reallocate this every time through the loop
        obstacles = new GameObject[numTrees];
        for(int i = 0; i < numTrees; i++)
        {
            pos = new Vector3( Random.Range(0, 50), 0.0f, Random.Range(0, 50));
            Quaternion rot = Quaternion.Euler(0.0f,Random.Range(0,180),0.0f);
            obstacles[i] = (GameObject)Instantiate(obstaclePrefab, pos, rot);
            obstacles[i].AddComponent<ObstacleScript>(); //so they have a radius
            
            
        }
	}

	/// <summary>
	/// createFlowField will use hard coded data to generate a flow field 
    /// used by the deer. Vectors will point towards areas of low tree density, and will have magnitude proportional to tree
    /// density in a given sector. Clearings will have 0 vectors associated with them. This flow field will also be used to
    /// place trees in the environment
    /// 
    /// Higher dimension flow fields will be, in general, more accurate.
	/// </summary>
    void createFlowField()
    {

    }
    /// <summary>
    /// placeTrees will use the flow field to place trees appropriately throughout the environment, placing more trees in 
    /// areas with higher magnitude vectors. Will use the weights provided in the inspector to decide on the proportion of different
    /// types of trees.
    /// </summary>
    void placeTrees()
    {

    }
    /// <summary>
    /// Uses the flow field to determine where the deer flocks and the pack of wolves
    /// should start.
    /// </summary>
    void determineStartingLocations()
    {


    }
    /// <summary>
    /// Prompts the flocks to calculate their centroid and flock direction,
    /// adjusts the camera as necessary in reaction to user input.
    /// </summary>
	void Update () {

        if (wolves.NumFlockers != 0)
        {
            wolves.CalcCentroid();
            wolves.CalcFlockDirection();
        }

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
        if(cameraID == 0)
        {
            this.transform.position = wolves.Centroid;
            this.transform.forward = wolves.FlockDirection;
        }
        else
        {
            this.transform.position = herds[cameraID - 1].Centroid;
            this.transform.forward = herds[cameraID - 1].FlockDirection;
        }
	}

	

	

}
