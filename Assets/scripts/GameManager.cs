using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public void NextScene ()
    {
	    int currentScene = SceneManager.GetActiveScene ().buildIndex;
        int nextScene = currentScene + 1;
        if (nextScene >= SceneManager.sceneCountInBuildSettings)
        {
            nextScene = 0;
        }

        SceneManager.LoadScene (nextScene);
    }

    

}
