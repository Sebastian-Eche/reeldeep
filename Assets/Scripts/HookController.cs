using System;
using UnityEngine;

public class HookController : MonoBehaviour
{

    public Camera mainCamera;
    private float startingY;
    public float descendSpeed = 2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startingY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        FollowMouse();
        if(Input.GetKeyDown(KeyCode.LeftShift)){
            AccelerateHook();
        }else if(Input.GetKeyUp(KeyCode.LeftShift)){
            descendSpeed = 2f;
        }

        if(Input.GetKeyDown(KeyCode.LeftControl)){
            StopHook();
        }else if(Input.GetKeyUp(KeyCode.LeftControl)){
            descendSpeed = 2f;
        }
    }

    void FollowMouse(){
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.x = Mathf.Clamp(mouseWorldPosition.x, -10, 10);
        startingY -= descendSpeed * Time.deltaTime;
        transform.position = new Vector3(mouseWorldPosition.x, startingY, 0f);
        // Debug.Log(transform.position);
    }

    void StopHook(){
        descendSpeed = 0f;
    }

    void AccelerateHook(){
        descendSpeed *= 2.2f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.TryGetComponent<Fish>(out Fish fish)){
            fish.isHooked = true;
            fish.HookFish();
        }
        // Debug.Log(other.gameObject.name + " is hooked");
    }
}
