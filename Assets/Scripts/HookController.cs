using System;
using UnityEngine;

public class HookController : MonoBehaviour
{

    public Camera mainCamera;
    private float currentY;
    public float descendSpeed = 2f;
    private bool isStopped = false;
    private bool isAccelerated = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        FollowMouse();
        if(Input.GetKeyDown(KeyCode.LeftShift)){
            AccelerateHook();
        }

        if(Input.GetKeyDown(KeyCode.LeftControl)){
            StopHook();
        }else if(Input.GetKeyUp(KeyCode.LeftControl)){
            descendSpeed = 2f;
        }
    }

    void FollowMouse(){
        mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        currentY -= descendSpeed * Time.deltaTime;
        transform.position = new Vector3(mouseWorldPosition.x, currentY, 0f);
    }

    void StopHook(){
        descendSpeed = 0f;
    }

    void AccelerateHook(){
        descendSpeed *= 2.2f;
    }
}
