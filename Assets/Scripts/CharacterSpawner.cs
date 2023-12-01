using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSpawner : MonoBehaviour
{
    public GameObject Knight_red;
    public GameObject Knight_blue;
   
    


    void Awake()
    {
        SpawnCharacters();
        
    }

    void SpawnCharacters()
    {
        int blueKnightCount = PlayerPrefs.GetInt("BlueKnightCount");
        int redKnightCount = PlayerPrefs.GetInt("RedKnightCount");
        
       
        for (int i = 0; i < redKnightCount; i++)
        {
            GameObject knightRed = Instantiate(Knight_red, GetRandomSpawnPosition(), Quaternion.identity);
            knightRed.name = "Knight_red_" + (i + 1);  // Renomme l'objet
            knightRed.SetActive(true);
            
            //Debug.Log("je spawn un knight red");
            
            if (i != 0)
            {
                Camera cameraDuChevalier = knightRed.GetComponentInChildren<Camera>();
               
                cameraDuChevalier.gameObject.SetActive(false);

            }
            

        }

        for (int i = 0; i < blueKnightCount; i++)
        {   

            GameObject knightBlue = Instantiate(Knight_blue, GetRandomSpawnPosition(), Quaternion.identity);
            knightBlue.name = "Knight_blue_" + (i + 1);  // Renomme l'objet
            knightBlue.SetActive(true);
            /*
            Debug.Log("je spawn un knight blue");
            
            Camera cameraDuChevalier = knightBlue.GetComponentInChildren<Camera>();
            cameraDuChevalier.gameObject.SetActive(false);
            */
        }

       
    }

    Vector3 GetRandomSpawnPosition()
    {
        float terrainHeight = 0f;
        Vector3 randomSpawnPosition;

        // Essaie de générer une position sans obstacle pendant un certain nombre d'essais
        int maxAttempts = 30;
        int attempts = 0;

        do
        {
            randomSpawnPosition = new Vector3(Random.Range(100.0f, 350.0f), 100f, Random.Range(100.0f, 350.0f));

            // Raycast vers le bas pour trouver la surface du terrain
            RaycastHit hit;
            Ray ray = new Ray(randomSpawnPosition + Vector3.up * 50f, Vector3.down);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                terrainHeight = hit.point.y;
            }

            // Vérifie si la position est sans obstacle
            bool isObstacleFree = !Physics.Raycast(randomSpawnPosition, Vector3.up, 2f);

            if (isObstacleFree)
            {
                // Sort de la boucle si la position est sans obstacle
                break;
            }

            // Si la position a un obstacle, réessaye jusqu'à atteindre le nombre maximum d'essais
            attempts++;
        } while (attempts < maxAttempts);

        return new Vector3(randomSpawnPosition.x, terrainHeight, randomSpawnPosition.z);
    }


}
