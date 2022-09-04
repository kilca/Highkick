using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlayer : MonoBehaviour
{

    public GameObject playerPrefab;
    public float minX;
    public float maxX;
    public float minZ;
    public float maxZ;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 randomPosition = new Vector3(Random.Range(minX, maxX), 10, Random.Range(minZ, maxZ));
        SteamManager.Instantiate(playerPrefab.name, new Vector3S(randomPosition), new QuaternionS(Quaternion.identity), new string[0]);
    }


}
