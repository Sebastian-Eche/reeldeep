using System;
using System.Collections.Generic;
using TMPro;
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
    public int maxFishCapacity = 2;
    public event Action OnMaxFishCapacity;
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
        hookControllerObject.isPaused = true;
        // fishMovementObject.enabled = false;
    }

    public void EndMinigame(){
        minigameStart = false;
        cameraFollowObject.enabled = true;
        // hookControllerObject.enabled = true;
        hookControllerObject.isPaused = false;
        // fishMovementObject.enabled = true;
    }

    public void AddFish(Fish caughtFish){
        fishCaught.Add(caughtFish);
        Debug.Log(fishCaught + " Fish is Caught");
        Debug.Log(fishCaught.Count);
        if(MaxCapcityOfFishReached()){
            Debug.Log("Max capacity reached condition TRUE");
            if(OnMaxFishCapacity != null){
                Debug.Log("EVENT TRIGGERED");
                OnMaxFishCapacity.Invoke();
            }else{
                Debug.Log("EVENT NOT TRIGGERED - EVENT IS NULL");
            }
        }
    }

    public void RemoveFish(Fish fish){}

    public int CurrentFishCapacity(){
        return fishCaught.Count;
    }

    private bool MaxCapcityOfFishReached(){
        if (fishCaught.Count >= maxFishCapacity){
            Debug.Log("Returning back to Boat");
            return true;
        }
        return false;
    }
}
