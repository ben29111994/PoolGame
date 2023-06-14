using UnityEngine;
using UnityEditor;
using System.Collections;

public class BallsSortManager: EditorWindow
{
    [MenuItem ("Window/Ball Pool/Create and Sort Balls")]
    static void Init ()
    {
        CueController.FindObjectOfType<CueController>().CreateAndSortBalls();
    }
}
