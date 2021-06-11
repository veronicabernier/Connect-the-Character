using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PathConnections : MonoBehaviour
{
    [System.Serializable]
    public class Path
    {
        public Transform[] pathPositions;
        int curPos;
    }


    [SerializeField] public Path[] paths;


    //line renderer preferences
    [Header("Paths Lines")]

    [SerializeField] public float lineThickness = 0.10f;
    [SerializeField] public Material lineMaterial;
    [SerializeField] public Transform parent;


    // Start is called before the first frame update
    void Start()
    {
        drawPaths();
    }

    public void drawPaths()
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
                    drawLine(paths[i].pathPositions[j - 1].position, paths[i].pathPositions[j].position);

                }
            }
        }
    }


    public void drawLine(Vector3 pos1, Vector3 pos2)
    {
        LineRenderer lineRenderer = new GameObject("line").AddComponent<LineRenderer>();
        lineRenderer.transform.SetParent(parent);
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = lineThickness;
        lineRenderer.endWidth = lineThickness;
        lineRenderer.material = lineMaterial;
        lineRenderer.SetPosition(0, pos1);
        lineRenderer.SetPosition(1, pos2);
    }


}