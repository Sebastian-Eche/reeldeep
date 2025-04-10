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
        MoveStraightToWayPoint();
    }

    // Update is called once per frame
    void Update()
    {
        // if (isSwimming){
            // MoveStraightToWayPoint();
        // }
    }

    void MoveStraightToWayPoint(){
        // transform.position = Vector3.MoveTowards(transform.position, waypoint.transform.position, moveSpeed * Time.deltaTime);
        transform.DOMove(waypoint.transform.position, 5f).SetEase(Ease.InSine);
    }

    void PauseMovement(Fish hookedFish){
        if (ReferenceEquals(this.gameObject, hookedFish.gameObject)){
            Debug.Log("PAUSEDDDD");
            // isSwimming = false;
            transform.DOPause();
        }
    }
}
