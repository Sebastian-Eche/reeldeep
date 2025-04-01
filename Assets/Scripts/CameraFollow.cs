using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Vector3 offset = new Vector3(0, 0, -10f);
    public Transform target;

    void Update()
    {
        Vector3 followPosition = target.position + offset;
        transform.position = new Vector3(0, followPosition.y, 0);
    }
}
