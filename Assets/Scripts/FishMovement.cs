using System;
using DG.Tweening;
using UnityEngine;

public class FishMovement : MonoBehaviour
{
    public enum SwimStyle {
        Straight, Randomly
    }
    public SwimStyle swimStyle; 
    public Transform endPoint;
    public Vector3 waypoint;
    public Transform head;
    private bool isSwimming = true;
    // public bool isIdleSwimming = false;
    // public bool isStraightSwimming = false;
    public float moveSpeed = 3f;
    private float currFishY;
    private float currFishX;
    private float directionChangeTimer;
    public float directionChangeTime = 3;
    private Quaternion keepRotation;

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
        currFishY = transform.position.y;
        currFishX = transform.position.x;
        Debug.Log("STARTING X: " + currFishX + "STARTING Y: " + currFishY);
        directionChangeTimer = directionChangeTime;
        NewWayPoint();
    }

    void Update()
    {
        //Unpauses all fish when minigame is complete
        if (!GameManager.Instance.minigameStart){
            isSwimming = true;
        }

        if(isSwimming){
            switch (swimStyle){
                case SwimStyle.Straight:
                    MoveStraightToEndPoint(endPoint.position);
                    break;
                case SwimStyle.Randomly:
                    MoveToWayPoint(waypoint);
                    break;
            }
        }

    }

    void MoveStraightToEndPoint(Vector3 waypoint){
        if ((waypoint - transform.position).magnitude < 0.01f){
            isSwimming = false;
        }
        transform.position = Vector3.MoveTowards(transform.position, waypoint, moveSpeed * Time.deltaTime);
        Debug.DrawLine(transform.position, waypoint, Color.red);
    }

    void MoveToWayPoint(Vector3 waypoint){
        if ((waypoint - transform.position).magnitude < 0.01f){
            head.rotation = keepRotation;
            if (directionChangeTimer <= 0){
                directionChangeTimer = directionChangeTime;
                NewWayPoint();
            }else{
                directionChangeTimer -= Time.deltaTime;
            }
        }else{
            keepRotation = head.rotation;
            TurnToWaypoint(waypoint);
        }
        transform.position = Vector3.MoveTowards(transform.position, waypoint, moveSpeed * Time.deltaTime);
        // TurnToWaypoint(waypoint);
        Debug.DrawLine(transform.position, waypoint, Color.red);
    }

    void NewWayPoint(){
        float randomX = UnityEngine.Random.Range(currFishX - 5, currFishX + 15);
        Debug.Log("RANDOM X: " + randomX);
        float randomY = UnityEngine.Random.Range(currFishY - 4, currFishY + 4);
        Debug.Log("RANDOM Y: " + randomY);
        Vector3 newWaypoint = new Vector3(randomX, randomY, 0f);
        waypoint = newWaypoint;
        directionChangeTime = (float)Math.Round(UnityEngine.Random.Range(1.5f, directionChangeTime+1));
        Debug.Log("WAIT TIME TO MOVE: " + directionChangeTime);
    }

    void TurnToWaypoint(Vector3 newWaypoint){
        Vector3 distance = newWaypoint - head.position;
        Quaternion lookToWaypoint = Quaternion.LookRotation(Vector3.forward, distance);
        head.rotation = Quaternion.Slerp(head.rotation, lookToWaypoint, Time.deltaTime * moveSpeed);
    }

    void PauseMovement(Fish hookedFish){
        Debug.Log("PAUSEDDDD");
        isSwimming = false;
    }

    
}
