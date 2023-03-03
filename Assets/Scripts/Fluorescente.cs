using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fluorescente : MonoBehaviour
{
    public float pos_x = 11.31624f;
    public float pos_y = 18.27761f;
    public float pos_z = -12.8423f;
    public int filas = 25;
    public int cols = 25;
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
                GameObject go = Instantiate(tubo, new Vector3((float) pos_x + distancia_x * i, pos_y + altura_y, (float) pos_z + distancia_z * j), Quaternion.Euler(90,0,0)) as GameObject;
                go.transform.localScale = new Vector3(1f, 1f, 0.25f);
                tubos[i,j] = go;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
