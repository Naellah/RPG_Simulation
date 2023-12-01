using UnityEditor.Timeline;
using UnityEngine;

public class DragonFlight : MonoBehaviour
{
    private float flyingAltitude = 35f;
    private float descentAltitude = 2f;
    private float movementSpeed = 5f;
    private float changeDestinationInterval = 700f;
    private float currentAltitude;
    private float lerpSpeed = 0.75f;

    private Vector3 currentDestination;
    private float timeSinceLastDestinationChange;
    private float timeLastAttack = 0f;
    private readonly float timeBetweenAttack = 25f;
    public bool isAttacking = false;
    public bool isFlaming = false;
    public bool isTakingOff = false;
    public bool isFlying = true;

    void Start()
    {
        // Initialisation de la première destination
        currentAltitude = flyingAltitude;
        SetRandomDestination();
    }

    void Update()
    {
        // Vérifier si le dragon doit changer de destination
        timeSinceLastDestinationChange += Time.deltaTime;
        timeLastAttack += Time.deltaTime;
        if (timeSinceLastDestinationChange >= changeDestinationInterval && isAttacking!= false)
        {
            SetRandomDestination();
            timeSinceLastDestinationChange = 0f;
        }

        // Régler la hauteur du dragon à l'altitude de vol
        float targetAltitude = isAttacking || isFlaming? descentAltitude : flyingAltitude;

        currentAltitude = Mathf.Lerp(currentAltitude, targetAltitude, lerpSpeed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, currentAltitude, transform.position.z);

        if(timeLastAttack >= timeBetweenAttack) 
        {
            
            detecteChevalier();
            timeLastAttack = 0f;


        }

        if (isAttacking)
        {
            movementSpeed = 7f;
            if (currentDestination == transform.position)
            {
                var attack_dragon = GetComponent<Attack_dragon>();
                movementSpeed = 15f;
                isAttacking = false;
                isFlaming = true;
                attack_dragon.Flam();
            }
            MoveDragon();
            RotateDragon();
        }

        // Ajout des comportements spécifiques à isFlaming, isTakingOff, isFlying
        else if (isFlaming)
        {
            FlameBreath();
        }
        else if (isTakingOff)
        {
            TakeOff();
            timeLastAttack = 0f;
            MoveDragon();
        }
        else if (isFlying)
        {
            MoveDragon();
        }

        // Incliner le dragon vers la destination lors de l'attaque
       

        // Debug.Log("état du dragon : " + isAttacking + " " + isFlaming + " " + isTakingOff + " " + isFlying);
    }

    void MoveDragon()
    {
        // Déplacer le dragon
        transform.position = Vector3.MoveTowards(transform.position, currentDestination, movementSpeed * Time.deltaTime);
    }

    void RotateDragon()
    {
        // Incliner le dragon vers la destination
        Vector3 moveDirection = (currentDestination - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * 2f);
    }

    void SetRandomDestination()
    {
        // Choisir une destination aléatoire dans la scène
        currentDestination = new Vector3(Random.Range(100f, 350f), descentAltitude, Random.Range(100f, 350f));
    }

    public void StartAttack()
    {
        isAttacking = true;
    }

    public void StopAttack()
    {
        isAttacking = false;
    }

    public void FlameBreath()
    {
        // Comportement spécifique à l'attaque de flamme (avancer tout droit, par exemple)
        Vector3 forwardDirection = transform.forward;
        transform.position += forwardDirection * movementSpeed * Time.deltaTime;
    }

    void TakeOff()
    {
        // Comportement spécifique au décollage (monter, par exemple)
        Vector3 upDirection = Vector3.up;
        transform.position += upDirection * movementSpeed * Time.deltaTime;

        float targetAltitude = 35f;

        if (transform.position.y >= targetAltitude)
        {
            // Atteint l'altitude cible, ajuste la position exacte
            transform.position = new Vector3(transform.position.x, targetAltitude, transform.position.z);

            // Assurez-vous que le dragon est droit
            transform.rotation = Quaternion.identity;
        }




        else
        {
            // Incliner le dragon vers le haut pendant le décollage
            Vector3 moveDirection = upDirection;
            Quaternion lookRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * 2f);
        }
    }


    public void detecteChevalier()
    {
        // Regarde si un chevalier est dans un rayon de 50m autour du dragon avec y = 0 car les chevaliers sont sur le sol
        isFlying = false;
        StartAttack();
        Collider[] hitColliders = Physics.OverlapSphere(new Vector3(transform.position.x, 1, transform.position.z), 50f);
        foreach (var collider in hitColliders)
        {
            if (collider.gameObject.tag == "Player")
            {
                Debug.Log("Je vais attaquer " + collider.gameObject.name);
                Vector3 target = collider.gameObject.transform.position;

                // Utilisez une interpolation pour déplacer progressivement le dragon vers la position du chevalier
                currentDestination = Vector3.Lerp(currentDestination, new Vector3(target.x, descentAltitude, target.z), Time.deltaTime * movementSpeed);

                break;
            }
        }

        // Si le tableau est vide, le dragon continue de voler
        if (hitColliders.Length == 0)
        {
            Debug.Log("Je continue de voler");
            isFlying = true;
            StopAttack();
        }
    }


}
