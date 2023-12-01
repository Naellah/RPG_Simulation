using System.Collections;
using UnityEngine;

public class Attack_dragon : MonoBehaviour
{
    public Animator anim;
    public Transform point_flame_breath;
    public GameObject flame_breath;

    void Update()
    {
        Attack();
       
    }

    void Attack()
    {
        var dragonFlight = GetComponent<DragonFlight>();
        if (dragonFlight.isAttacking)
        {
            
           
            
            anim.SetTrigger("Attack");  
           
            dragonFlight.isFlying = false;
            
            
        }
    }

    public void Flam()
    {
       
           
        anim.SetTrigger("Flame");
        StartCoroutine(TakeOffRoutine());
        
    }

    IEnumerator TakeOffRoutine()
    {
        yield return new WaitForSeconds(4f);

        var dragonFlight = GetComponent<DragonFlight>();
        
        
        dragonFlight.isFlaming = false;
        dragonFlight.isTakingOff = true;

        anim.SetTrigger("TakeOff");
       
        StartCoroutine(TakeOff());
    }


    IEnumerator TakeOff()
    {
        yield return new WaitUntil(() => Mathf.Approximately(transform.position.y, 34.5f));

        

        GetComponent<DragonFlight>().isTakingOff = false;
        GetComponent<DragonFlight>().isFlying = true;

        anim.SetTrigger("Fly");
    }



    public void Flame_breath()
    {
        
        Debug.Log("je crache du feu");
        GameObject flammePrincipale = Instantiate(flame_breath, point_flame_breath.position, point_flame_breath.rotation);
        flammePrincipale.SetActive(true);

        float delayBetweenFlames = 0.25f;

        for (int i = 0; i < 8; i++)
        {
            Quaternion rotationOffset = Quaternion.Euler(0, i * 45, 0);
            float delay = i * delayBetweenFlames;
            StartCoroutine(CreateAdditionalFlame(delay));
        }

        float destructionDelay = 2.0f;
        Destroy(flammePrincipale, destructionDelay);
    }

    IEnumerator CreateAdditionalFlame(float delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject flammeSupplementaire = Instantiate(flame_breath, point_flame_breath.position, point_flame_breath.rotation);
        flammeSupplementaire.SetActive(true);
        Destroy(flammeSupplementaire, 1.25f);
    }
}
