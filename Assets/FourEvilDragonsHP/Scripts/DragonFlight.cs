#if UNITY_EDITOR



using System.Collections;
using UnityEditor.Timeline;
using UnityEngine;


namespace Dragon
{
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
        private bool isAttacking = false;
        private bool isFlaming = false;
        private bool isTakingOff = false;
        private bool isFlying = true;


        public Animator anim;
        public Transform point_flame_breath;
        public GameObject flame_breath;

        void Start()
        {
            // Initialisation de la première destination
            currentAltitude = flyingAltitude;
            SetRandomDestination();
        }

        void Update()
        {
            // Vérifier si le dragon doit changer de destination
            Attack();
            timeSinceLastDestinationChange += Time.deltaTime;
            timeLastAttack += Time.deltaTime;
            if (timeSinceLastDestinationChange >= changeDestinationInterval && isAttacking != false)
            {
                SetRandomDestination();
                timeSinceLastDestinationChange = 0f;
            }

            // Régler la hauteur du dragon à l'altitude de vol
            float targetAltitude = isAttacking || isFlaming ? descentAltitude : flyingAltitude;

            currentAltitude = Mathf.Lerp(currentAltitude, targetAltitude, lerpSpeed * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, currentAltitude, transform.position.z);

            if (timeLastAttack >= timeBetweenAttack)
            {

                detecteChevalier();
                timeLastAttack = 0f;


            }

            if (isAttacking)
            {
                movementSpeed = 7f;
                if (currentDestination == transform.position)
                {
                    
                    movementSpeed = 15f;
                    isAttacking = false;
                    isFlaming = true;
                    Flam();
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
            currentDestination = new Vector3(Random.Range(100f, 350f), 35f, Random.Range(100f, 350f));
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


      

      

        public void Attack()
        {
           
            if (isAttacking)
            {


                anim.SetTrigger("Attack");

                isFlying = false;


            }
        }

        public void Flam()
        {


            anim.SetTrigger("Flame");
            StartCoroutine(TakeOffRoutine());

        }

        public IEnumerator TakeOffRoutine()
        {
            yield return new WaitForSeconds(4f);

           


            isFlaming = false;
            isTakingOff = true;

            anim.SetTrigger("TakeOff");

            StartCoroutine(TakeOff1());
        }


        public IEnumerator TakeOff1()
        {
            yield return new WaitUntil(() => Mathf.Approximately(transform.position.y, 34.5f));


            
            isTakingOff = false;
            isFlying = true;

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

}


#endif