using UnityEngine;

// First-person camera for Street Empire.
public class StreetEmpireCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 eyeOffset = new Vector3(0f, 1.62f, 0f);
    public float sensitivity = 2.0f;
    public float minPitch = -80f;
    public float maxPitch = 80f;
    float yaw, pitch;

    void Start()
    {
        if (target) yaw = target.eulerAngles.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (!target) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool locked = Cursor.lockState == CursorLockMode.Locked;
            Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = locked;
        }
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            yaw += Input.GetAxis("Mouse X") * sensitivity * 2.2f;
            pitch -= Input.GetAxis("Mouse Y") * sensitivity * 2.2f;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
        target.rotation = Quaternion.Euler(0f, yaw, 0f);
        transform.position = target.position + Vector3.up * eyeOffset.y;
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
