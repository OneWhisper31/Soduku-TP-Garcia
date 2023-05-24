using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DELETE : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int[,] xd = new int[15,16];
        for (int i = 0; i < xd.GetLength(0); i++)
        {
            for (int j = 0; j < xd.GetLength(1); j++)
            {
                xd[i,j] = Random.Range(1, 32);
            }
        }
        var matrix =new Matrix<int>(Tests.validBoards[0]);
    }
}
