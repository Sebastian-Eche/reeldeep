using System;
using DG.Tweening;
using UnityEngine;

public class HookController : MonoBehaviour
{
    public Camera mainCamera;
    private float startingY;
    public float descendSpeed = 4f;
    private bool hookStopped = false;
    public bool isPaused = false;
    private bool isReturning = false;
    private Transform boatPOS;

    private void OnDisable()
    {
        GameManager.Instance.OnMaxAttemptsMade -= ReturnToBoat;
    }

    void Start()
    {
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
        }else if(Input.GetKeyUp(KeyCode.LeftShift)){
            descendSpeed = 4f;
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
        mouseWorldPosition.x = Mathf.Clamp(mouseWorldPosition.x, leftBounds, rightBounds); //change hardcoded values of -10 to 10 to camera bounds
        startingY -= descendSpeed * Time.deltaTime;
        Vector3 newPosition = new Vector3(mouseWorldPosition.x, startingY, 0f);
        transform.DOMove(newPosition, 0.63f).SetEase(Ease.OutSine);
        // Debug.Log(transform.position);
    }

    void StopHook(){
        Debug.Log("STOP TOGGLE");
        hookStopped = !hookStopped;

        if (hookStopped){
            descendSpeed = 0f;
        }else{
            descendSpeed = 4f;
        }
    }

    void AccelerateHook(){
        descendSpeed *= 2.2f;
    }

    void ReturnToBoat(){
        Debug.Log("EVENT INVOKED: Max Capacity of Fish");
        isReturning = true;
    }

    void Returning(){
        transform.position = Vector3.MoveTowards(transform.position, boatPOS.position, descendSpeed * Time.deltaTime);
    }

    bool ReachedBoat(){
        if ((boatPOS.position - transform.position).magnitude < 0.1f){
            // Debug.Log("Boat Reached");
            isReturning = false;
            startingY = boatPOS.position.y;
            return true;
        }
        return false;
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
        // Debug.Log(other.gameObject.name + " is hooked");
    }
}
