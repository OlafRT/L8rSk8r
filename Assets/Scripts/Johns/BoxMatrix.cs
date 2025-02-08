using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BoxMatrix : MonoBehaviour
{
    //define
    const int W = 16;
    const int D = 16;
    const int H = 16;

    //allocate
    public GameObject[,,] MatrixObjs = new GameObject[W,D,H];

    //clone
    public GameObject baseObject;


    // Start is called before the first frame update
    void Start()
    {
        //assuming a unit object (box) with a scalar, otherwise need object dimensions
        float spaceW = baseObject.transform.localScale.x; 
        float spaceD = baseObject.transform.localScale.z;
        float spaceH = baseObject.transform.localScale.y;

        for (int w = 0; w < W; w++) 
        {
            for (int d = 0; d < D; d++)
            {
                for (int h = 0; h < H; h++)
                {
                    MatrixObjs[w,d,h] = GameObject.Instantiate(baseObject,transform); //child of this             

                    Vector3 pos = new Vector3(w * spaceW, h * spaceH, d * spaceD);  //place it

                    MatrixObjs[w, d, h].transform.localPosition = pos;              //LOCAL
                    
                    MatrixObjs[w, d, h].SetActive(true);                            // baseObject is deactivated in editor

                }
            }

        }
    }
}
