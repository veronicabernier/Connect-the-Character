using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PathDynamic : MonoBehaviour
{

    [System.Serializable]
    public class Path
    {
        public Transform[] pathPositions;
        public int curPos;
    }


    public Path[] paths;
    public int lineThickness = 1;


    //the original vertical lines
    private Path[] originalPaths;


    // Start is called before the first frame update
    void Start()
    {
        //saving them to restore them and know where we can draw
        originalPaths = paths;

    }

    // Update is called once per frame
    void Update()
    {

    
        
    }

    public void OnDrawGizmos()
    {
        //draw all n (or 4) paths
        for (var i = 0; i < paths.Length; i++)
        {
            //2 or more positions for a valid path
            if (paths[i].pathPositions != null && paths[i].pathPositions.Length > 1)
            {
                //connect each position with a line
                for (var j = 1; j < paths[i].pathPositions.Length; j++)
                {
                    Debug.Log("drawing");
                    Handles.DrawBezier(paths[i].pathPositions[j - 1].position, paths[i].pathPositions[j].position,
                        paths[i].pathPositions[j - 1].position, paths[i].pathPositions[j].position, Color.white, null, lineThickness);
                }
            }
        }
    }
}
