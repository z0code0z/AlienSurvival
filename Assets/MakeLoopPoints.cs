using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MakeLoopPoints : MonoBehaviour
{


    [MenuItem("GameObject/CREATE IT")]
    static void Create()
    {
        GameObject ObjToSpawn = new GameObject();

        if (Selection.activeTransform != null)
        {
            for (int Y = 0; Y < 100; Y += 8)
            {
                for (int X = 0; X < 200; X += 8)
                {
                    for (int Z = 0; Z < 200; Z += 8)
                    {

                        Instantiate(ObjToSpawn, new Vector3(X, Y, Z), Quaternion.identity, Selection.activeTransform);

                    }
                }
            }
        }
    }






}
