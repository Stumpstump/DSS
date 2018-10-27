using UnityEngine;
using UnityEngine.AI;

public class EnemyNav : MonoBehaviour {

    public Transform player;
    public NavMeshAgent agent;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();


    }

    void Update()
    {
        if (player != null)
            agent.SetDestination(player.position);
    }
}