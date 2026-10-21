using UnityEngine;

public class StreetEmpireCitizen : MonoBehaviour
{
    Vector3 destination; float timer; float speed;
    void Start() { speed = Random.Range(1.0f, 1.65f); PickDestination(); }
    void Update()
    {
        timer -= Time.deltaTime;
        Vector3 flat = destination - transform.position; flat.y = 0;
        if (timer <= 0 || flat.magnitude < 1f) PickDestination();
        if (flat.sqrMagnitude > .1f) { transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(flat), 5f * Time.deltaTime); transform.position += transform.forward * speed * Time.deltaTime; }
    }
    void PickDestination() { destination = transform.position + new Vector3(Random.Range(-18f, 18f), 0, Random.Range(-18f, 18f)); timer = Random.Range(4f, 10f); }
}
