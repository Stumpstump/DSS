using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    public bool TriggerASpawn;

    // Update is called once per frame
    void Update()
    {
        if (TriggerASpawn)
        {
            TriggerASpawn = false;
            GameObject.FindGameObjectWithTag("Fire").GetComponent<DDS.Spawner>().Trigger_Spawn = true;
        }
    }
}
