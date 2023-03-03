using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public GameObject canvas1;

    private void OnTriggerEnter(Collider player)
    {
        if(player.name == "PlayerCollider")
        {
            canvas1.SetActive(true);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
