using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class Groupe : MonoBehaviour
{
   
    public PlayerCharacter leader;
    public List<PlayerCharacter> members;

    public Groupe(PlayerCharacter leader)
    {
        this.leader = leader;
        members = new List<PlayerCharacter>();
    }

    public void addMember(PlayerCharacter member)
    {
        members.Add(member);
    }

    public void removeMember(PlayerCharacter member) { 
        members.Remove(member);
    }
}
