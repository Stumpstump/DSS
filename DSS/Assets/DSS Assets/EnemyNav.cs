/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyNav : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}*/

using UnityEngine;
using UnityEngine.AI;

public class EnemyNav : MonoBehaviour {

    public Transform player;
    public NavMeshAgent agent;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        agent.SetDestination(player.position);
    }
}