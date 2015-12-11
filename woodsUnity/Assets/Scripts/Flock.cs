using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Flock handles individual flocks
/// </summary>
public class Flock{

    
    //****************
    //Flock Attributes
    //****************
    private Vector3 centroid;
    private Vector3 flockDirection;
    private int numFlockers;
	public int leader = -1; //always -1 for deer, will be assigned for wolf

    public int NumFlockers { get { return numFlockers; } }
    public Vector3 Centroid { get { return centroid; } }
    public Vector3 FlockDirection { get { return flockDirection; } }

    //list of flockers in the flock
    private List<Flocker> flockers;
    public List<Flocker> Flockers { get { return flockers; } }

    /// <summary>
    /// The constructor of the Flock class.
    /// </summary>
    /// <param name="numFlock"> Number of flockers in the flock</param>
    /// <param name="centroidStart"> The starting position of the centroid</param>
    /// <param name="prefab"> The prefab of the flocker</param>
    public Flock(int numFlock, Vector3 centroidStart, GameObject prefab)
    {
        centroid = Vector3.zero;
        flockDirection = Vector3.zero;
        flockers = new List<Flocker>();
        numFlockers = numFlock;

        for (int i = 0; i < numFlock; i++)
        {
            GameObject newflocker = (GameObject) Object.Instantiate(prefab, centroidStart + new Vector3(Random.Range(0, 6), 0.97f, Random.Range(0, 6)), Quaternion.identity);
            flockers.Add(newflocker.GetComponent<Flocker>());
            flockers[i].GetComponent<Flocker>().flock = this; //let the flocker know what flock they're in

        }



    }
    //overloaded for wolves
    public Flock(int numFlock, Vector3 centroidStart, GameObject prefab, int numHerders)
    {
        centroid = Vector3.zero;
        flockDirection = Vector3.zero;
        flockers = new List<Flocker>();
        numFlockers = numFlock;

        for (int i = 0; i < numFlock; i++)
        {
            GameObject newThing = (GameObject)Object.Instantiate(prefab, centroidStart + new Vector3(Random.Range(4, 6), 1.2f, Random.Range(4, 6)), Quaternion.identity);
            flockers.Add(newThing.GetComponent<Flocker>());
            flockers[i].GetComponent<Flocker>().flock = this; //let the flocker know what flock they're in
            flockers[i].GetComponent<wolfScript>().id = i;
            if(i < numHerders && i < numFlock)
            {
                flockers[i].GetComponent<wolfScript>().isHerder = true;
               
            }

        }
        if (numFlockers != 0)
            leader = 0;


    }
    public Flock(Flocker initFlocker) //overloaded for deer regrouping
    {
		centroid = Vector3.zero;
		flockDirection = Vector3.zero;
		flockers = new List<Flocker>();
		numFlockers = 1;
		flockers.Add (initFlocker);
		flockers [0].flock = this;
    }


	
    /// <summary>
    /// CalcCentroid calculates the center of the flock and stores it in centroid.
    /// </summary>
    public void CalcCentroid()
    {
        if (numFlockers == 0)
            return;

        centroid = Vector3.zero;
        foreach (Flocker flocker in flockers)
        {
           if(flocker != null)
            centroid += flocker.transform.position;
        }
        centroid /= numFlockers;

    }

    /// <summary>
    /// CalcFlockDirection calculates the average velocity of the flock, normalizes it, and then stores it in flockDirection.
    /// </summary>
    public void CalcFlockDirection()
    {
        if (numFlockers == 0)
            return;

        flockDirection = Vector3.zero;
        foreach(Flocker flocker in flockers)
        {
            flockDirection += flocker.transform.forward;
        }
        flockDirection.Normalize();
    }

    public void addFlocker(Flocker f)
    {
        flockers.Add(f);
        numFlockers++;
    }
}
