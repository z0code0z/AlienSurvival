using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSystem : MonoBehaviour
{

    public Transform SystemPoint;
    public Transform BulletSpawnPoint;

    public GameObject Bullet;

    public float PositionSpeed;
    public float RotationSpeed;

    public float FireRate;
    bool CanFire = true;



    private void Update()
    {

        if (CanFire)
        {
            if (Input.GetMouseButton(0))
            {

                StartCoroutine(FireTimer());

                GameObject SpawnedBullet = Instantiate(Bullet, BulletSpawnPoint.position, Quaternion.identity);
                SpawnedBullet.GetComponent<BulletComponent>().Bullet(SystemPoint.forward);

            }
        }

    }


    private void FixedUpdate()
    {


        transform.position = Vector3.Lerp(transform.position, SystemPoint.position, PositionSpeed * Time.fixedDeltaTime);
        transform.forward = Vector3.Lerp(transform.forward, SystemPoint.forward, RotationSpeed * Time.fixedDeltaTime);


    }

    IEnumerator FireTimer()
    {

        CanFire = false;
        yield return new WaitForSecondsRealtime(FireRate);
        CanFire = true;

    }

    public void FireRateIncrease()
    {

        FireRate -= FireRate*0.035f;

    }

    public void Death()
    {

        CanFire=false;

    }


}
