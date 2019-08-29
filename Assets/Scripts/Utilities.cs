using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Utilities : MonoBehaviour
{
    public Creature creatureA;
    public Creature creatureB;

    public void GetCompatibility()
    {
        if (creatureA.PreviousMates.ContainsKey(creatureB))
        {
            Debug.Log("[Utilities] " + creatureB.ID + " remembers " + creatureA.ID + " from a past meeting.");
            return;
        }
        if (creatureA.gender == creatureB.gender)
        {
            Debug.Log("[Utilities] " + creatureA.ID + " are both " + creatureA.gender.ToString().ToLower() + ".");
            return;
        }
            if (creatureA.gender == GenderType.Female && creatureA.Pregnant)
        {
            Debug.Log("[Utilities] " + creatureA.ID + " is pregnant.");
            return;
        }
        if (creatureA.activity == CreatureActivity.Mating)
        {
            Debug.Log("[Utilities] " + creatureA.ID + " is currently mating.");
            return;
        }
        if (creatureB.gender == GenderType.Female && creatureB.Pregnant)
        {
            Debug.Log("[Utilities] " + creatureB.ID + " is pregnant.");
            return;
        }
        if (creatureB.activity == CreatureActivity.Mating)
        {
            Debug.Log("[Utilities] " + creatureB.ID + " is currently mating.");
            return;
        }
        if (GeneUtility.Relatives(creatureA, creatureB))
        {
            Debug.Log("[Utilities] " + creatureA.ID + " and " + creatureB.ID + " are relatives.");
            return;
        }
        if (Creature.CalculateMateCompatibility(creatureA, creatureB) || Creature.CalculateMateCompatibility(creatureB, creatureA))
        {
            if (creatureB.Nutrition > creatureB.maxNutrition * 0.25f && creatureB.Hydration > creatureB.maxHydration * 0.25f)
            {
                Debug.Log("[Utilities] Compatible.");
            } else
            {
                Debug.Log("[Utilities] " + creatureB.ID + " has a critical " + (creatureB.Nutrition > creatureB.maxNutrition * 0.25f ? "nutrition" : "hydration") + " level.");
            }
        }
    }

    public void Meetup()
    {
        creatureA.activity = CreatureActivity.Roaming;
        creatureB.activity = CreatureActivity.Roaming;
        StartCoroutine(GoToCreature());
    }

    public IEnumerator GoToCreature()
    {
        for (;;)
        {
            creatureA.navAgent.SetDestination(creatureB.transform.position);
            creatureB.navAgent.SetDestination(creatureA.transform.position);
            if (Physics.OverlapSphere(creatureA.transform.position, 0.5f).Contains(creatureB.GetComponent<Collider>()))
                yield break;
            else yield return null;
        }
    }

    public void GetRelativeLink()
    {
        Creature link = GeneUtility.GetLink(creatureA, creatureB);
        if (link == null)
        {
            Debug.Log("[Utilities] No link found.");
        }
        else
        {
            Debug.Log("[Utilities] " + creatureA.ID + " and " + creatureB.ID + " have " + link.ID + " in common.");
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Utilities))]
public class UtilitiesInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Utilities utilities = (Utilities)target;
        if (GUILayout.Button("Check Compatibility"))
        {
            utilities.GetCompatibility();
        }

        if (GUILayout.Button("Meetup"))
        {
            utilities.Meetup();
        }

        if (GUILayout.Button("Get Relative Link"))
        {
            utilities.GetRelativeLink();
        }
    }
}
#endif