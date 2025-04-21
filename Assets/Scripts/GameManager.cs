using System;
using System.Collections;
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
    public int maxAttempts = 2;
    public event Action OnMaxAttemptsMade;
    public GameObject returnPoint;
    public TextMeshProUGUI fishSpeciesText;
    public TextMeshProUGUI fishMetaDataText;
    public bool isDisplayingInfo = false;
    public int attemptsToCatch = 0;
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
        if(MaxAttmeptsReached()){
            Debug.Log("Max capacity reached condition TRUE");
            if(OnMaxAttemptsMade != null){
                Debug.Log("EVENT TRIGGERED");
                OnMaxAttemptsMade.Invoke();
            }else{
                Debug.Log("EVENT NOT TRIGGERED - EVENT IS NULL");
            }
        }
    }

    public int CurrentFishCapacity(){
        return fishCaught.Count;
    }

    public bool MaxAttmeptsReached(){
        if (attemptsToCatch >= maxAttempts){
            Debug.Log("Returning back to Boat");
            return true;
        }
        return false;
    }

    public IEnumerator DisplayCaughtFish(){
        fishSpeciesText.gameObject.SetActive(true);
        fishMetaDataText.gameObject.SetActive(true);
        for (int i = 0; i < fishCaught.Count; i++){
            fishSpeciesText.text = "You caught a \n " + fishCaught[i].fishInfo.fishSpecies;
            fishMetaDataText.text = "Length: " + fishCaught[i].GetLength() + "in" + " Weight: " + fishCaught[i].GetWeight() + "lbs";
            yield return new WaitForSeconds(3);
        }
        fishSpeciesText.gameObject.SetActive(false);
        fishMetaDataText.gameObject.SetActive(false);
        isDisplayingInfo = false;
        attemptsToCatch = 0;
        fishCaught = new List<Fish>();
    }

    public void IncrementAttempts(){
        attemptsToCatch++;
    }

    public void ReturningToBoat(){
        if(MaxAttmeptsReached()){
            if(OnMaxAttemptsMade != null){
                Debug.Log("EVENT TRIGGERED");
                OnMaxAttemptsMade.Invoke();
            }else{
                Debug.Log("EVENT NOT TRIGGERED - EVENT IS NULL");
            }
        }
    }

}
