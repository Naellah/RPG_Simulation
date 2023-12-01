using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class GameManager : MonoBehaviour
{
    public List<PlayerCharacter> characters = new List<PlayerCharacter>();
    public bool test;
    public List<DeathInfo> infos_mort = new List<DeathInfo>();
    public List<string> liste_temp_mort = new List<string>();
    public List<GameObject> Coeurs = new List<GameObject>();
    public List<Group> groups = new List<Group>(); // Liste pour stocker les groupes.
    public int taille_max = 4;
    public GameObject effet;


    public void AddCoeur(GameObject coeur)
    {
        Coeurs.Add(coeur);
    }
    public void RemoveCoeur(GameObject coeur)
    {
        Coeurs.Remove(coeur);
    }

    // Cette fonction permet d'ajouter un personnage à la liste gérée par le GameManager.
    public void AddCharacter(PlayerCharacter character)
    {
        characters.Add(character);
    }
    public void RemoveCharacter(PlayerCharacter character)
    {
        characters.Remove(character);
    }

    // Cette fonction permet à un personnage d'envoyer un message à tous les autres personnages de son équipe dans un certain rayon.
    public void SendMessageToAllies(PlayerCharacter sender, string message, float radius, int ennemis/*, Vector3? position*/)
    {
        foreach (PlayerCharacter character in characters)
        {
            bool isAlly = character != sender && character.team == sender.team;
            float distance = Vector3.Distance(sender.transform.position, character.transform.position);

            if (isAlly && distance <= radius && !sender.charactersMessageSent.Contains(character))
            {
                if (character.currentState == CharacterState.Normal && character.is_leader != true)
                {
                    //Debug.Log("MESSAGE RECU :  " + message);
                    character.ReceiveMessage(message, sender, sender.nb_ennemis/*, null*/);
                    sender.charactersMessageSent.Add(character); // Ajoute le destinataire à la liste.
                }
            }

        }

    }

    private void Start()
    {
        foreach (PlayerCharacter character in characters)
        {
            character.OnDeath += HandleCharacterDeath;
            character.OnCharacterEncounter += HandleCharacterEncounter;
        }
        var provisoire = GameObject.FindGameObjectsWithTag("Coeur");
        foreach (GameObject coeur in provisoire)
        {
            AddCoeur(coeur);
        }
    }
    private void Update()
    {
        foreach (Group group in groups)
        {
            group.Calcul_puissance();
        }
        for (int i = 0; i < Coeurs.Count; i++)
        {
            if (Coeurs[i] == null)
            {
                RemoveCoeur(Coeurs[i]);
            }
        }
    }

    void HandleCharacterEncounter(PlayerCharacter j1, PlayerCharacter j2)
    {
        if (j1.currentGroup == null && j2.currentGroup == null)
        {

            if (string.Compare(j1.name, j2.name) < 0)
            {
                PlayerCharacter leader = (Random.Range(0, 2) == 0) ? j1 : j2;
                PlayerCharacter member = (leader == j1) ? j2 : j1;
                leader.CreateGroup();
                member.JoinGroup(leader.currentGroup);
                //DisplayGroupsInfo();
            }
        }
        else if (j1.currentGroup != null && j2.currentGroup == null)
        {
            if (j1.currentGroup.taille < taille_max)
            {
                j2.JoinGroup(j1.currentGroup);
                //Debug.Log("cas 1");
            }
        }
        else if (j1.currentGroup == null && j2.currentGroup != null)
        {
            if (j2.currentGroup.taille < taille_max)
            {
                j1.JoinGroup(j2.currentGroup);
                //Debug.Log("cas 2");
            }
        }
        else
        {
            if (j1.Should_change_group(j2.currentGroup) && !j1.is_leader)
            {
                j1.JoinGroup(j2.currentGroup);
                //Debug.Log("cas 3");
            }
            if (j2.Should_change_group(j1.currentGroup) && !j2.is_leader)
            {
                j2.JoinGroup(j1.currentGroup);
                //Debug.Log("cas 4");
            }
            if (j1.is_leader && j2.is_leader && j1.currentGroup.taille + j2.currentGroup.taille <= taille_max)
            {
                PlayerCharacter nouveau_leader = (Random.Range(0, 2) == 0) ? j1 : j2;
                PlayerCharacter ancien_leader = (nouveau_leader == j1) ? j2 : j1;

                foreach (PlayerCharacter membre in ancien_leader.currentGroup.members.AsEnumerable().Reverse())
                {
                    membre.JoinGroup(nouveau_leader.currentGroup);
                }

            }
        }
    }

    void HandleCharacterDeath(DeathInfo info)
    {
        infos_mort.Add(info);
        string deathMessage = "un joueur " + info.tueur.team + " a tué un joueur " + info.victime.team;
        AddDeathMessage(deathMessage, 5.0f); // Afficher le message pendant 5 secondes, ajustez cette valeur en fonction de vos préférences.
    }

    public void AddDeathMessage(string message, float displayTime)
    {
        liste_temp_mort.Add(message);
        StartCoroutine(RemoveDeathMessage(message, displayTime));

    }

    private IEnumerator RemoveDeathMessage(string message, float displayTime)
    {
        yield return new WaitForSeconds(displayTime);
        liste_temp_mort.Remove(message);
    }

    void OnGUI()
    {
        float messageX = Screen.width - 300; // Position X du message
        float messageY = 10; // Position Y du message

        foreach (string deathMessage in liste_temp_mort)
        {
            GUI.Label(new Rect(messageX, messageY, 2000, 20), deathMessage);
            messageY += 20; // Ajustez la valeur pour espacer les messages
        }
    }

    public void DisplayGroupsInfo()
    {
        //Debug.Log("Liste des groupes :");
        foreach (var group in groups)
        {
            string equipe = "Groupe avec le leader : " + group.GetLeader() + " et les membres : ";

            foreach (var member in group.members)
            {
                equipe += "- " + member;
            }

            //Debug.Log(equipe);
            //Debug.Log("Nombre de membres dans le groupe : " + group.members.Count);
            //Debug.Log(""); // Ligne vide pour séparer les groupes
        }
    }
    public void RemoveGrp(Group group)
    {
        groups.Remove(group);
    }


    void SetKnightInfo(string knightName)
    {
        // Analyse du nom du chevalier
        string[] nameParts = knightName.Split('_');

        if (nameParts.Length == 3)
        {
            // Récupération du type, de la couleur et du numéro
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



    public void setCamera()
    {
        bool existeCamera = false;
        List<Camera> cameras = new List<Camera>();

        // Recherche de caméras dans les PlayerCharacter
        foreach (PlayerCharacter character in characters)
        {
            Camera cameraDuChevalier = character.GetComponentInChildren<Camera>();
            if (cameraDuChevalier != null)
            {
                existeCamera = true;

            }
        }

        if (!existeCamera)
        {
            // Si aucune caméra n'a été trouvée mais au moins une existe, choisissez une caméra au hasard
            PlayerCharacter chara = characters[Random.Range(0, characters.Count)];
            //Debug.Log("Je suis " + chara.name + "ET JE SUIS LE CHOISI");
            SetKnightInfo(chara.name);

        }
    }



}


public class Group
{
    public PlayerCharacter leader; // Référence au leader du groupe
    public List<PlayerCharacter> members = new List<PlayerCharacter>(); // Liste des membres du groupe
    public int taille = 0;
    public float puissance = 0;
    //public int groupID;

    public Group(PlayerCharacter leader)
    {
        this.leader = leader;
        leader.is_leader = true;
        //Debug.Log("groupe créé avec comme leader : " + leader.name);
    }

    public void AddMember(PlayerCharacter member)
    {
        members.Add(member);
        taille++;
        //Debug.Log("Ajout du membre : " + member.name + " dans le groupe de " + member.currentGroup.leader.name + " et la taille du grp : " + taille);
    }

    public void RemoveMember(PlayerCharacter member)
    {
        members.Remove(member);
        member.is_leader = false;
        taille--;
    }

    public int GetMemberCount()
    {
        return members.Count;
    }

    /*public void RemoveGroup()
    {
        foreach (PlayerCharacter member in members)
        {
            member.currentGroup = null;
            members.Remove(member);
        }
        leader.is_leader = false;
        leader = null;
        taille=0;
        RemoveGrp(this);
    }*/

    public PlayerCharacter GetLeader()
    {
        return leader;
    }
    public void Calcul_puissance()
    {
        float puissance_avant = 0;
        foreach (PlayerCharacter member in members)
        {
            puissance_avant = puissance_avant + member.calcul_puissance();
        }
        puissance = puissance_avant;
        //Debug.Log(" La puissance du groupe de " + leader + " est de : " + puissance);
    }

    public void UpdateGroupState()
    {
        if (leader != null)
        {
            foreach (PlayerCharacter member in members)
            {
                member.currentState = leader.currentState;
            }
        }
    }

    // Autres fonctions relatives à la gestion du groupe


}






