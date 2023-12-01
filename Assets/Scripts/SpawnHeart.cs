using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnHeart : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject heart;

    public int nb_heart;
    void Awake()
    {
        SpawnHearts();
    }

    // Update is called once per frame

    void SpawnHearts()
    {
        for (int i = 0; i < nb_heart; i++)
        {
            Instantiate(heart, GetRandomSpawnPosition(), Quaternion.identity);
            heart.name = "heart_" + (i + 1);  // Renomme l'objet
            heart.SetActive(true);
        }
    }

    public IEnumerator Wait(float time)
    {
        yield return new WaitForSecondsRealtime(time);  // Attendez un certain temps en temps réel

    }

    Vector3 GetRandomSpawnPosition()
    {
        float terrainHeight = 1f;
        Vector3 randomSpawnPosition;

        // Essaie de générer une position sans obstacle pendant un certain nombre d'essais
        int maxAttempts = 30;
        int attempts = 0;

        do
        {
            randomSpawnPosition = new Vector3(Random.Range(100.0f, 350.0f), 100f, Random.Range(100.0f, 350.0f));

            // Raycast vers le bas pour trouver la surface du terrain
            

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
