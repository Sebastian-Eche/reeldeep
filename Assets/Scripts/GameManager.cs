using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set;}
    private bool minigameStart = false;
    private Fish[] fishCaught;
    void Awake()
    {
        if (Instance == null){
            Instance = this;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void StartMinigame(){
        minigameStart = true;
    }

    void EndMinigame(){
        minigameStart = false;
    }
}
