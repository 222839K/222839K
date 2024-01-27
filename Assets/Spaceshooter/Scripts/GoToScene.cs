using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToScene : MonoBehaviour
{
    public void GotoScene(string scenename)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(scenename);
    }
}
