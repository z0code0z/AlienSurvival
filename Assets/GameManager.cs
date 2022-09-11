using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public GameObject EnemyToSpawn;
    public Transform Player;

    public GameObject WeaponSystem;
    public float SpawnTime = 5f;
    public float Timerate = 0.085f;


    private void Start()
    {

        StartCoroutine(StartSpawning());

    }

    IEnumerator StartSpawning()
    {
        while (true)
        {



            yield return new WaitForSecondsRealtime(SpawnTime);

            GameObject SpawnedEnemy = Instantiate(EnemyToSpawn, new Vector3(Random.Range(-25f, 25f), 85f, Random.Range(-25f, 25f)), Quaternion.identity);
            SpawnedEnemy.GetComponent<FlyingEnemyAI>().Player = Player;
            SpawnedEnemy.GetComponent<FlyingEnemyAI>().WeaponSystem = WeaponSystem;
            SpawnTime -= SpawnTime * Timerate;

        }

    }

}
