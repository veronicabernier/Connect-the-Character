using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PathConnections : MonoBehaviour
{
    [System.Serializable]
    public class Path
    {
        public Transform[] pathPositions; //array of path points (positions)
        public int nextPos = 0; //index to where character moves
        public GameObject characterHead;
        public GameObject characterBody;
    }


    [SerializeField] public Path[] paths;

    [Tooltip("Speed at which heads move")]
    [SerializeField] public int moveSpeed = 1;
    [SerializeField] private Camera mainCamera;

    //the original vertical lines
    private Transform[][] ogPathPositions;
    bool isDrawing;
    int newPathStart;
    Vector3 newPathStartPos;

    bool areMoving;

    //line renderer preferences
    [Header("Paths Lines")]

    [SerializeField] public float lineThickness = 0.10f;
    [SerializeField] public Material lineMaterial;
    [SerializeField] public Transform parent;


    // Start is called before the first frame update
    void Start()
    {
        //saving them to restore them and know where we can draw
        ogPathPositions = new Transform[paths.Length][];
        ogPathPositions = copyOGPathPositions(ogPathPositions, paths, 0, paths.Length - 1, 0);

        isDrawing = false;
        areMoving = false; //the characters dont start off moving

        drawPaths();
    }

    void Update()
    {
        if(areMoving)
        {
            moveCharacters();
        }
        else
        {
            checkForNewLines();
        }

    }

    private void checkForNewLines()
    {
        //if the player has clicked the screen
        if (Input.GetMouseButton(0) && !isDrawing)
        {
            checkFirstNewPathPoint();

        }
        else if (isDrawing && !Input.GetMouseButton(0))
        {
            checkEndNewPathPoint();
        }
        else if (!Input.GetMouseButton(0))
        {
            isDrawing = false;
        }
    }

    private void checkEndNewPathPoint()
    {
        //the user has let go of the button 
        isDrawing = false;

        //check if it has gotten any valid position
        for (var i = 0; i < ogPathPositions.Length; i++)
        {
            //validate that it is a path
            if (ogPathPositions[i].Length >= 2)
            {
                //error consideration for the x cordinate since it's hard to click the exact spot
                //y position clicked must be lesser that the first y (the highest) and higher than the second y (lowest).
                if (i != newPathStart && mouseInPath(ogPathPositions[i]))
                {
                    //alter current path
                    Debug.Log("new path");
                    Vector3 newPathEndPos = new Vector3(ogPathPositions[i][0].position.x, mainCamera.ScreenToWorldPoint(Input.mousePosition).y, 0);
                    alterPathsPositions(newPathStart, i, newPathStartPos, newPathEndPos);

                    //draw new path when we are done
                    drawPaths();
                }
            }
        }
    }

    private void checkFirstNewPathPoint()
    {
        Debug.Log("clocked somewhere");
        //if the player clicked on one of the vertical lines
        for (var i = 0; i < ogPathPositions.Length; i++)
        {
            //validate that it is a path
            if (ogPathPositions[i].Length >= 2)
            {
                //error consideration for the x cordinate since it's hard to click the exact spot
                //y position clicked must be lesser that the first y (the highest) and higher than the second y (lowest).
                if (mouseInPath(ogPathPositions[i]))
                {
                    isDrawing = true;
                    newPathStart = i;
                    newPathStartPos = new Vector3(ogPathPositions[i][0].position.x, mainCamera.ScreenToWorldPoint(Input.mousePosition).y, 0);
                    Debug.Log("clicked point");
                }
            }
        }
    }

    public bool mouseInPath(Transform[] ogCurPathPositions)
    {
        if ((Mathf.Abs(ogCurPathPositions[0].position.x - mainCamera.ScreenToWorldPoint(Input.mousePosition).x) < (lineThickness)) &&
            ((ogCurPathPositions[0].position.y >= mainCamera.ScreenToWorldPoint(Input.mousePosition).y) &&
            (ogCurPathPositions[1].position.y <= mainCamera.ScreenToWorldPoint(Input.mousePosition).y)))
        {
            return true;
        }

        return false;
    }

    public void alterPathsPositions(int startPath, int endPath, Vector3 startPos, Vector3 endPos)
    {
        GameObject newPointStart = new GameObject("pNew");
        newPointStart.transform.SetPositionAndRotation(startPos, new Quaternion(0, 0, 0, 0));
        newPointStart.transform.SetParent(parent);

        GameObject newPointEnd = new GameObject("pNew");
        newPointEnd.transform.SetPositionAndRotation(endPos, new Quaternion(0, 0, 0, 0));
        newPointEnd.transform.SetParent(parent);

        //find index of position in startPath that we draw after
        int i = findMiddleIndex(paths[startPath].pathPositions, startPos);
        
        //we need to save the rest of the array from here for later
        Transform[] temp = new Transform[paths[startPath].pathPositions.Length - i - 1];
        //then copy the rest of paths[startPath] to temp
        temp = addPartOfPath(temp, paths[startPath].pathPositions, i + 1, paths[startPath].pathPositions.Length - 1, 0);
        //now the "rest" of the paths[startPath] is saved and we can replace it

        //find at which point in endPath the newPoint intercepted
        int j = findMiddleIndex(paths[endPath].pathPositions, endPos);
        //intercepted between i and i + 1

        //create new array

        Transform[] newStartPathArray = new Transform[i + paths[endPath].pathPositions.Length - j + 2];
        //first the new point
        //copy first half of original
        newStartPathArray = addPartOfPath(newStartPathArray, paths[startPath].pathPositions, 0, i, 0);

        //add new points
        newStartPathArray[i + 1] = newPointStart.transform;
        newStartPathArray[i + 2] = newPointEnd.transform;

        //copy second part of the original endPath
        newStartPathArray = addPartOfPath(newStartPathArray, paths[endPath].pathPositions, j + 1, 
            paths[endPath].pathPositions.Length - 1, i + 3);

        //replace old one
        paths[startPath].pathPositions = newStartPathArray;


        //END PATH
        Transform[] newEndPathArray = new Transform[j + 1 + temp.Length + 2];
        //first part of endPath
        //copy first half of original
        newEndPathArray = addPartOfPath(newEndPathArray, paths[endPath].pathPositions, 0, j, 0);


        //add new points
        newEndPathArray[j + 1] = newPointEnd.transform;
        newEndPathArray[j + 2] = newPointStart.transform;

        //copy second part of the orifinal startPath (on temp)
        newEndPathArray = addPartOfPath(newEndPathArray, temp, 0, temp.Length - 1, i + 3);

        Debug.Log("temp: " + temp.Length);
        Debug.Log("end: " + newEndPathArray.Length);
        //assign it
        paths[endPath].pathPositions = newEndPathArray;
    }

    private int findMiddleIndex(Transform[] curPath, Vector3 curPos)
    {
        for (int i = 0; i < curPath.Length; i++)
        {
            if (curPos.x == curPath[i].position.x && curPath[i].position.y <
                curPos.y)
            {
                return i - 1;
            }
        }
        return -1;
    }

    public Transform[] addPartOfPath(Transform[] curArray, Transform[] curPath, int start, int end, int initialCounter)
    {
        int counter = initialCounter;
        for(int i = start; i <= end; i++)
        {
            curArray[counter] = curPath[i];
            counter++;
        }
        return curArray;
    }

    public Transform[][] copyOGPathPositions(Transform[][] copyPathPositions, Path[] ogPaths, int start, 
        int end, int initialCounter)
    {
        int counter = initialCounter;
        for (int i = start; i <= end; i++)
        {
            Transform[] curPositions = new Transform[ogPaths[i].pathPositions.Length];
            curPositions = addPartOfPath(curPositions, 
                ogPaths[i].pathPositions, 0, ogPaths[i].pathPositions.Length - 1, 0);
            copyPathPositions[counter] = curPositions;

            counter++;
        }
        return copyPathPositions;
    }


    public void moveCharacters()
    {
        //move through each of the paths (4)
        for (var i = 0; i < paths.Length; i++)
        {
            moveCharacterAt(paths[i]);
        }
    }

    public void moveCharacterAt(Path path)
    {
        //move to next position: curPathPositions[curNextPos].position
        GameObject curCharacterHead = path.characterHead;
        int curNextPos = path.nextPos;
        Transform[] curPathPositions = path.pathPositions;

        if (curCharacterHead.transform.position == curPathPositions[curNextPos].position)
        {
            if (curNextPos < curPathPositions.Length - 1)
            {
                path.nextPos++;
            }
        }
        else if (curNextPos < curPathPositions.Length)
        {
            Vector3 pos1 = curCharacterHead.transform.position;
            Vector3 pos2 = curPathPositions[curNextPos].position;
            //move at moveSpeed from pos1 to pos2
            curCharacterHead.transform.position = Vector3.MoveTowards(pos1, pos2, moveSpeed * Time.deltaTime);
        }
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
                    Debug.Log(i + ", Length: " + paths[i].pathPositions.Length);
                    drawLine(paths[i].pathPositions[j - 1].position, paths[i].pathPositions[j].position);

                }
            }
        }
    }


    public void drawLine(Vector3 pos1, Vector3 pos2)
    {
        //destroy previous ones
        //TODO


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