using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class WumpusAgent : MonoBehaviour
{

    /* agent goal:
     * find treasure 
     * avoid pit
     * avoid wumpus
    */


    public WumpusWorld world;

    //start position
    public int row = 0;
    public int col = 0;

    public Transform pip;

    public int nextCol;
    public int nextRow;

    public int iters = 0;  //iterations, public, just so I can kep an eye on it.

    public bool gameOver = false;

    // Start is called before the first frame update
    void Start()
    {

        pip.position = world.GetCellPosition(row, col) + Vector3.up;

    }

    // Update is called once per frame
    void Update()
    {
        if(gameOver)
        {

            //fanfare
            return;

        }

        if(Input.GetKeyDown(KeyCode.W))
        {
            //iterate one move/cycle
            Perceive();
            
            //do the move 
            Move();

            //check my new cell
            Check();
        }
    }

    void Perceive()
    {
        //check my neighborhood

        int leftcell, rightcell, topcell, bottomcell;
        int leftcellContent, rightcellContent, topcellContent, bottomcellContent, thiscellContent;


        leftcell = col - 1;
        rightcell = col + 1;
        topcell = row + 1;
        bottomcell = row - 1;

        //create a picture of my world
        leftcellContent = world.GetCellContent(row, leftcell);
        rightcellContent = world.GetCellContent(row, rightcell);
        topcellContent = world.GetCellContent(topcell, col);
        bottomcellContent = world.GetCellContent(bottomcell, col);
        thiscellContent = world.GetCellContent(row, col);

        if(leftcellContent >= 0 )
        {
            if(leftcellContent == WumpusWorld.TREASURE)
            {
                nextCol = leftcell;
                nextRow = row;
                return;
            }
            if( leftcellContent != WumpusWorld.WUMPUS && 
                leftcellContent != WumpusWorld.PIT    &&
                leftcellContent != WumpusWorld.AGENT   )
            {
                if(leftcellContent == WumpusWorld.EMPTY)
                {
                    //safest move
                    nextCol = leftcell;
                    nextRow = row;
                    return;
                }
            }
        }
        if (rightcellContent >= 0)
        {
            if (rightcellContent == WumpusWorld.TREASURE)
            {
                nextCol = rightcell;
                nextRow = row;
                return;

            }

            if (rightcellContent != WumpusWorld.WUMPUS &&
                rightcellContent != WumpusWorld.PIT &&
                rightcellContent != WumpusWorld.AGENT)
            {
                if (rightcellContent == WumpusWorld.EMPTY)
                {
                    //safest move
                    nextCol = rightcell;
                    nextRow = row;
                    return;
                }
            }

        }
        if (topcellContent >= 0)
        {
            if (topcellContent == WumpusWorld.TREASURE)
            {
                nextCol = col;
                nextRow = topcell;
                return;

            }

            if (topcellContent != WumpusWorld.WUMPUS &&
                topcellContent != WumpusWorld.PIT &&
                topcellContent != WumpusWorld.AGENT)
            {
                if (topcellContent == WumpusWorld.EMPTY)
                {
                    //safest move
                    nextCol = col;
                    nextRow = topcell;
                    return;
                }
            }


        }
        if (bottomcellContent >= 0)
        {
            if (bottomcellContent == WumpusWorld.TREASURE)
            {
                nextCol = col;
                nextRow = bottomcell;
                return;

            }

            if (bottomcellContent != WumpusWorld.WUMPUS &&
                bottomcellContent != WumpusWorld.PIT &&
                bottomcellContent != WumpusWorld.AGENT)
            {
                if (bottomcellContent == WumpusWorld.EMPTY)
                {
                    //safest move
                    nextCol = col;
                    nextRow = bottomcell;
                    return;
                }
            }
        }

        //final case, no prefered next tile, so choose a random
        
        iters = 0;                  //reset iters to keep track of the tight loop
        
        while(iters < 128)          //excluding a quantum singularity, I should hit it well under 128
        {

            iters++;

            int rndcell = Random.Range(0, 4);

            if (rndcell == 0)
            {
                //look up

                if (topcellContent != WumpusWorld.WUMPUS        &&
                    topcellContent != WumpusWorld.PIT           &&
                    topcellContent != WumpusWorld.AGENT         &&
                    topcellContent != WumpusWorld.BAD_SUSHI      )
                {
                    nextCol = col;
                    nextRow = topcell;

                    return;
                }


            }

            if (rndcell == 1)
            {
                //lookdown

                if (bottomcellContent != WumpusWorld.WUMPUS    &&
                    bottomcellContent != WumpusWorld.PIT       &&
                    bottomcellContent != WumpusWorld.AGENT     &&
                    bottomcellContent != WumpusWorld.BAD_SUSHI  )
                {
                    nextCol = col;
                    nextRow = bottomcell;

                    return;
                }
            }

            if (rndcell == 2)
            {

                //look left

                if (leftcellContent != WumpusWorld.WUMPUS       &&
                    leftcellContent != WumpusWorld.PIT          &&
                    leftcellContent != WumpusWorld.AGENT        &&
                    leftcellContent != WumpusWorld.BAD_SUSHI    )
                {
                    nextCol = leftcell;
                    nextRow = row;

                    return;
                }

            }
            if (rndcell == 3)
            {
                //look right

                if (rightcellContent != WumpusWorld.WUMPUS      &&
                    rightcellContent != WumpusWorld.PIT         &&
                    rightcellContent != WumpusWorld.AGENT       &&
                    rightcellContent != WumpusWorld.BAD_SUSHI   )
                {
                    nextCol = rightcell;
                    nextRow = row;

                    return;
                }
            }

        }

        Debug.Log("QUANTUM SINGULARITY!!!");
       
    }
    void Move()
    {
        
        //set visited
        world.SetCellVisited(row, col);
        
        //based on perception, move one cell
        row = nextRow;
        col = nextCol;

        //move the pip
        pip.position = world.GetCellPosition(row, col) + Vector3.up;

        //expose the tile
        world.ExposeCell(row, col);

    }
    void Check()
    {
        //check what is here
        if(world.GetCellContent(row, col) == WumpusWorld.TREASURE)
        {

            Debug.Log("YOU WIN!!!");


        }

    }


}
