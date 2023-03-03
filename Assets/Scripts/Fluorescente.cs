using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fluorescente : MonoBehaviour
{
    public int filas = 5;
    public int cols = 5;
    public int altura_y = 1;
    public int distancia_x = 10;
    public int distancia_z = 10;

    public GameObject[,] tubos;
    public GameObject tubo;

    // Start is called before the first frame update
    void Start()
    {
        tubos = new GameObject[filas,cols];

        for(int i=0; i<filas; i++){
            for(int j=0; j<cols; j++){
                GameObject go = Instantiate(tubo, new Vector3((float) distancia_x * i, altura_y, (float) distancia_z * j), Quaternion.identity) as GameObject;
                go.transform.localScale = new Vector3((float) 1, (float) 0.25, (float) 1);
                tubos[i,j] = go;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
