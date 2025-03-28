using UnityEngine;

public class Minigame : MonoBehaviour
{
    private void OnEnable()
    {
        Fish.OnFishHooked += StartMinigame;
    }

    void OnDisable()
    {
        Fish.OnFishHooked -= StartMinigame;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void StartMinigame(Fish hookedFish){
        Debug.Log("FISH MINIGAME STARTS");
    }
}
