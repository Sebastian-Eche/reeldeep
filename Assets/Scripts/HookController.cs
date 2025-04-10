using System;
using DG.Tweening;
using UnityEngine;

public class HookController : MonoBehaviour
{
    public Camera mainCamera;
    private float startingY;
    public float descendSpeed = 2f;
    private bool hookStopped = false;
    public bool isPaused = false;
    private bool isReturning = false;
    private Vector3 boatPOS = new Vector3(0,5,0);

    private void OnDisable()
    {
        GameManager.Instance.OnMaxFishCapacity -= ReturnToBoat;
    }

    void Start()
    {
        startingY = transform.position.y;
        GameManager.Instance.OnMaxFishCapacity += ReturnToBoat;
        Debug.Log("EVENT SUBSCRIBED");
    }

    void Update()
    {
        //when the minigame is playing pause ability to move hook around
        if(isPaused || isReturning){
            if(isReturning){
                Debug.Log("IT IS RETURNING");
                Returning();

                if(ReachedBoat()){
                    Debug.Log("BOAT IS REACHED");
                }
            }
            return;
        }


        FollowMouse();
        if(Input.GetKeyDown(KeyCode.LeftShift)){
            AccelerateHook();
        }else if(Input.GetKeyUp(KeyCode.LeftShift)){
            descendSpeed = 2f;
        }

        if(Input.GetKeyDown(KeyCode.LeftControl)){
            StopHook();
        }
    }

    void FollowMouse(){
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.x = Mathf.Clamp(mouseWorldPosition.x, -10, 10);
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
            descendSpeed = 2f;
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
        transform.position = Vector3.MoveTowards(transform.position, boatPOS, descendSpeed * Time.deltaTime);
    }

    bool ReachedBoat(){
        if ((boatPOS - transform.position).magnitude < 0.1f){
            // Debug.Log("Boat Reached");
            isReturning = false;
            startingY = boatPOS.y;
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
