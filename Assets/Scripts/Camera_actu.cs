using UnityEngine;

public class Camera_actu : MonoBehaviour
{
    public GameObject classe;
    public GameObject cam;
    public int couleur;
    public int numero;
    public int type_combattant;

    void Start()
    {
        // Récupération des informations du chevalier
        Invoke("SetKnightInfo",3.0f);
    }

    void Update()
    {
        // Vérifie si les informations stockées dans PlayerPrefs correspondent à ce chevalier
        int storedCouleur = PlayerPrefs.GetInt("KnightColor", -1);
        int storedNumero = PlayerPrefs.GetInt("KnightNumber", -1);
        int storedType = PlayerPrefs.GetInt("KnightType", -1);

        if (storedCouleur == couleur && storedNumero == numero)
        {
            // Active la caméra si les informations correspondent
            //Debug.Log("changement de camera");
            cam.SetActive(true);
        }
        else
        {
            cam.SetActive(false);
        }

        //teste si la caméra est activée
        /*
        if (cam.activeSelf)
        {
            Debug.Log(classe.name + classe.GetComponent<PlayerCharacter>().Point_de_vie );
        }*/



       
    }

    void SetKnightInfo()
    {
        // Analyse du nom du chevalier


        string[] nameParts = classe.name.Split('_');
        //Debug.Log("je suis dans setknightinfo");
        if (nameParts.Length == 3)
        {
            // Récupération de la couleur et du numéro
            string type = nameParts[0];
            if (type == "Knight")
            {
                type_combattant = 0;
            }
            else if (type == "Archer")
            {
                type_combattant = 1;
            }
            
            string color = nameParts[1];
            numero = int.Parse(nameParts[2]);

            // Stockage dans les variables du script
            if (color == "red")
            {
                couleur = 0;  // 0 pour rouge
            }
            else if (color == "blue")
            {
                couleur = 1;  // 1 pour bleu
            }
        }
        else
        {
            //Debug.Log("Nom de chevalier invalide : " + gameObject.name);
        }
    }
    
    
          
}
