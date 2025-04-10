using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set;}
    public CameraFollow cameraFollowObject;
    public HookController hookControllerObject;
    public FishMovement fishMovementObject;
    public bool minigameStart = false;
    public bool fishOnHook = false;
    private List<Fish> fishCaught = new List<Fish>();
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

    public void StartMinigame(){
        minigameStart = true;
        cameraFollowObject.enabled = false;
        hookControllerObject.enabled = false;
        // fishMovementObject.enabled = false;
    }

    public void EndMinigame(){
        minigameStart = false;
        cameraFollowObject.enabled = true;
        hookControllerObject.enabled = true;
        // fishMovementObject.enabled = true;
    }

    public void AddFish(Fish caughtFish){
        fishCaught.Add(caughtFish);
        Debug.Log(fishCaught + " Fish is Caught");
        Debug.Log(fishCaught.Count);
    }

    public void RemoveFish(Fish fish){}

    public int FishCaught(){
        return fishCaught.Count;
    }
}
