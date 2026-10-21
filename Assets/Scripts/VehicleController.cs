using UnityEngine;

public class VehicleController : MonoBehaviour
{
    public float speed = 9f;
    public float turnSpeed = 95f;
    bool hasDriver;

    public void SetDriver(bool value) { hasDriver = value; }

    void Update()
    {
        if (!hasDriver) return;
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        float dist = v * speed * Time.deltaTime;
        if (Mathf.Abs(dist) > 0.0001f)
        {
            // Простая проверка столкновения, чтобы машина не ездила сквозь здания
            Vector3 dir = transform.forward * Mathf.Sign(dist);
            Vector3 half = transform.localScale * 0.5f * 0.95f;
            if (!Physics.BoxCast(transform.position, half, dir, transform.rotation, Mathf.Abs(dist) + 0.05f))
                transform.position += transform.forward * dist;
        }

        // Руль работает только при движении (и инвертируется на заднем ходе)
        transform.Rotate(Vector3.up, h * v * turnSpeed * Time.deltaTime);
    }
}
