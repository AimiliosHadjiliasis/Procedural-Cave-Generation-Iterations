using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeSkin : MonoBehaviour
{
    public Material[] material;
    Renderer rend;

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.enabled = true;
        rend.sharedMaterial = material[0];
    }

    // Update is called once per frame
    void Update()
    {
        //Method that every time we press the left click to update the scene
        //it changes the skin of the map randomly
        if (Input.GetMouseButtonDown(0))
        {
            int randNum = UnityEngine.Random.Range(1, 10);
            if (randNum < 4)
            {
                rend.sharedMaterial = material[0];
            }
            else if(randNum >= 4 && randNum <=6)
            {
                rend.sharedMaterial = material[1];
            }
            else
            {
                rend.sharedMaterial = material[2];
            }
        }
    }
}
