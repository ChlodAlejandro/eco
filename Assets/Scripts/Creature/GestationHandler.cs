using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Creature))]
public class GestationHandler : MonoBehaviour
{

    private bool RandomBool { get { return Random.Range(0, 2) == 1; } }

    internal bool pregnant;
    internal Creature currentCreature;
    internal Creature father;
    internal Creature mother;

    internal float gestationPeriod;
    internal float gestationStart;
    internal uint totalChildren;
    internal uint childCount;

    private void Awake()
    {
        currentCreature = GetComponent<Creature>();
        Reactivate();
    }

    public void Reactivate()
    {
        if (currentCreature.gender == GenderType.Male)
        {
            enabled = false;
            return;
        }
        else
        {
            mother = currentCreature;
        }
    }

    /* <returns>if the reproduction has started successfully</returns> */
    internal bool Reproduce(Creature fatherCreature)
    {
        if (mother.activity == CreatureActivity.Mating) return false;
        father = fatherCreature;

        childCount = (uint) Mathf.FloorToInt(Mathf.Lerp(2, 9, RandomBool ? father.genes[Genotype.GestationSize] : mother.genes[Genotype.GestationSize]));
        gestationPeriod = Mathf.FloorToInt(Mathf.Lerp(Random.Range(16f, 30f), Random.Range(55f, 80f), mother.genes[Genotype.GestationPeriod]));
        gestationStart = Time.time;

        pregnant = true;
        StartCoroutine("Gestate");
        return true;
    }

    IEnumerator Gestate()
    {
        float miscarryChance = Random.Range(0.2f, 1f) * mother.genes[Genotype.Fertility];
        for (;;)
        {
            if (Time.time > gestationStart + gestationPeriod)
            {
                StartCoroutine("Birth");
                pregnant = false;
                yield break;
            }
            else if (0.01 > miscarryChance || CreatureDatabase.GetLiveCreatureCount() >= 2000)
            {
                Debug.Log(mother.ID + " miscarried (" + miscarryChance + "/" + CreatureDatabase.GetLiveCreatureCount() + ").");
                StopCoroutine("Gestate");
                pregnant = false;
                yield break;
            }
            yield return null;
        }
    }

    IEnumerator Birth()
    {
        for (uint childNo = 0; childNo <= childCount; childNo++)
        {
            totalChildren++;
            GameObject child = Instantiate(mother.gameObject);
            Creature childCreature = child.GetComponent<Creature>();
            childCreature.SetFamily(mother, father, totalChildren);
            
            yield return new WaitForSeconds(0.75f);
        }

        childCount = 0;
        gestationPeriod = 0f;
        gestationStart = 0f;
    }

}


#if UNITY_EDITOR
[CustomEditor(typeof(GestationHandler))]
public class GestationHandlerInspector : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GestationHandler gestationHandler = (GestationHandler)target;
        if (GUILayout.Button("Force Birth"))
        {
            if (Application.isPlaying)
            {
                gestationHandler.father = gestationHandler.mother;

                gestationHandler.childCount = (uint)Mathf.FloorToInt(Mathf.Lerp(2, 9, Random.Range(0, 2) == 1 ? gestationHandler.father.genes[Genotype.GestationSize] : gestationHandler.mother.genes[Genotype.GestationSize]));
                gestationHandler.gestationPeriod = 0f;
                gestationHandler.gestationStart = Time.time;

                gestationHandler.pregnant = true;
                gestationHandler.StartCoroutine("Gestate");
            }
        }
    }

}

#endif