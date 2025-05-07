using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class Minigame : MonoBehaviour
{
    private FishMannager fishManager;
    private Fish fishCurrHooked;
    [Header("UI Elements")]
    public TextMeshProUGUI caughtFishDisplay;

    [Header("Minigame Settings")]
    public float speed = 5f;
    private float maxSpeed;
    private float speedModifier;
    public float hitSpotBuffer = 0.15f;
    private float minFishHitspotSize;
    private float maxFishHitspotSize;

    [Header("Minigame State")]
    private bool minigameStart = false;
    private bool continueRight = true;
    private int amountOfMinigames = 3;
    private int correctHits = 0;
    private int missCounter = 0;

    [Header("Minigame Objects")]
    private GameObject indicator;
    private SpriteRenderer minigameBorder;
    private SpriteRenderer hitSpot;

    [Header("Position Bounds")]
    private float borderMinX, hitSpotMinX;
    private float borderMaxX, hitSpotMaxX;
    private float randomHitspotSizeX;
    private void OnEnable()
    {
        Fish.OnFishHooked += StartMinigame;
    }

    private void OnDisable()
    {
        Fish.OnFishHooked -= StartMinigame;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        minFishHitspotSize = 0.1f;
        maxFishHitspotSize = 0.3f;
        minigameBorder = gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
        indicator = gameObject.transform.GetChild(0).GetChild(1).gameObject;
        hitSpot = gameObject.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        borderMinX = minigameBorder.bounds.min.x + 1.5f;
        borderMaxX = minigameBorder.bounds.max.x - 1;
        maxSpeed = speed + 2;
        fishManager = FindObjectOfType<FishMannager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(minigameStart){
            MoveIndicator();
            IndicatorHitSpot();
        }

    }

    void StartMinigame(Fish hookedFish){
        Debug.Log("FISH MINIGAME STARTS");
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
        Vector3 offset = hookedFish.transform.position;
        gameObject.transform.position = new Vector3(0, offset.y, 0);
        indicator.transform.position = new Vector3(borderMinX, offset.y, 0);
        minigameStart = true;
        GameManager.Instance.StartMinigame();
        fishCurrHooked = hookedFish;
        ChangeHitSpot();
        GameManager.Instance.IncrementAttempts();
        CheckRarity();
    }

    void EndMinigame(){
        if (correctHits >= amountOfMinigames)
        {
            GameManager.Instance.AddFish(fishCurrHooked);
            fishManager.AddCaughtFish(fishCurrHooked.fishInfo);
        }else{
            GameManager.Instance.ReturningToBoat();
        }
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        minigameStart = false;
        GameManager.Instance.fishOnHook = false;
        GameManager.Instance.EndMinigame();
        fishCurrHooked.gameObject.SetActive(false);
        correctHits = 0;
        speedModifier += GameManager.Instance.CurrentFishCapacity();
    }

    void MoveIndicator(){
        Vector2 indicatorPOS = indicator.transform.position;
        if (indicatorPOS.x >= borderMaxX){
            continueRight = false;
        }

        if (indicatorPOS.x < borderMaxX && continueRight){
            indicatorPOS.x += speed * Time.deltaTime;
        }else{
            indicatorPOS.x -= speed * Time.deltaTime;
            if (indicatorPOS.x <= borderMinX){
                continueRight = true;
            }
        }

        indicator.transform.position = indicatorPOS;
        // Debug.Log(indicatorPOS);
    }

    void IndicatorHitSpot(){
        bool hitSpotHit = false;
        if (Input.GetKeyDown(KeyCode.Space)){
            Debug.Log("indicator hit here: " + indicator.transform.position.x);
            Debug.Log("hit spot is from: " + hitSpotMinX + " to " + hitSpotMaxX);
            if (indicator.transform.position.x >=  hitSpotMinX && indicator.transform.position.x <= hitSpotMaxX){
                Debug.Log("HIT");
                hitSpotHit = true;
                ++correctHits;
                ChangeHitSpot();
                ChangeIndicatorSpeed();
                missCounter = 0;
            }else{
                Debug.Log("MISS");
                missCounter++;
                MissHelper();
            }

            if (correctHits >= amountOfMinigames || !hitSpotHit){
                caughtFishDisplay.gameObject.SetActive(true);
                if (!hitSpotHit){
                    caughtFishDisplay.text = "FAILED";
                }
                StartCoroutine(RemoveDisplay());
                EndMinigame();
            }
        }
    }
    
    void ChangeHitSpot(){
        //changes the location of the hitspot somwhere in within the bounds of the border sprite
        float borderRandomX = UnityEngine.Random.Range(borderMinX, borderMaxX - 1f);
        hitSpot.gameObject.transform.position = new Vector3(borderRandomX, minigameBorder.transform.position.y, 0);
        ChangeSizeOfHitSpot();
        hitSpotMinX = hitSpot.bounds.min.x - hitSpotBuffer;
        hitSpotMaxX = hitSpot.bounds.max.x + hitSpotBuffer;
    }

    void ChangeSizeOfHitSpot(){
        //changes the size in this case the scale of hit spot to make it narrow or wider
        randomHitspotSizeX = UnityEngine.Random.Range(minFishHitspotSize, maxFishHitspotSize); //0.1, 0.3
        hitSpot.gameObject.transform.localScale = new Vector3(randomHitspotSizeX, 0.44f, 0);
    }

    void ChangeIndicatorSpeed(){
        //something we can do is keep the speed and never reset it so the deeper they go the more difficult the minigame becomes
        float randomSpeed = UnityEngine.Random.Range(speed, maxSpeed); //difficully range: hard would be speed to 20 easy: speed to 13 NOTE: this is if speed is set to 7
        speed = randomSpeed;
        Debug.Log($"Speed: {speed}");
    }

    void MissHelper(){
        Debug.Log("MISS HELPER");
        if (missCounter % 2 == 0){
            Debug.Log("helper activated");
            speed -= 1;
        }
    }

    void CheckRarity(){
        switch (fishCurrHooked.fishInfo.rarity){
            case FishInfo.Rarity.Common:
                speed = 6f;
                maxSpeed = 8f;
                minFishHitspotSize = 0.1f;
                maxFishHitspotSize = 0.3f;
                break;
            case FishInfo.Rarity.Uncommon:
                speed = 9f;
                maxSpeed = 12f;
                minFishHitspotSize = 0.1f;
                maxFishHitspotSize = 0.2f;
                break;
            case FishInfo.Rarity.Rare:
                speed = 13f;
                maxSpeed = 16f;
                minFishHitspotSize = 0.1f;
                maxFishHitspotSize = 0.16f;
                break;
            case FishInfo.Rarity.Legendary:
                speed = 17f;
                maxSpeed = 21f;
                minFishHitspotSize = 0.1f;
                maxFishHitspotSize = 0.13f;
                break;
        }
        speed += speedModifier;
        maxSpeed += speedModifier;
        Debug.Log(fishCurrHooked.fishInfo.rarity);
    }

    IEnumerator RemoveDisplay(){
        yield return new WaitForSeconds(1);
        caughtFishDisplay.gameObject.SetActive(false);
        caughtFishDisplay.text = "CAUGHT";
    }

}
