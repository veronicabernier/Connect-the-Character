using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PathConnections : MonoBehaviour
{
    [System.Serializable]
    public class Path
    {
        public Transform[] ogVerticalPoints = new Transform[2];
        public LinkedList<Transform> curPoints;
        public LinkedListNode<Transform> nextPoint; //index to where character moves
        public bool isMoving = false;

        //public Transform[] pathPositions; //array of path points (positions)
        //public int nextPos = 0; //index to where character moves
        public GameObject characterHead;
        public GameObject characterBody;
    }


    [SerializeField] public Path[] paths;

    [Tooltip("Speed at which heads move")]
    [SerializeField] public int moveSpeed = 1;
    [SerializeField] private Camera mainCamera;


    bool isDrawing;
    int newPathStart;
    Vector3 newPathStartPos;


    [SerializeField] public bool areMoving;
    [SerializeField] public Transform pointsParent;

    //line renderer preferences
    [Header("Paths Lines")]

    [SerializeField] public float lineThickness = 0.10f;
    [SerializeField] public Material lineMaterial;
    [SerializeField] public Transform linesParent;


    // Start is called before the first frame update
    void Start()
    {
        //filling up curPositions of each path
        initializeCurPoints();

        isDrawing = false;
        areMoving = false; //the characters dont start off moving

        drawPaths();

        //after 10 seconds characters will start moving
        Invoke("startMovingCharacters", 10);

        Debug.Log("paths drawn");
    }

    private void startMovingCharacters()
    {
        initializeNextPoints();
        areMoving = true;
    }

    private void initializeCurPoints()
    {
        for (int i = 0; i < paths.Length; i++)
        {
            paths[i].curPoints = new LinkedList<Transform>();
            paths[i].curPoints.AddLast(paths[i].ogVerticalPoints[0]);
            paths[i].curPoints.AddLast(paths[i].ogVerticalPoints[1]);
        }
    }

    private void initializeNextPoints()
    {
        for (int i = 0; i < paths.Length; i++)
        {
            paths[i].nextPoint = paths[i].curPoints.First;
        }
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
            checkFirstNewPoint();

        }
        else if (isDrawing && !Input.GetMouseButton(0))
        {
            checkEndNewPoint();
        }
        else if (!Input.GetMouseButton(0))
        {
            isDrawing = false;
        }
    }

    private void checkFirstNewPoint()
    {
        Debug.Log("clocked somewhere");
        //if the player clicked on one of the vertical lines
        for (var i = 0; i < paths.Length; i++)
        {
            //y position clicked must be lesser that the first y (the highest) and higher than the second y (lowest).
            if (mouseInPath(paths[i]))
            {
                isDrawing = true;
                newPathStart = i;
                newPathStartPos = new Vector3(paths[i].ogVerticalPoints[0].position.x, mainCamera.ScreenToWorldPoint(Input.mousePosition).y, 0);
                Debug.Log("clicked point");
            }
        }
    }

    private void checkEndNewPoint()
    {
        //the user has let go of the button 
        isDrawing = false;

        //check if it has gotten any valid position
        for (var i = 0; i < paths.Length; i++)
        {
            //error consideration for the x cordinate since it's hard to click the exact spot
            //y position clicked must be lesser that the first y (the highest) and higher than the second y (lowest).
            if (i != newPathStart && mouseInPath(paths[i]))
            {
                //alter current path
                Debug.Log("new path");
                int newPathEnd = i;
                Vector3 newPathEndPos = new Vector3(paths[i].ogVerticalPoints[0].position.x, mainCamera.ScreenToWorldPoint(Input.mousePosition).y, 0);
                alterPathsPositions(newPathStart, newPathEnd, newPathStartPos, newPathEndPos);

                //draw new path when we are done
                drawPaths();
            }
        }
    }

    public bool mouseInPath(Path curPath)
    {
        if ((Mathf.Abs(curPath.ogVerticalPoints[0].position.x - mainCamera.ScreenToWorldPoint(Input.mousePosition).x) < (lineThickness)) &&
            ((curPath.ogVerticalPoints[0].position.y >= mainCamera.ScreenToWorldPoint(Input.mousePosition).y) &&
            (curPath.ogVerticalPoints[1].position.y <= mainCamera.ScreenToWorldPoint(Input.mousePosition).y)))
        {
            return true;
        }

        return false;
    }

    public void alterPathsPositions(int startPath, int endPath, Vector3 startPos, Vector3 endPos)
    {
        GameObject newPointStart = new GameObject("pNew");
        newPointStart.transform.SetPositionAndRotation(startPos, new Quaternion(0, 0, 0, 0));
        newPointStart.transform.SetParent(pointsParent);

        GameObject newPointEnd = new GameObject("pNew");
        newPointEnd.transform.SetPositionAndRotation(endPos, new Quaternion(0, 0, 0, 0));
        newPointEnd.transform.SetParent(pointsParent);

        //find nodes of positions in middle of new points for both paths
        LinkedListNode<Transform> midNodeStart = findMiddleIndex(paths[startPath].curPoints, startPos);
        LinkedListNode<Transform> midNodeEnd = findMiddleIndex(paths[endPath].curPoints, endPos);

        //New nodes created to add to the current points on each paths
        LinkedListNode<Transform> newNodeStart1 = new LinkedListNode<Transform>(newPointStart.transform);
        LinkedListNode<Transform> newNodeStart2 = new LinkedListNode<Transform>(newPointStart.transform);
        LinkedListNode<Transform> newNodeEnd1 = new LinkedListNode<Transform>(newPointEnd.transform);
        LinkedListNode<Transform> newNodeEnd2 = new LinkedListNode<Transform>(newPointEnd.transform);


        //new linked list with first elements before middle (of start path)
        LinkedList<Transform> newStartPathPoints = linkedListUntilMid(paths[startPath].curPoints, midNodeStart);
        //set startPath's midNodeStart's next to the new points (with "1" on variable)
        newStartPathPoints.AddLast(newNodeStart1);
        newStartPathPoints.AddLast(newNodeEnd1);
        //add the nodes from after the middle of the end path
        linkedListFromMid(newStartPathPoints, paths[endPath].curPoints, midNodeEnd.Next);



        //new linked list with first elements before middle (of end path)
        LinkedList<Transform> newEndPathPoints = linkedListUntilMid(paths[endPath].curPoints, midNodeEnd);
        //set endPath's midNodeEnd's next to the new points (with "2" on variable)
        newEndPathPoints.AddLast(newNodeEnd2);
        newEndPathPoints.AddLast(newNodeStart2);
        //add the nodes from after the middle of the end path
        linkedListFromMid(newEndPathPoints, paths[startPath].curPoints, midNodeStart.Next);


        //assign new variables to old ones
        paths[startPath].curPoints = newStartPathPoints;
        paths[endPath].curPoints = newEndPathPoints;

    }

    private LinkedList<Transform> linkedListUntilMid(LinkedList<Transform> curPoints, LinkedListNode<Transform> midNode)
    {
        LinkedListNode<Transform> curPointNode = curPoints.First;
        LinkedList<Transform> newPoints = new LinkedList<Transform>();

        while (curPointNode != midNode.Next)
        {
            //copy only the value so it doesn't point to where it was originally pointing
            newPoints.AddLast(new LinkedListNode<Transform>(curPointNode.Value));
            curPointNode = curPointNode.Next;
        }
        return newPoints;
    }

    private LinkedList<Transform> linkedListFromMid(LinkedList<Transform> curList, LinkedList<Transform> curPoints, LinkedListNode<Transform> midNode)
    {
        LinkedListNode<Transform> curPointNode = curPoints.First;

        while (curPointNode != midNode)
        {
            curPointNode = curPointNode.Next;
        }
        while (curPointNode != null)
        {
            //copy only the value so it doesn't point to where it was originally pointing
            curList.AddLast(new LinkedListNode<Transform>(curPointNode.Value));
            curPointNode = curPointNode.Next;
        }

        return curList;
    }

    private LinkedListNode<Transform> findMiddleIndex(LinkedList<Transform> curPoints, Vector3 curPos)
    {
        LinkedListNode<Transform> curPointNode = curPoints.First; 

        while (curPointNode.Next.Value.position.y > curPos.y && curPos.x != curPointNode.Next.Value.position.x)
        {
            curPointNode = curPointNode.Next;
        }
        return curPointNode;
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
        GameObject characterHead = path.characterHead;
        LinkedListNode<Transform> curNext = path.nextPoint;


        //check if it already reached the position
        if (characterHead.transform.position == curNext.Value.position)
        {
            if (path.nextPoint.Next != null)
            {
                path.nextPoint = path.nextPoint.Next;
                path.isMoving = true;
            }
            else
            {
                path.isMoving = false;
            }
        }
        else
        {
            Vector3 curPos = characterHead.transform.position;
            Vector3 nextPos = curNext.Value.position;

            //move character's head towards the next position at speed moveSpeed
            characterHead.transform.position = Vector3.MoveTowards(curPos, nextPos, moveSpeed * Time.deltaTime);
        }
    }


    public void drawPaths()
    {
        destroyLines();

        //draw all n (or 4) paths
        for (var i = 0; i < paths.Length; i++)
        {
            drawPathAt(paths[i]);
        }
    }

    private void drawPathAt(Path path)
    {
        LinkedListNode<Transform> curPointNode = path.curPoints.First;

        //connect each two points with a line
        while (curPointNode.Next != null)
        {
            drawLine(curPointNode.Value.position, curPointNode.Next.Value.position);
            curPointNode = curPointNode.Next;
        }
    }

    private void destroyLines()
    {
        //destroy all children of linesParent
        foreach(Transform child in linesParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void drawLine(Vector3 pos1, Vector3 pos2)
    {

        LineRenderer lineRenderer = new GameObject("line").AddComponent<LineRenderer>();
        lineRenderer.transform.SetParent(linesParent);
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = lineThickness;
        lineRenderer.endWidth = lineThickness;
        lineRenderer.material = lineMaterial;
        lineRenderer.SetPosition(0, pos1);
        lineRenderer.SetPosition(1, pos2);
    }


}