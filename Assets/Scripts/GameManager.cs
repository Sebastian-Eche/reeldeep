using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private float depthScale = 2.5f;
    public static GameManager Instance { get; private set;}
     [Header("Game Object References")]
    public CameraFollow cameraFollowObject;
    public HookController hookControllerObject;
    public FishMovement fishMovementObject;
    public GameObject returnPoint;

    [Header("UI Elements")]
    public TextMeshProUGUI fishSpeciesText;
    public Image fishSprite;
    public TextMeshProUGUI fishMetaDataText;
    public TextMeshProUGUI depthText;
    public TextMeshProUGUI caughtFishText;

    [Header("Game State")]
    public bool minigameStart = false;
    public bool fishOnHook = false;
    public bool isDisplayingInfo = false;

    [Header("Fish Capture Tracking")]
    [SerializeField] private List<Fish> fishCaught = new List<Fish>();
    public int maxAttempts = 2;
    public int attemptsToCatch = 0;
    public event Action OnMaxAttemptsMade;
    void Awake()
    {
        if (Instance == null){
            Instance = this;
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (hookControllerObject.gameObject.transform.position.y <= 0.1){
            float depth = Mathf.Abs(hookControllerObject.gameObject.transform.position.y)/depthScale;
            depthText.text = $"Depth: {depth:F1}m";
        }
        
    }

    public void StartMinigame(){
        minigameStart = true;
        cameraFollowObject.enabled = false;
        hookControllerObject.isPaused = true;
    }

    public void EndMinigame(){
        minigameStart = false;
        cameraFollowObject.enabled = true;
        hookControllerObject.isPaused = false;
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
        if (fishCaught.Count > 0){
            fishSprite.gameObject.SetActive(true);
        }
        if (fishCaught.Count <= 0){
            fishSpeciesText.text = "NO FISH CAUGHT \n";
            fishMetaDataText.text = ":(";
            yield return new WaitForSeconds(3);
        }

        for (int i = 0; i < fishCaught.Count; i++){
            fishSpeciesText.text = "You caught a " + fishCaught[i].fishInfo.fishSpecies;
            fishSprite.sprite = fishCaught[i].fishInfo.fishSprite;
            fishMetaDataText.text = "Length: " + fishCaught[i].GetLength() + "in" + " Weight: " + fishCaught[i].GetWeight() + "lbs";
            yield return new WaitForSeconds(3);
        }
        fishSpeciesText.gameObject.SetActive(false);
        fishMetaDataText.gameObject.SetActive(false);
        fishSprite.gameObject.SetActive(false);
        isDisplayingInfo = false;
        attemptsToCatch = 0;
        fishCaught = new List<Fish>();
        hookControllerObject.MakeHookBig();
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

    public void CaughtFishText(){
        caughtFishText.text = "x " + fishCaught.Count;
    }

}
