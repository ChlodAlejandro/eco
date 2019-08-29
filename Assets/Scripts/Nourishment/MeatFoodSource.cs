#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
#endif

public class MeatFoodSource : FoodSource
{
    [SerializeField]
    public Material meatMaterial;
    public float baseNutrition = 2.0f;
    public float remainingNutrition = 2.0f;
    public override FoodType type { get { return FoodType.Meat; } }

    public void Update()
    {
        if (remainingNutrition == 0)
        {
            Die();
        }
    }

    public void CreatureToMeat()
    {
        if (GetComponent<Creature>() == null) throw new System.Exception("You can only turn creatures into raw meat.");
        NavMeshAgent navAgent = GetComponent<NavMeshAgent>();
        navAgent.isStopped = true;;
        navAgent.speed = 0;
        navAgent.angularSpeed = 0;
        navAgent.acceleration = 0;
        remainingNutrition = baseNutrition;
        if (GetComponent<GestationHandler>() != null) Destroy(GetComponent<GestationHandler>());
        Creature creature = GetComponent<Creature>();
        name = "meat-" + creature.ID;
        Destroy(creature);
        GetComponent<Renderer>().material = meatMaterial;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MeatFoodSource))]
public class MeatFoodSourceInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MeatFoodSource mfs = (MeatFoodSource) target;
        if (GUILayout.Button("Creature To Meat"))
        {
            try
            {
                mfs.CreatureToMeat();
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}
#endif