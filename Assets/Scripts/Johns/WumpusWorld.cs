using UnityEngine;

public class WumpusWorld : MonoBehaviour
{
    private const int ROWS = 4;
    private const int COLS = 4;


    //should do a typedef enum but rather I shall do it in simplest terms
    public const int BAD_SUSHI = -1;
    public const int EMPTY = 0;
    public const int BREEZE = 1;
    public const int STENCH = 2;
    public const int TREASURE = 3;
    public const int PIT = 4;
    public const int WUMPUS = 5;
    public const int AGENT = 6;


    public GameObject baseobject;
    private readonly GameObject[,] objs = new GameObject[ROWS, COLS];

    private void Awake()
    {
        Build();
        StartGame();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public int GetCellContent(int row, int col)
    {
        int content;


        // sanity check!
        if (row < 0)
            return -1;
        if (col < 0)
            return -1;

        if (row >= ROWS)
            return -1;
        if (col >= COLS)
            return -1;


        content = objs[row, col].GetComponent<WumpusData>().cellContents;

        return content;
    }

    public Vector3 GetCellPosition(int row, int col)
    {
        // sanity check!
        if (row < 0)
            return Vector3.zero;
        if (col < 0)
            return Vector3.zero;
        if (row >= ROWS)
            return Vector3.zero;
        if (col >= COLS)
            return Vector3.zero;


        return objs[row, col].transform.position;
    }

    private void Build()
    {
        for (var row = 0; row < ROWS; row++)
        for (var col = 0; col < COLS; col++)
        {
            objs[row, col] = Instantiate(baseobject, transform);
            var space = baseobject.transform.localScale.x;
            var pos = new Vector3(row * space, 0.5f, col * space);

            objs[row, col].transform.localPosition = pos;

            // get the data object (script component) of the game object 
            var data = objs[row, col].GetComponent<WumpusData>();
            data.row = row;
            data.col = col;

            //set up this cell
            ApplyInitialState(objs[row, col]);
        }
    }

    //apply our creation rules to this cell
    private void ApplyInitialState(GameObject thisobj)
    {
        thisobj.SetActive(true);

        // get the data object (script component) of the game object 
        var data = thisobj.GetComponent<WumpusData>();

        //apply hard rules as to where things are, based on the classic demo
        // row 0 (1)
        if (data.row == 0 && data.col == 0)
        {
            data.cellContents = AGENT;
            return;
        }

        if (data.row == 0 && data.col == 1)
        {
            data.cellContents = BREEZE;
            return;
        }

        if (data.row == 0 && data.col == 2)
        {
            data.cellContents = PIT;
            return;
        }

        if (data.row == 0 && data.col == 3)
        {
            data.cellContents = BREEZE;
            return;
        }

        // row 1 (2)
        if (data.row == 1 && data.col == 0)
        {
            data.cellContents = STENCH;
            return;
        }

        if (data.row == 1 && data.col == 1)
        {
            data.cellContents = EMPTY;
            return;
        }

        if (data.row == 1 && data.col == 2)
        {
            data.cellContents = BREEZE;
            return;
        }

        if (data.row == 0 && data.col == 3)
        {
            data.cellContents = EMPTY;
            return;
        }

        // row 2 (3)
        if (data.row == 2 && data.col == 0)
        {
            data.cellContents = WUMPUS;
            return;
        }

        if (data.row == 2 && data.col == 1)
        {
            data.cellContents = TREASURE;
            return;
        }

        if (data.row == 2 && data.col == 2)
        {
            data.cellContents = PIT;
            return;
        }

        if (data.row == 2 && data.col == 3)
        {
            data.cellContents = BREEZE;
            return;
        }

        // row 3 (4)
        if (data.row == 3 && data.col == 0)
        {
            data.cellContents = STENCH;
            return;
        }

        if (data.row == 3 && data.col == 1)
        {
            data.cellContents = EMPTY;
            return;
        }

        if (data.row == 3 && data.col == 2)
        {
            data.cellContents = BREEZE;
            return;
        }

        if (data.row == 3 && data.col == 3) data.cellContents = PIT;
    }

    private void StartGame()
    {
        //Activate player
        objs[0, 0].GetComponent<WumpusData>().Expose(true);


        //if we want to debug
        // if (false)
        // {
        //     for (int row = 0; row < ROWS; row++)
        //     {
        //         for (int col = 0; col < COLS; col++)
        //         {
        //             objs[row, col].GetComponent<WumpusData>().Expose(true);
        //
        //         }
        //     }
        //
        // }
    }

    public void ExposeCell(int row, int col)
    {
        objs[row, col].GetComponent<WumpusData>().Expose(true);
    }

    public void SetCellVisited(int row, int col)
    {
        objs[row, col].GetComponent<WumpusData>().cellContents = AGENT;
    }
}