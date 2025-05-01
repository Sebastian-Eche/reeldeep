using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame(){
        SceneManager.LoadScene("PlayTest");
    }

    public void QuitGame(){
        Debug.Log("Quit!");
        Application.Quit();
    }
}
