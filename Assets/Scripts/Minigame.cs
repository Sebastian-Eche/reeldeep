using UnityEngine;

public class Minigame : MonoBehaviour
{
    public float speed = 5f;
    private SpriteRenderer minigameBorder;
    private GameObject indicator;
    private float minX, hitSpotMinX;
    private float maxX, hitSpotMaxX;
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
        minX = minigameBorder.bounds.min.x + 1;
        maxX = minigameBorder.bounds.max.x - 1;
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
        indicator.transform.position = new Vector3(minX, offset.y, 0);
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
    }

    void MoveIndicator(){
        Vector2 indicatorPOS = indicator.transform.position;
        if (indicatorPOS.x >= maxX){
            continueRight = false;
        }

        if (indicatorPOS.x < maxX && continueRight){
            indicatorPOS.x += speed * Time.deltaTime;
        }else{
            indicatorPOS.x -= speed * Time.deltaTime;
            if (indicatorPOS.x <= minX){
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
        float randomX = Random.Range(minX, maxX - 1f);
        hitSpot.gameObject.transform.position = new Vector3(randomX, fishCurrHooked.transform.position.y, 0);
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
        float randomSpeed = Random.Range(5f, 10f);
        speed = randomSpeed;
    }
}
