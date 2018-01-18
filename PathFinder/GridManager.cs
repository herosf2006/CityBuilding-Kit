using UnityEngine;
using System.Collections;

public class GridManager : MonoBehaviour
{
	public int 
		numOfRows, 
		numOfColumns;

	public float 
		gridCellSize, 
		gridCellWidth, 
		gridCellHeight;

	public bool 
		showGrid = true, 
		showNodes = true, 
		showLabels = true, 
		showObstacleBlocks = true, 
		nodeLabelsReady = false;

	public UILabel gridNoLabel;
	public GameObject GridLabels;
		
	public Vector3 origin = new Vector3();
	private GameObject[] obstacleArray;
	public Node[,] nodes { get; set; }
	private bool nodesReady = false;

    // cache the GridManager instance
    private static GridManager gm_Instance = null;

    //defines a static instance property to find the GridManager object     
    public static GridManager instance
    {
        get
        {
            if (gm_Instance == null)
            {                
                //returns the first GridManager object found in the scene.
                gm_Instance = FindObjectOfType(typeof(GridManager)) as GridManager;
                if (gm_Instance == null)
                    Debug.Log("GridManager object not found. \nA single GridManager should be in the scene.");
            }
            return gm_Instance;
        }
    }

	void OnApplicationQuit() //destroy instance at playmode exit
    {
        gm_Instance = null;
    }
	     
	public Vector3 Origin //used as starting point for placing all nodes
    {
        get { return origin; }
    }
	   
    void Awake()
    {
		GridLabels = GameObject.Find ("GridLabels");        
		CreateNodeGrid ();
	}

	public void UpdateObstacles()
	{ 
		UpdateObstacleArray ();
		DetectObstacles ();
	}

	public void UpdateObstacleArray()
	{
		//some of the obstacles are removed after building distruction to make the debris partially passable; 
		//called by saveloadmap after buildings load, and by helios after building destruction
		obstacleArray = GameObject.FindGameObjectsWithTag("Obstacle");
	}
	private void CreateNodeGrid()
	{
		nodes = new Node[numOfColumns, numOfRows];

		int index = 0;
		for (int i = 0; i < numOfColumns; i++)
		{
			for (int j = 0; j < numOfRows; j++)
			{
				Vector3 cellPos = GetGridCellCenter(index);
				Node node = new Node(cellPos);
				nodes[i, j] = node;
				index++;
			}
		}
		
		nodesReady = true;
	}
	private void ResetNodeGrid()
	{
		for (int i = 0; i < nodes.GetLength(0); i++)
		{
			for (int j = 0; j < nodes.GetLength(1); j++)
			{
				nodes[i, j].MarkAsFree();
			}
		}
	}
	   
    void DetectObstacles()//finds and marks the nodes as obstacles
    {      
		ShowHideLabels(); //useful for debug; useless otherwise; comment for performance in final game

		ResetNodeGrid();
      
		if (obstacleArray != null && obstacleArray.Length > 0) //Check bObstacle list for bObstacle position
        {
            foreach (GameObject data in obstacleArray)
            {
				int indexCell = GetGridIndex(data.transform.position);
                int col = GetColumn(indexCell);
                int row = GetRow(indexCell);
                nodes[row, col].MarkAsObstacle();
            }
        }
    }
    
	private void ShowHideLabels()
	{
		if(!showLabels&&nodeLabelsReady)
		{
			GridLabels.SetActive(false);
		}
		if(showLabels)
		{
			GridLabels.SetActive(true);
		}
	}

	public Vector3 GetGridCellCenter(int index) //get grid cell position in world coordinates
    {
        Vector3 cellPosition = GetGridCellPosition(index);

        cellPosition.x += (gridCellHeight / 2.0f);       
		cellPosition.y += (gridCellWidth / 2.0f);
		cellPosition.z -= 10 / (cellPosition.y + 3300);

        return cellPosition;
    }

	public Vector3 GetGridCellPosition(int index) //get grid cell position by index
    {
        int row = GetRow(index);
        int col = GetColumn(index);

		float xPosInGrid = col * gridCellWidth/2 - row * (gridCellWidth)/2;       
		float yPosInGrid = row * gridCellHeight/2 + col * (gridCellHeight)/2;		       

		return Origin + new Vector3(xPosInGrid, yPosInGrid, 0.0f);//0.0f

    }
    
	public int GetGridIndex(Vector3 pos) //get grid cell index by world position
    {    			   

		Vector2 gridPos = GetGridLocation(pos); 	
		return ((int)gridPos.x * numOfColumns + (int)gridPos.y);
    }

	private Vector2 GetGridLocation(Vector3 pos)
	{
		int row = 0;
		int col = 0;

		for (int i = 0; i < nodes.GetLength(0); i++)
		{
			for (int j = 0; j < nodes.GetLength(1); j++)
			{

				if(Vector3.Distance(nodes[i,j].position,pos)<gridCellSize)
				{
					col=i; row=j;
					break;
				}
			}
		}

		return new Vector2(col,row);
	}

	public int GetRow(int index) //get grid cell row number by index
    {
        int row = index / numOfColumns;
        return row;
    }

	public int GetColumn(int index) //get grid cell column number by index
    {
        int col = index % numOfColumns;
        return col;
    }

	public void GetNeighbours(Node node, ArrayList neighbors) //get neighour nodes in all directions
    {
        Vector3 neighborPos = node.position;
		int neighborIndex = GetGridIndex(neighborPos);

        int row = GetRow(neighborIndex);
        int column = GetColumn(neighborIndex);

        //SE
        int leftNodeRow = row - 1; int leftNodeColumn = column;
        AssignNeighbour(leftNodeRow, leftNodeColumn, neighbors);

		//NE
		leftNodeRow = row; leftNodeColumn = column + 1;
		AssignNeighbour(leftNodeRow, leftNodeColumn, neighbors);

		//NW
		leftNodeRow = row + 1; leftNodeColumn = column;
		AssignNeighbour(leftNodeRow, leftNodeColumn, neighbors);

		//SW
		leftNodeRow = row; leftNodeColumn = column - 1;
		AssignNeighbour(leftNodeRow, leftNodeColumn, neighbors);


		//S
		leftNodeRow = row - 1; leftNodeColumn = column -1;
		AssignNeighbour(leftNodeRow, leftNodeColumn, neighbors);

		//E
		leftNodeRow = row - 1; leftNodeColumn = column + 1;
		AssignNeighbour(leftNodeRow, leftNodeColumn, neighbors);

		//N
		leftNodeRow = row + 1; leftNodeColumn = column +1;
		AssignNeighbour(leftNodeRow, leftNodeColumn, neighbors);

		//W
		leftNodeRow = row + 1; leftNodeColumn = column - 1;
		AssignNeighbour(leftNodeRow, leftNodeColumn, neighbors);
	}

	void AssignNeighbour(int row, int column, ArrayList neighbors) //assigns the neighbour if free
    {
        if (row != -1 && column != -1 && row < numOfRows && column < numOfColumns)
        {
            Node nodeToAdd = nodes[row, column];
            if (!nodeToAdd.isObstacle)
            {
                neighbors.Add(nodeToAdd);
            }
        } 
    }
	   
	void OnDrawGizmos() //show grid and obstacles in editor
    {
        //grid blue lines 
        if (showGrid)
        {
            DebugDrawGrid(transform.position, numOfRows, numOfColumns, gridCellWidth, gridCellHeight, Color.blue);
        }

        //grid start position sphere
        Gizmos.DrawSphere(transform.position, 10.5f);

        //white square obstacles 
        if (showObstacleBlocks)
        {            
			Vector3 cellSize = new Vector3(gridCellSize, gridCellSize, 1.0f);
            if (obstacleArray != null && obstacleArray.Length > 0)
            {
                foreach (GameObject data in obstacleArray)
                {
					Gizmos.DrawCube(GetGridCellCenter(GetGridIndex(data.transform.position)), cellSize);
                }
            }
        }
    }
	   
    //draw grid lines
	public void DebugDrawGrid(Vector3 origin, int numRows, int numCols, float cellWidth, float cellHeight, Color color)
    {
        float width = (numCols * cellWidth);
        float height = (numRows * cellHeight);

		float correctionX = 0;
		float correctionY = 0;

		for (int i = 0; i < numRows + 1; i++) //horizontal grid lines
		{
			Vector3 startPos = origin + new Vector3(- correctionX, - correctionY, 0.0f) + i * cellHeight * new Vector3( 0.0f, 1.0f, 0.0f);
			Vector3 endPos = startPos + height * new Vector3(0.7074f, 0.5f, 0.0f);
			Debug.DrawLine(startPos, endPos, color);
		
			correctionX += cellWidth/2;
			correctionY += cellHeight/2;
		}

		correctionX = 0;
		correctionY = 0;

		for (int i = 0; i < numCols + 1; i++) //vertial grid lines
		{
			Vector3 startPos = origin + new Vector3(- correctionX, correctionY, 0.0f) + i * cellWidth * new Vector3(1.0f, 0.0f, 0.0f);
			Vector3 endPos = startPos + width * new Vector3(-0.499849f, 0.3535f, 0.0f);
			
			Debug.DrawLine(startPos, endPos, color);

			correctionX += cellWidth/2;
			correctionY += cellHeight/2;

		}

		if(showNodes && nodesReady) //detection areas
		{
			for (int i = 0; i < nodes.GetLength(0); i++)
			{
				for (int j = 0; j < nodes.GetLength(1); j++)
				{	
					Gizmos.DrawWireSphere( nodes[i,j].position, gridCellSize);
					if(showLabels)
					DrawLabels();

				}
			}

		}
    }

	private void DrawLabels() //node labels  (col x row)
	{
		if(!nodeLabelsReady)
		{
			for (int i = 0; i < nodes.GetLength(0); i++)
			{
				for (int j = 0; j < nodes.GetLength(1); j++)
				{
					GameObject.Instantiate(gridNoLabel, nodes[i,j].position+new Vector3(0,30,0), Quaternion.identity);
					GameObject[] gridNoLabels = GameObject.FindGameObjectsWithTag("GridLabel");
			
					foreach (GameObject label in gridNoLabels) 
					{
						if(((Selector)label.GetComponent("Selector")).isSelected)
						{
							label.transform.parent=GridLabels.transform;
							((UILabel)label.GetComponent("UILabel")).text=i.ToString()+"x"+ j.ToString();
							((Selector)label.GetComponent("Selector")).isSelected = false;
							break;
						}						
					}
				}
			}

			nodeLabelsReady = true;
		}		
	}
}
