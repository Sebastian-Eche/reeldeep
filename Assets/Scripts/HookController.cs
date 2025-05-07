using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class HookController : MonoBehaviour
{
    public Camera mainCamera;
    private float startingY;
    public float descendSpeed = 4f;
    private float regularDescendSpeed = 0;
    private bool hookStopped = false;
    private bool hookAccelerated = false;
    public bool isPaused = false;
    private bool isReturning = false;
    private Transform boatPOS;
    private float returningTimer = 3f;
    private float returnSpeed = 4f;

    private void OnDisable()
    {
        GameManager.Instance.OnMaxAttemptsMade -= ReturnToBoat;
    }

    void Start()
    {
        regularDescendSpeed = descendSpeed;
        startingY = transform.position.y;
        GameManager.Instance.OnMaxAttemptsMade += ReturnToBoat;
        Debug.Log("EVENT SUBSCRIBED");
        boatPOS = GameManager.Instance.returnPoint.transform;
    }

    void Update()
    {
        //when the minigame is playing pause ability to move hook around
        if(isPaused || isReturning || GameManager.Instance.isDisplayingInfo){
            if(isReturning){
                // Debug.Log("IT IS RETURNING");
                Returning();

                if(ReachedBoat()){
                    Debug.Log("BOAT IS REACHED");
                    GameManager.Instance.isDisplayingInfo = true;
                    StartCoroutine(GameManager.Instance.DisplayCaughtFish());
                }
            }
            return;
        }


        FollowMouse();
        if(Input.GetKeyDown(KeyCode.LeftShift)){
            AccelerateHook();
        } 

        if(Input.GetKeyDown(KeyCode.LeftControl)){
            StopHook();
        }
    }

    void FollowMouse(){
        // Next three lines makes it where the hook can only move to left bounds of the camera or right
        float cameraWidth = mainCamera.orthographicSize * mainCamera.aspect;
        float leftBounds = mainCamera.transform.position.x - cameraWidth;
        float rightBounds = mainCamera.transform.position.x + cameraWidth;

        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.x = Mathf.Clamp(mouseWorldPosition.x, leftBounds, rightBounds);
        startingY -= descendSpeed * Time.deltaTime;
        Vector3 newPosition = new Vector3(mouseWorldPosition.x, startingY, 0f);
        transform.DOMove(newPosition, 0.63f).SetEase(Ease.OutSine);
    }

    void StopHook(){
        Debug.Log("STOP TOGGLE");
        hookStopped = !hookStopped;

        if (hookStopped){
            descendSpeed = 0f;
        }else{
            descendSpeed = regularDescendSpeed;
        }
    }

    void AccelerateHook(){
        hookAccelerated = !hookAccelerated;
        if (hookAccelerated){
            descendSpeed *= 2.2f;
        }else{
            descendSpeed = regularDescendSpeed;
        }
    }

    void ReturnToBoat(){
        Debug.Log("EVENT INVOKED: Max Capacity of Fish");
        isReturning = true;
    }

    void Returning(){
        if (returningTimer <= 0) {
            returnSpeed += 5f * Time.deltaTime;
        }
        returningTimer -= Time.deltaTime;
        float step = returnSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, boatPOS.position, step);
        gameObject.GetComponent<Collider2D>().enabled = false;

        if ((boatPOS.position - transform.position).magnitude < 25f){
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 2f * Time.deltaTime);
        }

    }

    bool ReachedBoat(){
        if ((boatPOS.position - transform.position).magnitude < 0.1f){
            returningTimer = 3f;
            isReturning = false;
            returnSpeed = 4f;
            startingY = boatPOS.position.y;
            gameObject.GetComponent<Collider2D>().enabled = true;
            GameManager.Instance.attemptsToCatch = 0;
            return true;
        }
        return false;
    }

    public void MakeHookBig(){
        transform.localScale = Vector3.one;
    } 

    IEnumerator SkipNearReturnPoint(){
        yield return new WaitForSeconds(3);
        if (transform.position.y <= -40){
            transform.position = new Vector3(0, -10, 0);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!GameManager.Instance.fishOnHook){
            if(other.gameObject.TryGetComponent<Fish>(out Fish fish)){
                fish.isHooked = true;
                fish.HookFish();
                GameManager.Instance.fishOnHook = true;
            }
        }
        if (other.gameObject.CompareTag("Habitats")){
            GameManager.Instance.habitatHit.gameObject.SetActive(true);
            GameManager.Instance.habitatHit.text = "-1 ATTEMPTS";
            StartCoroutine(GameManager.Instance.RemoveDisplay());
            GameManager.Instance.attemptsToCatch++;
            Destroy(other.gameObject);
            GameManager.Instance.ReturningToBoat();
        }
        Debug.Log(GameManager.Instance.attemptsToCatch);
    }
}
