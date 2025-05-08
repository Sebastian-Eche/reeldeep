using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame(){
        SceneManager.LoadScene("New New Demo");

    }

    public void QuitGame(){
        Debug.Log("Quit!");
        Application.Quit();
    }
}
