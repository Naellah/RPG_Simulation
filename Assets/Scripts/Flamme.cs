using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flamme : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("je suis touché");
            other.gameObject.GetComponent<PlayerCharacter>().Point_de_vie-=5;
        }
    }
}
