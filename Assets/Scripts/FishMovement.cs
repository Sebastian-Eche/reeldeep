using DG.Tweening;
using UnityEngine;

public class FishMovement : MonoBehaviour
{
    public GameObject waypoint;
    public float moveSpeed = 3f;
    private bool isSwimming = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        Fish.OnFishHooked += PauseMovement;
    }

    void OnDisable()
    {
        Fish.OnFishHooked -= PauseMovement;
    }
    void Start()
    {
        // MoveStraightToWayPoint();
    }

    void Update()
    {
        //Unpauses all fish when minigame is complete
        if (!GameManager.Instance.minigameStart){
            isSwimming = true;
        }

        if (isSwimming){
            MoveStraightToWayPoint();
        }
    }

    void MoveStraightToWayPoint(){
        if ((waypoint.transform.position - transform.position).magnitude < 0.01f){
            isSwimming = false;
        }
        transform.position = Vector3.MoveTowards(transform.position, waypoint.transform.position, moveSpeed * Time.deltaTime);
    }

    void PauseMovement(Fish hookedFish){
        Debug.Log("PAUSEDDDD");
        isSwimming = false;
    }

    
}
