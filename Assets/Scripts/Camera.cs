using UnityEngine;

public class Camera : MonoBehaviour
{

    Transform playerTransform;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    void Start()
    {
        playerTransform = GameObject.Find("Player").transform;
    }

    void Update()
    {
        Vector3 desiredPosition = playerTransform.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}