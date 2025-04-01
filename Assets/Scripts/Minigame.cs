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
        maxX = minigameBorder.bounds.max.x - 2;
        hitSpotMinX = hitSpot.bounds.min.x;
        hitSpotMaxX = hitSpot.bounds.max.x;
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
    }

    void EndMinigame(){
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
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
        if (Input.GetKeyDown(KeyCode.Space)){
            if (indicator.transform.position.x >=  hitSpotMinX && indicator.transform.position.x <= hitSpotMaxX){
                Debug.Log("HIT");
            }else{
                Debug.Log("MISS");
            }
            minigameStart = false;
            EndMinigame();
        }
    }
}
