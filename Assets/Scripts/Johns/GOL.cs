using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOL : MonoBehaviour
{

    [SerializeField]  GameObject baseobject;
    [SerializeField] float interval = 0.5f;

    const int MAX_ROWS = 32;
    const int MAX_COLUMNS = 32;

    //2d array of ints
    int[,] cells;
    GameObject[,] objs;

    float timer = -1;

    // Start is called before the first frame update
    void Start()
    {
        timer = Time.realtimeSinceStartup;

        Random.InitState(987654321);

        //redimension our array
        cells = new int[MAX_ROWS, MAX_COLUMNS];
        objs = new GameObject[MAX_ROWS, MAX_COLUMNS];

        //deactivate the base object, it is there only for cloning, and to see it in design
        baseobject.SetActive(false);
        
        for (int row = 0; row < MAX_ROWS; row++)
        {
            for (int col = 0; col < MAX_COLUMNS; col++)
            {
                //create cells
                objs[row, col] = GameObject.Instantiate(baseobject, transform);
                float space = baseobject.transform.localScale.x;
                Vector3 pos = new Vector3(row * space, 20, col * space );                

                objs[row, col].transform.position = pos;

                //find the ground
                int layerMask = 1 << 6; //ground
                RaycastHit hit;
                
                // Does the ray intersect any surface in the layer mask
                if (Physics.Raycast(pos, Vector3.down, out hit, 10000, layerMask))
                {
                                   
                    float x = objs[row, col].transform.localPosition.x;
                    float y = hit.point.y;
                    float z = objs[row, col].transform.localPosition.z;

                   
                }

                //init cells
                int state =  Random.Range(0, 2);   // here I init more cells live than dead. 
                if (state >= 1)
                {
                    objs[row, col].SetActive(true);
                    cells[row, col] = 1;
                }                    
                else
                {
                    objs[row, col].SetActive(false);
                    cells[row, col] = 0;
                }

            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.realtimeSinceStartup - timer > interval)
        {
            generateGOL();
            timer = Time.realtimeSinceStartup;
        }      
        
    }

    //next generation game of life
    void generateGOL()
    {
        int[,] next = new int[MAX_ROWS, MAX_COLUMNS];

        // Loop through every spot in our 2D array and check spots neighbors
        for (int row = 0; row < MAX_ROWS ; row++)
        {
            

            for (int col = 0; col < MAX_COLUMNS ; col++)
            {


                //check my neigbors, so up , down, left, right, and diagonal
                //this is what we need to do, but need to wrap around the array
                int rowminus = row - 1;
                int rowplus = row + 1;
                int colminus = col - 1;
                int colplus = col + 1;

                if (rowminus < 0) rowminus = MAX_ROWS - 1;
                if (rowplus == MAX_ROWS) rowplus = 0;

                if (colminus < 0) colminus = MAX_COLUMNS - 1;
                if (colplus == MAX_COLUMNS) colplus = 0;

                // Add up all the states in a 3x3 surrounding grid, not including where i am now
                int neighbors = 0;

                neighbors += cells[rowminus, col];
                neighbors += cells[rowplus, col];
                neighbors += cells[row, colminus];
                neighbors += cells[row, colplus];
                neighbors += cells[rowplus, colplus];
                neighbors += cells[rowplus, colminus];
                neighbors += cells[rowminus, colplus];
                neighbors += cells[rowminus, colminus];

                // Rules of Life
                if ((cells[row, col] == 1) && (neighbors < 2))				// Loneliness 
                    next[row, col] = 0;
                else if ((cells[row, col] == 1) && (neighbors > 3))         // Overpopulation
                    next[row, col] = 0;
                else if ((cells[row, col] == 0) && (neighbors == 3))        // Reproduction 
                    next[row, col] = 1;
                else                                                        // Stasis         
                    next[row, col] = cells[row, col];
            }
        }

        //now swap new values for old
        for (int row = 0; row < MAX_ROWS ; row++)
        {
            for (int col = 0; col < MAX_COLUMNS ; col++)
            {

                cells[row, col] = next[row, col];

                if (cells[row, col] == 1)
                    objs[row, col].SetActive(true);
                else
                    objs[row, col].SetActive(false);                
                
            }
        }

    }
}
