using UnityEngine;

public class Minigame : MonoBehaviour
{
    public float speed = 5f;
    private float maxSpeed;
    private SpriteRenderer minigameBorder;
    private GameObject indicator;
    private float borderMinX, hitSpotMinX;
    private float borderMaxX, hitSpotMaxX;
    private bool minigameStart = false;
    private bool continueRight = true;
    private SpriteRenderer hitSpot;
    private int amountOfMinigames = 3;
    private int correctHits = 0;
    private Fish fishCurrHooked;
    private float randomSizeX;
    public float hitSpotBuffer = 0.15f;
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
        minigameBorder = gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
        indicator = gameObject.transform.GetChild(0).GetChild(1).gameObject;
        hitSpot = gameObject.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        borderMinX = minigameBorder.bounds.min.x + 1.5f;
        borderMaxX = minigameBorder.bounds.max.x - 1;
        maxSpeed = speed + speed;
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
    }

    void EndMinigame(){
        if (correctHits >= amountOfMinigames)
        {
            GameManager.Instance.AddFish(fishCurrHooked);
        }
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        minigameStart = false;
        GameManager.Instance.fishOnHook = false;
        GameManager.Instance.EndMinigame();
        fishCurrHooked.gameObject.SetActive(false);
        correctHits = 0;
        speed = 7 + GameManager.Instance.FishCaught() - 0.5f; //reset speed so the difficulty remains the same and speed doesn't increase exponentionally
        maxSpeed = speed + speed - 2;
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
            }else{
                Debug.Log("MISS");
            }

            if (correctHits >= amountOfMinigames || !hitSpotHit){
                EndMinigame();
            }
        }
    }
    
    void ChangeHitSpot(){
        //changes the location of the hitspot somwhere in within the bounds of the border sprite
        float borderRandomX = Random.Range(borderMinX, borderMaxX - 1f);
        hitSpot.gameObject.transform.position = new Vector3(borderRandomX, minigameBorder.transform.position.y, 0);
        ChangeSizeOfHitSpot();
        hitSpotMinX = hitSpot.bounds.min.x - hitSpotBuffer;
        hitSpotMaxX = hitSpot.bounds.max.x + hitSpotBuffer;
    }

    void ChangeSizeOfHitSpot(){
        //changes the size in this case the scale of hit spot to make it narrow or wider
        randomSizeX = Random.Range(0.1f, 0.3f);
        hitSpot.gameObject.transform.localScale = new Vector3(randomSizeX, 0.44f, 0);
    }

    void ChangeIndicatorSpeed(){
        //something we can do is keep the speed and never reset it so the deeper they go the more difficult the minigame becomes
        float randomSpeed = Random.Range(speed, maxSpeed); //difficully range: hard would be speed to 20 easy: speed to 13 NOTE: this is if speed is set to 7
        speed = randomSpeed;
        Debug.Log($"Speed: {speed}");
    }
}
