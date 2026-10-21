
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class CitizenAI : MonoBehaviour
{
    [Header("Блуждание")]
    public float wanderRadius = 25f;
    public float wanderTimer = 6f;

    NavMeshAgent agent;
    float timer;
    bool hasReported;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = wanderTimer;
    }

    void Update()
    {
        if (!agent || !agent.enabled || !agent.isOnNavMesh)
            return;

        timer += Time.deltaTime;

        if (timer >= wanderTimer)
        {
            Vector3 newPos = RandomNavSphere(
                transform.position, wanderRadius, NavMesh.AllAreas);

            agent.SetDestination(newPos);
            timer = 0f;
        }
    }

    public void WitnessCrime(float delay)
    {
        if (hasReported) return;
        StartCoroutine(ReportAfterDelay(delay));
    }

    IEnumerator ReportAfterDelay(float delay)
    {
        hasReported = true;

        // Свидетель заметил преступление и сообщает не мгновенно.
        yield return new WaitForSeconds(delay);

        if (GameManager.I != null)
        {
            GameManager.I.AddWanted(1);
            GameManager.I.Notice = "Свидетель сообщил полиции!";
        }
    }

    public static Vector3 RandomNavSphere(
        Vector3 origin, float dist, int layermask)
    {
        Vector3 randomDirection = Random.insideUnitSphere * dist;
        randomDirection += origin;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(
            randomDirection, out hit, dist, layermask))
            return hit.position;

        return origin;
    }
}
