using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Microlight.MicroBar;
using Unity.VisualScripting;
using UnityEngine.Assertions;
using UnityEngine.TextCore.Text;
using TMPro;
using System.Linq;

public enum Team
{
    Rouge,
    Bleu,
}

public enum Personality
{
    Courageux, // Plus grande probabilité d'attaquer
    Peureux,   // Moins grande probabilité d'attaquer
}

public enum CharacterState
{
    Normal,

    Reflexion,

    Fuite,
    Attaque,
}

public class DeathInfo
{
    public PlayerCharacter tueur;
    public PlayerCharacter victime;
    public float heureMort;
}

public class PlayerCharacter : MonoBehaviour
{
    public Animator anim;
    public NavMeshAgent agent;
    public Transform terrain;
    public Team team;
    public Personality personality;
    private float point_de_vie = 100;
    private const float pv_max = 100;
    private int degats;
    public float detectionRadius = 15f;
    public float randomDestinationInterval = 3f;
    public CharacterState currentState = CharacterState.Normal;
    private float timeSinceLastAttack = 0f;
    public PlayerCharacter targetEnemy = null;
    public float attackCooldown = 3.0f; // 3 secondes de délai entre les attaques
    private GameManager gameManager;
    [SerializeField] MicroBar microBar;
    //UPDATE





    public float Point_de_vie
    {
        get => point_de_vie;
        set
        {
            point_de_vie = value;
            microBar.UpdateHealthBar(point_de_vie);
        }
    }

    private PlayerCharacter renforts;
    public int nb_ennemis = 0;
    public int nb_allier = 0;
    public List<HelpMessage> helpMessages = new List<HelpMessage>();
    public bool ecoute = true;
    public TextMeshPro textMesh;
    public TextMeshPro messages;
    private float timeInAttaqueState = 0.0f;
    private float delaiAvantModeNormal = 1.0f;
    public List<PlayerCharacter> charactersMessageSent = new List<PlayerCharacter>();
    private Coroutine checkDistanceCoroutine;
    public bool isHelping = false;
    public bool helped = false;
    private bool previent_allie = false;
    public int characterID; // Ajoutez cette variable
    private static int nextUniqueID = 1;
    public List<PlayerCharacter> Allies_liste = new List<PlayerCharacter>();
    public List<PlayerCharacter> Ennemis_liste = new List<PlayerCharacter>();
    public Group currentGroup; // Référence au groupe actuel du personnage
    public delegate void EncounterEventHandler(PlayerCharacter j1, PlayerCharacter j2);
    public event EncounterEventHandler OnCharacterEncounter;
    private Dictionary<string, bool> encounteredPairs = new Dictionary<string, bool>();
    public bool is_leader = false;
    public float maxEnergie = 100f;
    public float currentEnergie = 100f;
    public float energieDepletionRate = 10f;
    public float energieRecoveryRate = 5f;
    public bool en_train_de_chercher = false;
    public float timetirage = 0f;
    public float tiragecooldown = 5f;
    private Coroutine Deplacements;
    public bool present = true;
    public PlayerCharacter ancien_leader;
    private GameObject effet_heal;





    /// //////////////////
    public delegate void DeathEventHandler(DeathInfo infos);
    public event DeathEventHandler OnDeath;

    /// ////////////////




    public void AddHelpMessage(HelpMessage message)
    {
        helpMessages.Add(message);
        //
    }

    public class HelpMessage
    {
        //public int messageID;
        public string message;
        public PlayerCharacter sender;
        public int ennemis;
        // public Vector3? pos;



        public HelpMessage(/*int messageID,*/ string message, PlayerCharacter sender, int ennemis/*, Vector3? pos*/)
        {
            //this.messageID = messageID; // Identifiant unique du message
            this.message = message;
            this.sender = sender;
            this.ennemis = ennemis;
            //this.pos= pos;
        }
    }
    //UPDATE

    private void Start()
    {
        microBar.Initialize(pv_max);
        Point_de_vie = pv_max;

    }








    public void JoinGroup(Group group)
    {
        if (currentGroup != null)
        {
            currentGroup.RemoveMember(this); // Quitter le groupe actuel
            if (currentGroup.taille == 0)
            {
                gameManager.groups.Remove(currentGroup);
            }
        }
        currentGroup = group;
        group.AddMember(this); // Rejoindre le nouveau groupe
    }

    public void LeaveGroup()
    {
        if (currentGroup != null)
        {
            ancien_leader = currentGroup.leader;
            currentGroup.RemoveMember(this);
            currentGroup = null;
        }
    }
    public void leave_Temporaire()
    {
        present = false;
    }


    public Group CreateGroup()
    {
        if (currentGroup != null)
        {
            return null;
        }
        Group newGroup = new Group(this); // Crée un nouveau groupe avec le joueur spécifié comme leader
        this.JoinGroup(newGroup); // Ajoute le joueur au groupe
        gameManager.groups.Add(newGroup); // Ajoute le groupe à la liste des groupes

        return newGroup;
    }

    public bool Should_change_group(Group autre)
    {
        if (personality == Personality.Peureux)
        {
            bool changer = (autre.taille <= currentGroup.taille) ? false : true;
            return changer;
        }
        return false;
    }

    public float calcul_puissance()
    {
        float ratio_pv = Point_de_vie / pv_max;
        float puissance = degats * ratio_pv;
        return puissance;
    }

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        gameManager.AddCharacter(this);
        effet_heal = gameManager.effet;
        Deplacements = StartCoroutine(RandomDestinationRoutine());
        characterID = GenerateUniqueID();
        degats = (personality == Personality.Courageux) ? UnityEngine.Random.Range(20, 36) : UnityEngine.Random.Range(15, 31);
    }

    private int GenerateUniqueID()
    {
        int uniqueID = nextUniqueID;
        nextUniqueID++; // Incrémente l'identifiant unique pour le prochain personnage.
        return uniqueID;
    }

    private IEnumerator RandomDestinationRoutine()
    {
        while (true)
        {
            // Attendez que le personnage atteigne sa destination actuelle.
            while (Vector3.Distance(transform.position, agent.destination) > agent.stoppingDistance)
            {
                yield return null;
            }

            // Une fois que le personnage a atteint sa destination, attendez un certain temps.
            yield return new WaitForSeconds(randomDestinationInterval);

            // Après l'attente, définissez une nouvelle destination aléatoire.
            SetRandomDestination();
        }
    }

    private IEnumerator FollowLeader(Transform leader)
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        while (true)
        {
            if (leader != null)
            {
                // Calcule la direction vers le leader
                Vector3 leaderPosition = leader.position;
                Vector3 followDirection = leaderPosition - transform.position;

                if (followDirection.magnitude > 5)
                {
                    agent.SetDestination(leaderPosition);
                }
            }

            yield return new WaitForSeconds(0.1f); // Attendre un court laps de temps avant la prochaine mise à jour
        }
    }

    private void SetRandomDestination()
    {
        if (Ennemis_liste.Count == 0)
        {
            detectionRadius += 10;
        }

        Vector3 randomDestination = GetRandomPositionInTerrain();
        Vector3 test = new Vector3(50, 0, 50);
        agent.SetDestination(randomDestination);
    }

    private Vector3 GetRandomPositionInTerrain()
    {
        float randomX = Random.Range(1.0f, 100.0f);
        float randomZ = Random.Range(1.0f, 100.0f);
        return new Vector3(randomX, 0f, randomZ);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red; // Choisissez la couleur que vous préférez.
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    private string GetMessagesText()
    {
        string messageText = "";

        foreach (HelpMessage message in helpMessages)
        {
            // Vous pouvez formater le message comme vous le souhaitez ici
            messageText += message.sender.name + ": " + message.message + " " + message.sender.transform.position + "\n";
        }

        return messageText;
    }


    private void Update()
    {
        timeSinceLastAttack += Time.deltaTime;
        timetirage += Time.deltaTime;
        float previousHealth = Point_de_vie;

        switch (currentState)
        {
            case CharacterState.Normal:
                HandleNormalState();
                break;

            case CharacterState.Fuite:
                HandleFuiteState();
                break;

            case CharacterState.Attaque:
                HandleAttaqueState();
                break;
        }

        if (is_leader && currentGroup != null)
        {
            currentGroup.UpdateGroupState();
        }
        if (currentGroup != null && !is_leader && currentGroup.leader != null && Deplacements != null && !en_train_de_chercher && present)
        {
            StopCoroutine(Deplacements);
            Deplacements = StartCoroutine(FollowLeader(currentGroup.leader.transform));

        }
        /*if ((en_train_de_chercher)&&(randomDestinationCoroutine != null))
        {
            StopCoroutine(randomDestinationCoroutine);
            randomDestinationCoroutine = null;
        }*/

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Coeur")
        {
            // Instancier l'effet de soin
            GameObject healEffect = Instantiate(effet_heal, transform.position, Quaternion.identity);

            // Attacher l'effet de soin en tant qu'enfant du personnage
            healEffect.transform.parent = transform;

            // Vous pouvez également détruire l'effet après un certain temps si nécessaire
            Destroy(healEffect, 4);
        }
    }

    /// <summary>
    /// //////////////////////////////////////// STATE ////////////////////////////////////////////////////////////
    /// </summary>


    private void HandleNormalState()
    {
        DetectNearbyCharacters();

        if (timetirage >= tiragecooldown && !en_train_de_chercher && Point_de_vie != pv_max)
        {
            should_take_heal();
            timetirage = 0.0f;
        }

        if (Ennemis_liste.Count() != 0 && !isHelping)
        {
            if (currentGroup == null && personality == Personality.Peureux)
            {
                currentState = (ShouldAttack()) ? CharacterState.Attaque : CharacterState.Fuite;
            }
            else
            if (currentGroup == null && personality == Personality.Courageux)
            {
                currentState = CharacterState.Attaque;
            }
            if (currentGroup != null && is_leader)
            {
                HandleLeaderState();
            }

            else if (!isHelping && helpMessages.Count != 0)
            {
                //HandleDecision();
            }


        }
    }
    private void HandleLeaderState()
    {

        if (Ennemis_liste[0] != null && Ennemis_liste[0].currentGroup != null)
        {
            float puissance_adverse = Ennemis_liste[0].currentGroup.puissance;
            float notre_puissance = this.currentGroup.puissance;

            if (puissance_adverse < notre_puissance)
            {
                targetEnemy = Ennemis_liste[0];
                SetGroupStateForMembers(CharacterState.Attaque, targetEnemy);
            }
            else
            {
                targetEnemy = Ennemis_liste[0];
                SetGroupStateForMembers(CharacterState.Fuite, targetEnemy);
            }
        }
        else
        {
            SetGroupStateForMembers(CharacterState.Normal, null);
        }

        if (Ennemis_liste[0] != null && Ennemis_liste[0].currentGroup == null)
        {
            targetEnemy = Ennemis_liste[0];
            SetGroupStateForMembers(CharacterState.Attaque, targetEnemy);
        }

    }




    private void SetGroupStateForMembers(CharacterState nextState, PlayerCharacter target)
    {
        foreach (PlayerCharacter member in currentGroup.members)
        {
            member.FollowLeaderState(nextState, target);
        }
    }

    private void FollowLeaderState(CharacterState nextState, PlayerCharacter target)
    {
        // Votre logique pour suivre le leader ici
        if (target != null)
        {
            currentState = nextState;
            targetEnemy = target;
        }
    }


    private void HandleAttaqueState()
    {

        Best_ennemy_to_attack();
        ecoute = false;
        if (targetEnemy == null || targetEnemy.Point_de_vie <= 0)
        {
            DetectNearbyCharacters();

            //anim.SetBool("IsCombat", false);//currentState = CharacterState.Normal;
            timeInAttaqueState += Time.deltaTime;
            if (timeInAttaqueState >= delaiAvantModeNormal)
            {
                ecoute = true;
                currentState = CharacterState.Normal;
                targetEnemy = null;
                timeInAttaqueState = 0f;
                previent_allie = false;
                detectionRadius = 15f;
                //ecoute = true;

                if (currentGroup.leader.transform != null)
                {
                    StartCoroutine(FollowLeader(currentGroup.leader.transform));
                }

            }

            return;
        }


        if (targetEnemy != null)
        {
            //anim.SetBool("IsCombat", false);

            DetectNearbyCharacters();
            Best_ennemy_to_attack();
            if (targetEnemy != null)
            {
                SuivreEnnemi(targetEnemy.transform);
            }


            if (IsInAttackRange(targetEnemy.transform) && targetEnemy!=null)
            {
                if (timeSinceLastAttack >= attackCooldown)
                {
                    nb_ennemis = count_nb_ennemis();
                    nb_allier = count_nb_allier();
                    //

                    if (nb_ennemis >= nb_allier)
                    {
                        SendRequestForHelp(this, "J'ai besoin de renfort", 25f, nb_ennemis, null);
                    }
                    anim.SetTrigger("IsAttack");
                    Attack(targetEnemy);
                    timeSinceLastAttack = 0.0f;

                }
                else
                {

                }
            }
            else
            {
                if (targetEnemy.currentState == CharacterState.Attaque)
                {

                    if (!previent_allie && targetEnemy.targetEnemy != null && targetEnemy.targetEnemy.currentState == CharacterState.Fuite)
                    {
                        SendRequestForHelp(this, "Je te prévient", 21, 1, targetEnemy.targetEnemy);
                        previent_allie = true;

                    }

                }


            }
        }
    }

    private void HandleFuiteState()
    {

        nb_ennemis = count_nb_ennemis();
        if (currentEnergie < 60.0f)
        {
            SetGroupStateForMembers(CharacterState.Attaque, targetEnemy);                   // RAJOUTER
        }
        //Best_ennemy_to_flee();
        if (targetEnemy == null || targetEnemy.Point_de_vie <= 0 || nb_ennemis == 0)
        {
            currentEnergie = maxEnergie;
            currentState = CharacterState.Normal;
            targetEnemy = null;
            return;
        }
        else //si on est pas aidé
        {
            FuirEnnemi(targetEnemy.transform);
        }
        if ((helped || !helped) && currentGroup == null)
        {
            SendRequestForHelp(this, "Besoin d'aide !", 100, nb_ennemis, null);
            SendRequestForHelp(this, "Besoin d'aide !", 100f, nb_ennemis, null);
        }

        //
    }

    private void DetectNearbyCharacters() //JE VEUX STOCKER UNE LISTE D'ENNEMIS POUR POUVOIR PARCOURIR LES ENNEMIS//
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        Ennemis_liste.Clear();
        Allies_liste.Clear();
        foreach (Collider collider in colliders)
        {
            PlayerCharacter otherPlayer = collider.GetComponent<PlayerCharacter>();

            if (otherPlayer != null && otherPlayer != this && otherPlayer.team != team)
            {
                Ennemis_liste.Add(otherPlayer);
            }
            else if (otherPlayer != null && otherPlayer != this && otherPlayer.team == team)
            {
                Allies_liste.Add(otherPlayer);
                string name1 = this.name;
                string name2 = otherPlayer.name;

                if (string.Compare(name1, name2) > 0)
                {
                    string temp = name1;
                    name1 = name2;
                    name2 = temp;
                }

                string pairKey = name1 + "-" + name2;


                if (!encounteredPairs.ContainsKey(pairKey) || !encounteredPairs[pairKey])
                {
                    encounteredPairs[pairKey] = true;
                    OnCharacterEncounter?.Invoke(this, otherPlayer); // Déclenche l'événement
                }
            }
        }
    }


    /// <summary>
    /// /////////////////////////////////////////////// ATTAQUER ////////////////////////////////////////////////////
    /// </summary>
    /// <returns></returns>
    private bool ShouldAttack()
    {
        float probabiliteAttaquePeureux = 0.0000001f;
        return Random.Range(0f, 1f) <= probabiliteAttaquePeureux;
    }

    private bool IsInAttackRange(Transform target)
    {
        float distance = Vector3.Distance(transform.position, target.position);
        float porteeAttaque = 2.0f;
        return distance <= porteeAttaque;
    }

    private void Attack(PlayerCharacter ennemi)
    {
        if (ennemi != null)
        {
            //anim.SetTrigger("IsAttack");
            ennemi.Point_de_vie -= degats;

            if (ennemi.Point_de_vie <= 0)
            {

                gameManager.RemoveCharacter(ennemi);
                Ennemis_liste.Remove(ennemi);
                Allies_liste.Remove(ennemi);
                ennemi.LeaveGroup();
                DeathInfo info = new DeathInfo();
                info.heureMort = Time.time;
                info.tueur = this;
                info.victime = ennemi;


                Destroy(ennemi.gameObject);

                OnDeath?.Invoke(info);
                // Déclenchez l'événement OnDeath pour informer le GameManager de la mort

            }
        }
    }

    private void Best_ennemy_to_attack()
    {
        float minHealth = float.MaxValue;
        PlayerCharacter best_ennemi = null;
        foreach (PlayerCharacter ennemis in Ennemis_liste)
        {
            if (ennemis.Point_de_vie < minHealth)
            {
                minHealth = ennemis.Point_de_vie;
                best_ennemi = ennemis;
            }
        }
        targetEnemy = best_ennemi;
    }


    /// ////////////////////////////////////// FUIR ET COMMUNICATION ////////////////////////////////////////////////////////////

    private void FuirEnnemi(Transform ennemi)
    {
        // Limiter l'énergie minimale à 0
        currentEnergie = Mathf.Max(currentEnergie, 0f);

        // Utiliser l'énergie pour ajuster la vitesse de fuite
        float speedMultiplier = currentEnergie / maxEnergie;

        anim.SetBool("IsCombat", false);

        // Calculer la direction et la destination
        Vector3 direction = transform.position - ennemi.position;
        Vector3 destination = transform.position + direction;

        // Définir la destination et ajuster la vitesse
        agent.SetDestination(destination);
        agent.speed = 3.5f * speedMultiplier;                                   // RAJOUTER

        // Dépléter l'énergie en fonction du temps passé en fuite
        currentEnergie -= energieDepletionRate * Time.deltaTime;

        // Ajouter un système de récupération d'énergie ici si nécessaire
        // Exemple simple : augmenter l'énergie au fil du temps
        currentEnergie += energieRecoveryRate * Time.deltaTime;

        // Limiter l'énergie maximale
        currentEnergie = Mathf.Min(currentEnergie, maxEnergie);
    }



    private void SuivreEnnemi(Transform ennemi)
    {
        //anim.SetBool("IsCombat", false);
        agent.SetDestination(ennemi.position);
    }

    private void Best_ennemy_to_flee()
    {
        float distanceMAX = float.MaxValue;
        foreach (PlayerCharacter ennemis in Ennemis_liste)
        {
            float distance = Vector3.Distance(transform.position, ennemis.transform.position);
            if (distance < distanceMAX)
            {
                distanceMAX = distance;
                targetEnemy = ennemis;
            }
        }
    }


    public void SendRequestForHelp(PlayerCharacter sender, string message, float radius, int ennemis, /*Vector3? position, */PlayerCharacter specificRecipient = null)
    {
        if (sender != null)
        {
            if ((message == "Je te prévient" || message == "j'arrive") && specificRecipient != null)
            {
                specificRecipient.ReceiveMessage(message, sender, ennemis);
            }
            else
            {
                gameManager.SendMessageToAllies(this, message, radius, ennemis);
            }
        }
    }

    public void ReceiveMessage(string message, PlayerCharacter sender, int ennemis)
    {
        StopCoroutine(RandomDestinationRoutine());

        HelpMessage receivedMessage = new HelpMessage(message, sender, ennemis);

        helpMessages.Add(receivedMessage);

        if (sender != null && (message == "j'arrive"))
        {
            HandleAnswer(sender);
        }
        else if (this.is_leader == false)
        {
            this.currentGroup.leader.ReceiveMessage(message, sender, ennemis);
        }
        else
        {
            HandleLeaderInfo();
        }
    }
    /// <summary>
    /// /////////////////////////////////////////////////  LEADER //////////////////////////////////////////////////////
    /// </summary>

    private void HandleLeaderInfo()
    {
        HelpMessage bestMessage = GetBestHelpMessage();
        if (bestMessage != null)
        {
            SendRequestForHelp(this, "j'arrive", 28f, nb_ennemis, bestMessage.sender);

            agent.SetDestination(bestMessage.sender.transform.position);

            StartCoroutine(CheckDistanceCoroutine(bestMessage.sender));
            isHelping = true;
            helpMessages.Remove(bestMessage);

        }
    }

    private void SendMessageToGroup()
    {

    }

    /// <summary>
    /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>

    private void HandleAnswer(PlayerCharacter sender)
    {
        HelpMessage answerMessage = helpMessages.FirstOrDefault(message => message.sender == sender && (message.message == "j'arrive" || message.message == "Je te prévient"));
        float distance = Vector3.Distance(sender.transform.position, transform.position);

        if (distance < 100)
        {
            //helped = true;
        }

        if (answerMessage != null)
        {
            agent.SetDestination(sender.transform.position);
            StartCoroutine(CheckDistanceCoroutine(sender));
            helpMessages.Remove(answerMessage);
        }
    }


    /*private void HandleDecision()
    {
        HelpMessage bestMessage = GetBestHelpMessage();

        if (bestMessage != null)
        {
            if (!isHelping)
            {
                SendRequestForHelp(this, "j'arrive", 28f, nb_ennemis, bestMessage.sender);
                agent.SetDestination(bestMessage.sender.transform.position);

                // Vérifiez la distance tant qu'elle est supérieure à 7 unités.
                StartCoroutine(CheckDistanceCoroutine(bestMessage.sender));
                isHelping = true;
                helpMessages.Remove(bestMessage);
            }
        }
    }*/

    private IEnumerator CheckDistanceCoroutine(PlayerCharacter target)
    {
        while (true)
        {
            if (target == null)
            {
                // La cible n'est plus valide, revenir à l'état Normal.
                isHelping = false;
                currentState = CharacterState.Normal;
                yield break; // Sortir de la boucle.
            }

            float distance = Vector3.Distance(target.transform.position, transform.position);

            if (distance < 7)
            {
                isHelping = false;
                currentState = CharacterState.Attaque;
                yield break; // Sortir de la boucle.
            }

            yield return null; // Attendre la prochaine frame pour éviter une boucle infinie.
        }
    }

    private HelpMessage GetBestHelpMessage()
    {
        HelpMessage bestMessage = null;

        foreach (HelpMessage message in helpMessages)
        {
            if (bestMessage == null)
            {
                bestMessage = message;
            }
            else if (IsHigherPriority(message, bestMessage))
            {
                bestMessage = message;
            }
        }

        return bestMessage;
    }

    private bool IsHigherPriority(HelpMessage message1, HelpMessage message2)
    {
        int priorityMessage1 = GetMessagePriority(message1);
        int priorityMessage2 = GetMessagePriority(message2);

        // Compare les priorités
        if (priorityMessage1 != priorityMessage2)
        {
            return priorityMessage1 < priorityMessage2;
        }

        // Si les priorités sont égales, compare le nombre d'ennemis
        return message1.ennemis < message2.ennemis;
    }

    private int GetMessagePriority(HelpMessage message)
    {
        if (message.message == "Besoin d'aide !")
        {
            return 0;
        }
        else if (message.message == "J'ai besoin de renfort")
        {
            return 1;
        }
        else return 2;
    }
    //////////////////////////////// COEURS //////////////////////////////////////////////////

    private void should_take_heal()
    {
        float previousHealth = Point_de_vie;
        GameObject nearestHeart = FindNearestHeart(1000);
        float distance = Vector3.Distance(transform.position, nearestHeart.transform.position);
        float ratio_pv = pv_max - Point_de_vie;
        float ratio_distance = (100 - distance) / 100;
        float chanceToHeal = (personality == Personality.Peureux) ? ratio_pv * 1.1f * ratio_distance : ratio_pv * 0.9f * ratio_distance;


        if (Random.Range(0.0f, 101.0f) < chanceToHeal)
        {
            if (!is_leader && currentGroup != null)
            {
                StopCoroutine(Deplacements);
                agent.SetDestination(nearestHeart.transform.position);
                en_train_de_chercher = true;
                LeaveGroup();
                //leave_Temporaire();
            }

            //Point_de_vie += nearestHeart.GetComponent<Heart>().healthAmount;
            //Destroy(nearestHeart);

        }
    }

    private GameObject FindNearestHeart(float radius)
    {
        GameObject[] hearts = GameObject.FindGameObjectsWithTag("Coeur");
        GameObject nearestHeart = null;
        float nearestDistance = float.MaxValue;

        foreach (GameObject heart in gameManager.Coeurs)
        {
            float distance = Vector3.Distance(transform.position, heart.transform.position);
            if (distance <= radius && distance < nearestDistance)
            {
                nearestHeart = heart;
                nearestDistance = distance;
            }
        }

        return nearestHeart;
    }


    /// //////////////////////////// UTILITAIRE //////////////////////////////////////////////


    private int count_nb_ennemis()
    {
        int nb_ennemis = 0;
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);

        foreach (Collider collider in colliders)
        {
            PlayerCharacter otherPlayer = collider.GetComponent<PlayerCharacter>();

            if (otherPlayer != null && otherPlayer.team != team)
            {
                nb_ennemis++;
            }

        }
        return nb_ennemis;
    }

    private int count_nb_allier()
    {
        int nb_allier = 0;
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);

        foreach (Collider collider in colliders)
        {
            PlayerCharacter otherPlayer = collider.GetComponent<PlayerCharacter>();

            if (otherPlayer != null && otherPlayer.team == team)
            {
                nb_allier++;
            }

        }
        return nb_allier;
    }
}

