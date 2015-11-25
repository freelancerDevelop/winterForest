using UnityEngine;
using System.Collections;


/// <summary>
/// Wolf script inherits from Flocker and controls the behavior of wolves.
/// </summary>
public class wolfScript:Flocker {

    private bool isHerder; //does this wolf herd or hunt when deer are found?
    enum WolfState {TRACK, HUNT, HERD, EAT};
    WolfState state;

    /// <summary>
    /// the calcSteeringForces method used by wolves.
    /// </summary>
    protected override void calcSteeringForces()
    {
        base.calcSteeringForces();
    }	

    private Vector3 herd()
    {
        return Vector3.zero;
    }
}
