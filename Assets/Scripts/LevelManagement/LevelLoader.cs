using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour {

	public static IEnumerator UnloadLevel(int sceneIndex)
    {
        AsyncOperation ao = SceneManager.UnloadSceneAsync(sceneIndex);
        while (!ao.isDone)
            yield return null;

    }
}
