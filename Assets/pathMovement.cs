using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class pathMovement : MonoBehaviour
{

    public Transform[] PathSequence; //arrays of points
    public int movingTo = 0; //index in PathSequence


    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnDrawGizmos()
    { 
        // make sure there's a path
        if(PathSequence == null || PathSequence.Length < 2)
        {
            return;
        }

        //draw line between each point
        for(var i=1; i < PathSequence.Length; i++)
        {
            Gizmos.DrawLine(PathSequence[i - 1].position, PathSequence[i].position);
        }

    }

}
