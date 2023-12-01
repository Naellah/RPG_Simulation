using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class minimap_handle : MonoBehaviour
{
    public Texture2D cursorTexture;
    public Transform blue_IconPrefab; // Préfab de l'icône du joueur
    public Transform red_IconPrefab;  // Préfab de l'icône de l'ennemi
    public Transform minimapImage; // Référence à l'image de la minimap
    public Sprite blueTeamMarker; // Image de marquage pour l'équipe bleue
    public Sprite redTeamMarker;
    public GameManager gameManager; 
    private Dictionary<PlayerCharacter, Transform> playerIcons = new Dictionary<PlayerCharacter, Transform>();

    private void Start()
    {
        // Recherchez l'image de la minimap en tant qu'enfant de cet objet.
        minimapImage = transform.Find("MinimapImage");
    }

    private void UpdateMinimap()
{
    if (gameManager != null)
    {
        List<PlayerCharacter> charactersToRemove = new List<PlayerCharacter>();

        foreach (PlayerCharacter characterIcon in playerIcons.Keys)
        {
            // Si le joueur n'est pas présent dans la liste du GameManager, supprimez son icône.
            if (!gameManager.characters.Contains(characterIcon))
            {
                Destroy(playerIcons[characterIcon].gameObject);
                charactersToRemove.Add(characterIcon);                                          // SUPPRESSION ICONES JOUEURS MORT SUR LA MINIMAP
            }
        }

        foreach (PlayerCharacter characterToRemove in charactersToRemove)
        {
            playerIcons.Remove(characterToRemove);
        }

        foreach (PlayerCharacter character in gameManager.characters)
        {
            // Obtenez les positions des joueurs et ennemis dans le monde
            // et convertissez-les en coordonnées de la minimap.
            Vector3 playerWorldPosition = character.transform.position;
            Vector2 playerMinimapPosition = world_to_map(playerWorldPosition);

            // Créez ou mettez à jour les icônes sur la minimap.
            if (playerIcons.ContainsKey(character))
            {
                playerIcons[character].position = playerMinimapPosition;
            }
            else
            {
                if (character.team == Team.Bleu)
                {
                    Transform playerIcon = Instantiate(blue_IconPrefab, playerMinimapPosition, Quaternion.identity);            
                    playerIcon.SetParent(minimapImage);
                    playerIcons.Add(character, playerIcon);
                }
                else                                                                                                                    // CREATION ICONES JOUEURS 
                {
                    Transform playerIcon = Instantiate(red_IconPrefab, playerMinimapPosition, Quaternion.identity);
                    playerIcon.SetParent(minimapImage);
                    playerIcons.Add(character, playerIcon);
                }
            }
        }
    }
}




    Vector2 world_to_map(Vector3 position) 
{
    // Obtenez les dimensions de la minimap
    RectTransform minimapRectTransform = minimapImage.GetComponent<RectTransform>();
    float minimapWidth = minimapRectTransform.rect.width;
    float minimapHeight = minimapRectTransform.rect.height;

    // Obtenez les dimensions de la map
    float mapWidth = 450; // Remplacez par la largeur de votre map
    float mapHeight = 450; // Remplacez par la hauteur de votre map

    // Calculez les ajustements en fonction de la taille de la minimap
    float xOffset = minimapWidth / 2;
    float yOffset = minimapHeight / 2;

    // Utilisez ces ajustements pour la conversion
    Vector2 playerMinimapPosition = new Vector2(
        (position.x / mapWidth) * minimapWidth + minimapImage.position.x - xOffset,
        (position.z / mapHeight) * minimapHeight + minimapImage.position.y - yOffset
    );

     return playerMinimapPosition;
}

    private void Update()
    {
        UpdateMinimap();
    }




   /* private void HandleCharacterSelected(int characterID)
    {
    foreach (var entry in playerIcons)
        {
            PlayerCharacter character = entry.Key;
            Transform icon = entry.Value;

            if (character.characterID == characterID)
            {
                GameObject cursorObject = new GameObject("Cursor");
                RectTransform cursorTransform = cursorObject.AddComponent<RectTransform>();                                                      // BROUILLON
                Image cursorImage = cursorObject.AddComponent<Image>();
                cursorImage.sprite = Sprite.Create(cursorTexture, new Rect(0, 0, cursorTexture.width, cursorTexture.height), Vector2.one * 0.5f);
                cursorTransform.SetParent(icon);
                cursorTransform.anchoredPosition = Vector2.zero; // Position au-dessus de l'icône du joueur
            }
        }
    }*/

    private void HandleCharacterSelected(int characterID)
    {
    foreach (var entry in playerIcons)
        {
            PlayerCharacter character = entry.Key;
            Transform icon = entry.Value;

            if (character.characterID == characterID)
            {
                icon.GetComponent<Image>().color=Color.yellow;
            }
            else
            {
                if (character.team == Team.Bleu)
                {           
                    icon.GetComponent<Image>().color = Color.blue;                      // ON UTILISE L'EVENEMENT POUR AFFICHER LES ICONES DE LA COULEUR DE L'EQUIPE DU PERSONNAGE 
                }                                                                       // ET EN JAUNE POUR LE PERSONNAGE SUR LEQUEL ON EST (CAMERA)
                else 
                {
                    icon.GetComponent<Image>().color = Color.red;
                }
                
            }
        }
    }

    private void OnDisable()
    {
        // Désabonnez-vous de l'événement OnCharacterSelected.
           
    }

}

