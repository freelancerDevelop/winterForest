
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
    private int terrainWidth; //z //need the dimensions of the terrain to make the flow field
    private int terrainLength; //x

    //*************************
    //Flocks
    //*************************
    public List<Flock> herds; //the multiple herds of deer
    private Flock wolves; //the pack of wolves
    private Vector3 wolfStart;
    private List<Vector3> deerStart;
    public float BOUNDS_RADIUS;
    public Vector3 BOUNDS_CENTER;

    public Flock Wolves
    { get { return wolves; } }
	public List<Flock> Herds
	{ get{return herds;}}
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

    //*************************
    //Obstacles
    //*************************
	private List<GameObject> obstacles;
    public List<GameObject> Obstacles
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
        terrainWidth = (int)terrain.terrainData.size.z;
        terrainLength = (int)terrain.terrainData.size.x;
        BOUNDS_CENTER = new Vector3(terrainLength / 2, 0, terrainWidth / 2);
        BOUNDS_RADIUS = 200.0f;
        obstacles = new List<GameObject>();
        deerStart = new List<Vector3>();
        flowField = new Vector3[terrainLength, terrainWidth];
        createFlowField();
        determineStartingLocations();
        wolves = new Flock(Random.Range(minWolves, maxWolves+1), wolfStart, wolfPrefab);
        herds = new List<Flock>();      

		for (int i = 0; i < numHerds; i++) {
			//make the herds
            herds.Add(new Flock(Random.Range(minDeer,maxDeer+1),Vector3.zero,deerPrefab));

		}

		//set camera to follow the game manager
		myCamera.GetComponent<SmoothFollow>().target = this.transform;
        myCamera.enabled = true;
        
	}

	/// <summary>
	/// createFlowField will use the waypoints to generate a flow field 
    /// used by the deer and the trees. Vectors will point towards areas of low tree density, and will have magnitude proportional to tree
    /// density in a given sector. Clearings will have 0 vectors associated with them. This flow field will also be used to
    /// place trees in the environment, with them being in general farther away from the center of waypoints and nonexistent
    /// within the radius of the waypoints.
    /// 
	/// </summary>
    void createFlowField()
    {
        GameObject[] waypoints = GameObject.FindGameObjectsWithTag("waypoint"); //get all the waypoints
        Vector2[] waypointLocs = new Vector2[waypoints.Length];
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypointLocs[i] = new Vector2((int)waypoints[i].transform.position.x,(int) waypoints[i].transform.position.z);
        }

        for(int i = 0; i < terrainLength; i++)
        {
            for(int j = 0; j < terrainWidth; j++)
            {
                Vector2 pos = new Vector2(i,j);
                int nearest = getNearest(pos, waypointLocs);
                if((waypointLocs[nearest]-pos).sqrMagnitude < Mathf.Pow(waypoints[nearest].GetComponent<waypointScript>().radius,2.0f))
                {
                    flowField[i, j] = Vector3.zero; //we're inside a meadow, so 0
                }
                else
                {
                    //need a vector pointing back towards the nearest point, with magnitude scaled based on distance
                    Vector3 fieldVec = new Vector3(waypointLocs[nearest].x, 0, waypointLocs[nearest].y) - new Vector3(pos.x, 0, pos.y);
                    fieldVec.Normalize();
                    fieldVec *= ((waypointLocs[nearest] - pos).magnitude - waypoints[nearest].GetComponent<waypointScript>().radius) / 3.0f; //scale based on how far away we are
                    flowField[i, j] = fieldVec;
                }

                Debug.DrawRay(new Vector3(i, 0, j), flowField[i, j], Color.red, 600.0f, false); // 10 minutes of flow field
            }

        }

        placeTrees(); //place all our trees! How exciting.

    }
    private void placeTrees()
    {
        GameObject tree;
        float rand;
        bool treePlaced = false; //controls how closely together trees get placed
        for(int i = 0; i < terrainLength; i++)
        {
            for(int j = 0; j < terrainWidth; j++)
            {
                if(treePlaced || flowField[i,j] == Vector3.zero)
                {
                    treePlaced = false;
                    continue; //don't put a tree in a clearing, dummy!
                }
                else
                {
                    rand = Random.Range(0.0f, 10000.0f);
                    if (rand < flowField[i, j].sqrMagnitude)
                    {
                        tree = (GameObject)GameObject.Instantiate(treePrefab1, new Vector3(i, 0, j), Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                        tree.AddComponent<ObstacleScript>(); //for radius
                        obstacles.Add(tree);
                        treePlaced = true;
                    }
                    else
                        treePlaced = false; //shouldn't need this, it would already be false to get here, but just for clarity
                }
            }
        }

    }

    /// <summary>
    /// Gets the index of the nearest point in the array to the position
    /// </summary>
    /// <param name="position"> The position that needs a nearest point</param>
    /// <param name="points"> An array of 2D points</param>
    /// <returns> the index of the nearest point</returns>
    private int getNearest(Vector2 position, Vector2[] points)
    {
        int nearest = 0;
        for (int i = 1; i < points.Length; i++)
        {
            //closer than our current nearest point
            if ((points[i] - position).sqrMagnitude < (points[nearest] - position).sqrMagnitude)
                nearest = i;
        }
        return nearest;
    }

    /// <summary>
    /// getFlow returns vectors out of the flow field
    /// </summary>
    /// <param name="indexPosition">The position of the desired flow vector, in index form</param>
    /// <returns>A force vector from the flow field</returns>
    public Vector3 getFlow(int xIndex, int zIndex)
    {
        if (xIndex >= 0 && zIndex >= 0 && xIndex < terrainLength && zIndex < terrainWidth)
            return flowField[xIndex, zIndex];
        else
            return Vector3.zero;
    }
    /// <summary>
    /// Uses the waypoints to determine where the deer flocks and the pack of wolves
    /// should start.
    /// </summary>
    void determineStartingLocations()
    {
		List<Vector3> deerStarts = new List<Vector3> ();


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
