using UnityEngine;
using System.Collections;


/// <summary>
/// Deer script inherits from Flocker and controls the behavior of deer.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class deerScript : Flocker {

    enum DeerState { GRAZE, FLEE, SEARCH, ALERT };
    DeerState state;

    //**********
    // Inspector Variables
    //**********
    public float regroupRadius;

    /// <summary>
    /// The particular calcSteeringForces used by
    /// deer.
    /// </summary>
    protected override void calcSteeringForces()
    {
        steeringForce = Vector3.zero;
        steeringForce += arrive(gm.Obstacles[0].transform.position);
        base.calcSteeringForces();
    }	

    /// <summary>
    /// After being separated by flee, the deer will try to find each other again and form
    /// new herds. Will return a force to the nearest deer, and if there is a deer within the regroup
    /// radius, will form a new flock with that deer and change state.
    /// </summary>
    /// <returns>A force that will direct the deer to the nearest detected deer.</returns>
    protected Vector3 regroup()
    {
        return Vector3.zero;
    }
}
