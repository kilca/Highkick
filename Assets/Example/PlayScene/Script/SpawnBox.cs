using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBox : MonoBehaviour
{


    public GameObject cube;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("Spawn du cube");
            Vector3S pos = new Vector3S(new Vector3(0,10,0));
            HNetwork.Instantiate(cube.name, pos, new QuaternionS(Quaternion.identity), new string[0]);
        }
    }
}
