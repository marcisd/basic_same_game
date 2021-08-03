using System.Collections;
using System.Collections.Generic;
using MSD.BasicSameGame.GameLogic;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SameGame test = new SameGame(new Vector2Int(10, 10), 5, 3);
        test.Initialize();
        Debug.Log("t");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
