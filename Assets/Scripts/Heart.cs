using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heart : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other == null || other.GetComponent<PlayerCharacter>().ancien_leader == null)
        {
            // G?rer le cas o? 'other' est null
            return;
        }
        if (other.tag == "Player")
        {
            other.GetComponent<PlayerCharacter>().Point_de_vie = +30;
            Destroy(gameObject);
            other.GetComponent<PlayerCharacter>().present = true;
            other.GetComponent<PlayerCharacter>().en_train_de_chercher = false;

            other.GetComponent<PlayerCharacter>().JoinGroup(other.GetComponent<PlayerCharacter>().ancien_leader.currentGroup);



        }


    }
}
