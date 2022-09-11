using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletComponent : MonoBehaviour
{

    public ParticleSystem Particles;

    public GameObject HitEnemyParticle;

    private void OnCollisionEnter(Collision collision)
    {



        Particles.Stop();
        
        if(collision.transform.tag == "Enemy")
        {

            Instantiate(HitEnemyParticle, transform.position, Quaternion.identity);
            collision.transform.GetComponent<FlyingEnemyAI>().GotHit();
        }
        
        StartCoroutine(ParticleDeathTimer());



    }
    private void Start()
    {
        StartCoroutine(Real());
    }

    public void Bullet(Vector3 DirectionToShoot)
    {

        GetComponent<Rigidbody>().AddForce(DirectionToShoot * 40f, ForceMode.VelocityChange);

    }


    IEnumerator ParticleDeathTimer()
    {

        yield return new WaitForSecondsRealtime(6);
        
        Destroy(gameObject);

    }

    IEnumerator Real()
    {

        yield return new WaitForSecondsRealtime(16);

        Destroy(gameObject);

    }

}
