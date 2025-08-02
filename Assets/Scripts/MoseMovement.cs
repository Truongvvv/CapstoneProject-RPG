using UnityEngine;

public class MoseMovement : MonoBehaviour
{
    public Transform target; // gán là CameraRig
    public float distance = 5.0f;
    public float xSpeed = 120f;
    public float ySpeed = 120f;
    public float yMin = -20f;
    public float yMax = 60f;

    float x = 0f;
    float y = 0f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        if (!target) return;

        x += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
        y -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;
        y = Mathf.Clamp(y, yMin, yMax);

        Quaternion rotation = Quaternion.Euler(y, x, 0);
        Vector3 position = rotation * new Vector3(0, 0, -distance) + target.position;

        transform.rotation = rotation;
        transform.position = position;
    }
}


