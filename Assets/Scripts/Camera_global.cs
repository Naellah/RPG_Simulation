using UnityEngine;

public class CameraGlobal : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float zoomSpeed = 5.0f;
    public LayerMask characterLayer;

    public Camera cam;
    private Vector3 lastMousePosition;
    private Vector3 targetPosition;
    private bool isMoving = false;

    void Update()
    {
        HandleMouseInput();
        MoveCamera();
        HandleZoom();
        activate_self_cam();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
            targetPosition = transform.position;
            isMoving = true;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Vérifiez si l'objet cliqué est un personnage
                PlayerCharacter character = hit.collider.GetComponent<PlayerCharacter>();

                if (character != null)
                {
                    // C'est un personnage, faites quelque chose (par exemple, changer de caméra)
                    //Debug.Log("Clic sur le personnage : " + character.name);
                    SwitchToCharacterCamera(character.gameObject);
                    // Ajoutez votre logique pour changer de caméra ici
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isMoving = false;
        }
    }

    private void MoveCamera()
    {
        if (isMoving)
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            Vector3 moveDirection = new Vector3(-mouseDelta.x, 0, -mouseDelta.y);

            Vector3 newPosition = targetPosition + moveDirection * moveSpeed * Time.deltaTime;
            transform.position = newPosition;

            lastMousePosition = Input.mousePosition;
            targetPosition = newPosition;
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Vector3 zoomDirection = transform.forward * scroll * zoomSpeed * 20.0f * Time.deltaTime;

        Vector3 newPosition = transform.position + zoomDirection;
        transform.position = newPosition;
    }


    private void SwitchToCharacterCamera(GameObject character)
    {
        // Faites quelque chose ici pour changer la caméra en fonction du personnage cliqué
        // Par exemple, vous pouvez activer la caméra du personnage et désactiver les autres caméras
        cam.enabled = false;
        SetKnightInfo(character.name);
 
    }

    void SetKnightInfo(string knightName)
    {
        // Analyse du nom du chevalier
        string[] nameParts = knightName.Split('_');

        if (nameParts.Length == 3)
        {
            // Récupération de la couleur et du numéro
            string type = nameParts[0];
            string color = nameParts[1];
            int knightNumber = int.Parse(nameParts[2]);

            // Stockage dans PlayerPrefs

            if (type == "Knight")
            {
                PlayerPrefs.SetInt("KnightType", 0);
            }
            else if (type == "Archer")
            {
                PlayerPrefs.SetInt("KnightType", 1);
            }
            if (color == "red")
            {
                PlayerPrefs.SetInt("KnightColor", 0);  // 0 pour rouge
            }
            else if (color == "blue")
            {
                PlayerPrefs.SetInt("KnightColor", 1);  // 1 pour bleu
            }

            PlayerPrefs.SetInt("KnightNumber", knightNumber);

            //Debug.Log("Informations du chevalier stockées : Couleur = " + color + ", Numéro = " + knightNumber);
        }
        else
        {
            //Debug.Log("Nom de chevalier invalide : " + knightName);
        }
    }

    private void activate_self_cam()
    {
        //recactive cam si la touche p est rentré
        if (Input.GetKey(KeyCode.P) || Input.GetKey(KeyCode.P)) { 
            cam.enabled = true;
        }
    }

    
}
