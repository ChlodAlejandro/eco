using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeatFoodSource))]
[RequireComponent(typeof(NavMeshAgent))]
public class Creature : MonoBehaviour
{
    // Static methods and fields

    public static bool CalculateMateCompatibility(Creature observingMate, Creature possibleMate)
    {
        float attractabilityModifier = Mathf.Lerp(-0.2f, 0.2f, possibleMate.genes[Genotype.Attractability]);
        return observingMate.attractabilityThreshold + Random.Range(-0.1f + attractabilityModifier, 0.1f + attractabilityModifier) < possibleMate.genes[Genotype.Attractability];
    }

    private static readonly float quickSpeed = 0.02f;

    // components

    internal NavMeshAgent navAgent;

    // biology

    public string speciesName; // name of the creature species

    // family and growth

    [SerializeField]
    public GeneSet genes { get; private set; } = new GeneSet();
    internal float birthTime { get; private set; }
    private Vector3 originalScale;

    internal float childGrowth
    {
        get
        {
            return Mathf.Clamp(Mathf.Abs((Time.time - birthTime) / 60), 0, 1);
        }
    }

    internal Creature father { get; private set; }
    internal Creature mother { get; private set; }

    public uint Generation { get; private set; } = 1;
    internal uint specieNo { get; private set; } = 0;
    internal uint childNo { get; private set; } = 0;

    internal string ID
    {
        get
        {
            return speciesName.Replace(" ", "").ToLower() + "-" + gender.ToString().Substring(0, 1).ToUpper()
                + "G" + Generation.ToString("D3") + "C" + childNo.ToString("D1") + "-S" + specieNo.ToString("D4");
        }
    }

    // ability from age
    public GeneSet ability
    {
        get
        {
            GeneSet calculatedAbility = new GeneSet();
            foreach (Genotype gene in genes.Keys)
            {
                calculatedAbility[gene] = Mathf.Lerp(0.01f, genes[gene], childGrowth);
            }
            return calculatedAbility;
        }
    }

    // survival attributes

    public float maxHealth = 10f;
    public float originalMaxNutrition = 5f;
    public float originalMaxHydration = 5f;

    internal float maxNutrition = 5f;
    internal float maxHydration = 5f;

    internal float Health { get; private set; } = 10f;
    internal float Nutrition { get; private set; } = 5f;
    internal float Hydration { get; private set; } = 5f;

    public DietType dietType;
    private Coroutine chaseCoroutine;
    private Stopwatch chaseTimer = new Stopwatch();

    public float baseSpeed = 3.5f;

    // memory
    
    public Dictionary<FoodSource, float> PreviousFood { get; private set; } = new Dictionary<FoodSource, float>();
    internal Dictionary<Creature, float> PreviousMates { get; private set; } = new Dictionary<Creature, float>(); // creature, last meet time
    internal Vector3 SightPosition;

    // woohoo

    public GenderType gender;
    internal float ReproductiveUrge { get; private set; }  = 0f;
    private float lastReproduction = 0f;
    private float lastReproductionTry;
    private float attractabilityThreshold;
    internal bool Pregnant { get { return gestation.pregnant; } }
    private Stopwatch matingTimer = new Stopwatch();
    private GestationHandler gestation;
    private Creature partner;
    internal uint children { get; private set; }

    // vision

    private CreatureSight Sight;

    // activity

    public CreatureActivity activity { get; internal set; }
    private float lastRoam;

    // lifecycle

    private IEnumerator UpdateSight()
    {
        if (SightPosition != transform.position)
        {
            Sight = new CreatureSight(this, Physics.OverlapSphere(
                transform.position,
                Mathf.Lerp(3, 12, ability[Genotype.Sight])
            ));

            SightPosition = transform.position;
        }
        yield return new WaitForSeconds(Time.deltaTime);
    }

    private void Awake()
    {
        transform.parent = GameObject.Find("Creatures").transform;
        originalScale = transform.localScale;
        navAgent = GetComponent<NavMeshAgent>();
        gestation = GetComponent<GestationHandler>();
        birthTime = Time.time;

        if (Generation == 1)
        {
            genes = GeneSet.GetFirstGenerationGenes();
            birthTime = Time.time - 60f;

            maxNutrition = Mathf.Clamp(Mathf.Lerp(originalMaxNutrition - 2, originalMaxNutrition + 2, ability[Genotype.Nutrition]), 1, Mathf.Infinity);
            maxHydration = Mathf.Clamp(Mathf.Lerp(originalMaxHydration - 2, originalMaxHydration + 2, ability[Genotype.Nutrition]), 1, Mathf.Infinity);

            Nutrition = Random.Range(1f, maxNutrition);
            Hydration = Random.Range(1f, maxHydration);
            ReproductiveUrge = Random.Range(0.1f, 0.35f);
        }
        else
        {
            gender = (Random.Range(0, 2) == 1 ? GenderType.Male : GenderType.Female);
        }

        if (gestation != null && gender == GenderType.Male) gestation.enabled = false;
        ReproductiveUrge = 0f;

        CreatureDatabase.RegisterCreature(this);
        specieNo = (uint)CreatureDatabase.GetCreatureCount(this);

        name = ID;

        StartCoroutine(UpdateSight());
    }

    internal void SetFamily(Creature motherCreature, Creature fatherCreature, uint no)
    {
        mother = motherCreature;
        father = fatherCreature;
        Generation = (uint)Mathf.FloorToInt(Mathf.Max(motherCreature.Generation, fatherCreature.Generation)) + 1;
        childNo = no;
        birthTime = Time.time;
        genes = new GeneSet(motherCreature, fatherCreature);

        ReproductiveUrge = 0f;

        gender = (Random.Range(0, 2) == 1 ? GenderType.Male : GenderType.Female);
        if (gestation != null && gender == GenderType.Male) gestation.enabled = false;
        gestation.Reactivate();
        birthTime = Time.time;

        name = ID;

        maxNutrition = Mathf.Clamp(Mathf.Lerp(originalMaxNutrition - 2, originalMaxNutrition + 2, ability[Genotype.Nutrition]), 1, Mathf.Infinity);
        maxHydration = Mathf.Clamp(Mathf.Lerp(originalMaxHydration - 2, originalMaxHydration + 2, ability[Genotype.Nutrition]), 1, Mathf.Infinity);
        Nutrition = maxNutrition;
        Hydration = maxHydration;
    }

    private void Start()
    {
        DataCollector.main.BirthWrite(this);
        Nutrition = maxNutrition;
        Hydration = maxHydration;

        attractabilityThreshold = Mathf.Lerp(0.1f, 0.8f, genes[Genotype.Attractability]);
        activity = CreatureActivity.Idle;

        StartCoroutine(Live());
        StartCoroutine(Idle());
    }

    private void Update()
    {
        if (Health < 0 && Nutrition == 0) Die(DeathCause.Nutrition);
        else if (Health < 0 && Hydration == 0) Die(DeathCause.Hydration);
        else if (Health < 0) Die(DeathCause.Injury);

        float remainingDistance = navAgent.remainingDistance;
        if ((remainingDistance != Mathf.Infinity && navAgent.pathStatus == NavMeshPathStatus.PathComplete && navAgent.remainingDistance == 0) && activity == CreatureActivity.Roaming)
        {
            lastRoam = Time.time;
            activity = CreatureActivity.Idle;
        }
    }

    IEnumerator Live()
    {
        for (; ; )
        {
            maxNutrition = Mathf.Clamp(Mathf.Lerp(originalMaxNutrition - 2, originalMaxNutrition + 2, ability[Genotype.Nutrition]), 1, Mathf.Infinity);
            maxHydration = Mathf.Clamp(Mathf.Lerp(originalMaxHydration - 2, originalMaxHydration + 2, ability[Genotype.Nutrition]), 1, Mathf.Infinity);

            if (childGrowth <= 1)
            {
                transform.localScale = Vector3.Lerp(new Vector3(
                        originalScale.x / 5,
                        originalScale.y / 5,
                        originalScale.z / 5
                    ), originalScale, childGrowth);
            }
            if (Time.time > birthTime + 720f)
            {
                Die(DeathCause.Age);
            }

            navAgent.speed = ((baseSpeed * Mathf.Lerp(0.8f, 1.2f, ability[Genotype.Speed])) * Mathf.Lerp(0.1f, 1f, childGrowth)) * Mathf.Lerp(0f, 1f, Health / maxHealth);
            navAgent.acceleration = baseSpeed * 2.25f;
            navAgent.angularSpeed = Mathf.Lerp(60f, 180f, ability[Genotype.Speed]);

            try
            {
                Dictionary<Creature, float> newPreviousMates = PreviousMates;
                foreach (KeyValuePair<Creature, float> entry in PreviousMates)
                {
                    if (Time.time > entry.Value + Mathf.Lerp(20f, 360f, ability[Genotype.Memory]))
                    {
                        newPreviousMates.Remove(entry.Key);
                    }
                }
                PreviousMates = newPreviousMates;
            }
#pragma warning disable CS0168 // Variable is declared but never used
            catch (System.Exception e) { }

            try
            {
                Dictionary<FoodSource, float> newPreviousFood = PreviousFood;
                foreach (KeyValuePair<FoodSource, float> entry in PreviousFood)
                {
                    if (entry.Key == null || Time.time > entry.Value + Mathf.Lerp(60f, 400f, ability[Genotype.Memory]) || Vector3.Distance(transform.position, entry.Key.transform.position) > 64f)
                    {
                        newPreviousFood.Remove(entry.Key);
                    }
                }
                PreviousFood = newPreviousFood;
            }
            catch (System.Exception e) { }
#pragma warning restore CS0168 // Variable is declared but never used

            if (Time.time > lastReproduction + 25f)
            {
                ReproductiveUrge += quickSpeed * 0.05f;
            }
            if (ReproductiveUrge > 1f) ReproductiveUrge = 1f;

            if (Nutrition <= 0)
            {
                if (activity != CreatureActivity.Eating) Nutrition = 0;
                Health -= quickSpeed * 0.5f;
            }
            else
            {
                Nutrition -= quickSpeed * (0.1f * (Mathf.Lerp(0.3f, 2f, ability[Genotype.Metabolism])));
            }

            if (Hydration <= 0)
            {
                if (activity != CreatureActivity.Drinking) Hydration = 0;
                Health -= quickSpeed * 0.5f;
            }
            else
            {
                Hydration -= quickSpeed * (0.15f * (Mathf.Lerp(0.5f, 2f, ability[Genotype.Metabolism])));
            }

            if (Hydration > maxHydration * 0.7 && Nutrition > maxNutrition * 0.7 && Health < maxHealth)
            {
                if ((quickSpeed * 0.5f) * Mathf.Lerp(0.01f, 1f, Mathf.Min(Hydration + Nutrition)) > maxHealth)
                    Health = maxHealth;
                else
                    Health += (quickSpeed * 0.5f) * Mathf.Lerp(0.01f, 1f, Mathf.Min(Hydration + Nutrition));
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator Idle()
    {
        for (; ; )
        {
            if (transform.up.y < 0.99)
            {
                activity = CreatureActivity.Rotating;
                Quaternion q = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, q, quickSpeed * Mathf.Lerp(1f, 6f, ability[Genotype.Speed]));
                yield return null;
            }
            else if (activity == CreatureActivity.Rotating)
                activity = CreatureActivity.Idle;

            if (partner != null && activity == CreatureActivity.Mating)
            {
                navAgent.SetDestination(partner.transform.position);
            }

            if (activity == CreatureActivity.Idle)
            {
                if (Hydration < maxHydration / 2)
                {
                    Hydrate();
                }
                else if (Nutrition < maxNutrition / 2)
                {
                    Eat();
                }
                else if (childGrowth <= 0.9f && mother != null && !Physics.OverlapSphere(transform.position, 4f).Contains(mother.GetComponent<Collider>()))
                {
                    activity = CreatureActivity.Navigating;
                    StartCoroutine(FollowCreature(mother));
                }
                else if (ReproductiveUrge > 0.6 && Time.time > lastReproductionTry + Mathf.Lerp(45f, 1f, ReproductiveUrge) && childGrowth >= 1)
                {
                    FindMate();
                }
                else if (Time.time > lastRoam + Random.Range(1f, 1.3f))
                {
                    Roam(2, Mathf.FloorToInt(Mathf.Lerp(4, 12, ability[Genotype.Sight])));
                }
            }
            yield return null;
        }
    }

    IEnumerator FollowCreature(Creature parent)
    {
        if (mother == null) yield break;
        while (!Physics.OverlapSphere(transform.position, 3f).Contains(mother.GetComponent<Collider>()))
        {
            navAgent.SetDestination(mother.transform.position);
            yield return new WaitForSeconds(0.1f);
        }
        activity = CreatureActivity.Idle;
        yield break;
    }

    public void Roam(int minDistance, int maxDistance)
    {
        Vector3 roamVector;
        do
        {
            bool negativeX = (Random.Range(0, 2) == 1);
            bool negativeZ = (Random.Range(0, 2) == 1);

            float x = Random.Range(minDistance, maxDistance);
            float z = Random.Range(minDistance, maxDistance);

            roamVector = transform.position + new Vector3(
                negativeX ? -x : x,
                0,
                negativeZ ? -z : z
            );
        } while (Physics.OverlapCapsule(roamVector, roamVector - new Vector3(0, -1, 0), 0.1f).Where(collider => collider.GetComponent<WaterBlock>() != null).Count() > 0);

        activity = CreatureActivity.Roaming;
        navAgent.SetDestination(roamVector);
    }

    public void Eat()
    {
        if (dietType == DietType.Carnivore) CreatureDebug("EF: " + Sight.EdibleFood.Length + ", CFN: " + (Sight.ClosestFood == null));
        if ((Sight.EdibleFood.Length == 0 || Sight.ClosestFood == null) &&
            (dietType == DietType.Carnivore ? (PreviousFood.Count == 0 || CreatureSight.Closest<GameObject>(this, PreviousFood.Keys.Select(f => f.gameObject).ToArray()) == null) : true))
        {
            if (dietType == DietType.Carnivore) CreatureDebug("can't find food (" + PreviousFood.Count + ")");
            Roam(4, 8);
        }
        else
        {
            if (dietType == DietType.Carnivore) CreatureDebug("finding food");
            activity = CreatureActivity.Eating;
            if (dietType == DietType.Carnivore && PreviousFood.Count > 0)
            {
                if (dietType == DietType.Carnivore) CreatureDebug("looking for past food (" + PreviousFood.Count + ")");
                FoodSource closestMeat = CreatureSight.Closest<FoodSource>(this, PreviousFood.Keys.Select(f => f.gameObject).ToArray());
                navAgent.SetDestination(closestMeat.transform.position);
                closestMeat.Reserve();
                StartCoroutine(EatMeat((MeatFoodSource)closestMeat));
            }
            else
            {
                if (dietType == DietType.Carnivore) CreatureDebug("looking for food");
                navAgent.SetDestination(Sight.ClosestFood.transform.position);
                Sight.ClosestFood.Reserve();
                if (Sight.ClosestFood == null) return; // in the hypothetical situation that the plant dies before the creature is able to move.
                if (Sight.ClosestFood.type == FoodType.Foliage)
                    StartCoroutine(EatPlant((FoliageFoodSource)Sight.ClosestFood));
                else if (Sight.ClosestFood.type == FoodType.Meat)
                {
                    StartCoroutine(EatMeat((MeatFoodSource)Sight.ClosestFood));
                }
            }
        }
    }

    IEnumerator EatPlant(FoliageFoodSource plant)
    {
        while (!Physics.OverlapSphere(transform.position, 1f).Contains(plant.GetComponent<Collider>()))
        {
            navAgent.SetDestination(plant.transform.position);
            yield return new WaitForSeconds(1f);
        }
        while (Nutrition < maxNutrition && plant.remainingNutrition != 0)
        {
            if (plant.remainingNutrition < 0.01f)
            {
                Nutrition += plant.remainingNutrition;
                plant.remainingNutrition = 0;
            }
            else
            {
                Nutrition += 0.01f;
                plant.remainingNutrition -= 0.01f;
                yield return null;
            }
        }
        plant.Release();
        activity = CreatureActivity.Idle;
        yield break;
    }

    IEnumerator EatMeat(MeatFoodSource meat)
    {
        if (dietType == DietType.Carnivore) CreatureDebug("starting meat");
        if (meat == null) yield break;
        chaseCoroutine = StartCoroutine(GoToObject(meat.gameObject));
        chaseTimer.Reset();
        chaseTimer.Start();
        while (!Physics.OverlapSphere(transform.position, 1.5f).Contains(meat.GetComponent<Collider>()))
        {
            if (chaseTimer.ElapsedMilliseconds > 15000f)
            {
                chaseTimer.Stop();
                chaseTimer.Reset();
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
        }
        if (meat.GetComponent<Creature>() != null)
        {
            Creature meatCreature = meat.GetComponent<Creature>();
            if (dietType == DietType.Carnivore) CreatureDebug("attacking " + meatCreature.ID + " (" + meatCreature.Health + ")");
            while (meatCreature.Health > Mathf.Lerp(0.2f, 2f, ability[Genotype.Strength]) * Mathf.Lerp(1.5f, 0.5f, meatCreature.ability[Genotype.Resistance]))
            {
                meatCreature.Health -= Mathf.Lerp(0.2f, 2f, ability[Genotype.Strength]) * Mathf.Lerp(1.5f, 0.5f, meatCreature.ability[Genotype.Resistance]);
                yield return new WaitForSeconds(Mathf.Lerp(1.2f, 0.1f, ability[Genotype.Speed]));
            }
            meat.GetComponent<Creature>().Die(DeathCause.Injury);
            if (dietType == DietType.Carnivore) CreatureDebug("killed. meat time.");
        }
        while (Nutrition < maxNutrition && meat.remainingNutrition != 0)
        {
            if (meat.remainingNutrition < 0.05f)
            {
                Nutrition += meat.remainingNutrition;
                meat.remainingNutrition = 0;
            }
            else
            {
                Nutrition += 0.05f;
                meat.remainingNutrition -= 0.05f;
                yield return null;
            }
        }
        if (meat != null && meat.remainingNutrition != 0) PreviousFood.Add(meat, Time.time);
        activity = CreatureActivity.Idle;
        yield break;
    }

    IEnumerator GoToObject(GameObject go)
    {
        while (activity == CreatureActivity.Eating)
        {
            navAgent.SetDestination(go.transform.position);
            yield return new WaitForSeconds(0.1f);
        }
        yield break;
    }

    public void Hydrate()
    {
        if (Sight.Water.Length == 0)
        {
            Roam(4, 8);
        }
        else
        {
            activity = CreatureActivity.Drinking;
            navAgent.SetDestination(Sight.ClosestWater.transform.position);
            StartCoroutine(Drink(Sight.ClosestWater));
        }
    }

    IEnumerator Drink(WaterBlock water)
    {
        while (!Physics.OverlapSphere(transform.position, 1f).Contains(water.GetComponent<Collider>()))
        {
            navAgent.SetDestination(water.transform.position);
            yield return new WaitForSeconds(1f);
        }
        while (Hydration < maxHydration)
        {
            Hydration += 0.03f;
            yield return null;
        }
        activity = CreatureActivity.Idle;
        yield break;
    }

    public void FindMate()
    {
        Creature[] possibleMates = Sight.SameCreature;
        lastReproductionTry = Time.time;
        if (possibleMates.Length > 0)
        {
            foreach (Creature possibleMate in possibleMates)
            {
                if (PreviousMates.ContainsKey(possibleMate)) continue;
                if (gender == possibleMate.gender) continue;
                if (gender == GenderType.Female && gestation.pregnant) continue;
                if (activity == CreatureActivity.Mating) continue;
                if (possibleMate.gender == GenderType.Female && possibleMate.gestation.pregnant) continue;
                if (possibleMate.activity == CreatureActivity.Mating) continue;
                if (GeneUtility.Relatives(this, possibleMate)) continue;
                if (CalculateMateCompatibility(this, possibleMate) || CalculateMateCompatibility(possibleMate, this))
                {
                    if (possibleMate.Nutrition > possibleMate.maxNutrition * 0.25f && possibleMate.Hydration > possibleMate.maxHydration * 0.25f)
                    {
                        if (gender == GenderType.Male) StartMate(possibleMate);
                        else possibleMate.StartMate(this);
                    }
                }
                else
                {
                    possibleMate.PreviousMates.Add(this, Time.time);
                    PreviousMates.Add(possibleMate, Time.time);
                }
            }
        }
    }

    public void StartMate(Creature partnerCreature)
    {
        if (partnerCreature == null) return;
        partner = partnerCreature;
        partnerCreature.partner = this;
        activity = CreatureActivity.Mating;
        partner.activity = CreatureActivity.Mating;
        navAgent.isStopped = true; ;
        StartCoroutine(Mate());
    }

    public IEnumerator Mate()
    {
        for (;;)
        {
            if (partner == null) yield break;
            if (Physics.OverlapSphere(transform.position, 1.5f).Contains(partner.GetComponent<Collider>()))
            {
                if (matingTimer.IsRunning == false)
                {
                    matingTimer.Reset();
                    matingTimer.Start();
                }
                else if (matingTimer.ElapsedMilliseconds > 3 * 1000)
                {
                    activity = CreatureActivity.Idle;
                    partner.activity = CreatureActivity.Idle;

                    if (partner.gender == GenderType.Female)
                    {
                        partner.gestation.Reproduce(this);
                        partner.children += partner.gestation.childCount;
                        children += partner.gestation.childCount;
                    }

                    matingTimer.Reset();
                    matingTimer.Stop();

                    partner.ReproductiveUrge = 0f;
                    partner.lastReproductionTry = Time.time;
                    partner.lastReproduction = Time.time;

                    ReproductiveUrge = 0f;
                    lastReproductionTry = Time.time;
                    lastReproduction = Time.time;
                    yield break;
                }
            }
            else
            {
                matingTimer.Stop();
                matingTimer.Reset();
            }
            yield return null;
        }
    }

    internal void Halt()
    {
        StopCoroutine("EatPlant");
        StopCoroutine("EatMeat");
        StopCoroutine("Drink");
        StopCoroutine("Mate");
        StopCoroutine("GoToObject");
        StopCoroutine("FollowCreature");
    }

    internal void CreatureDebug(string message)
    {
        Debug.Log("[" + ID + "] " + message);
    }

    internal void Die(DeathCause causeOfDeath)
    {
        Debug.Log(ID + " has died. Cause: " + causeOfDeath.ToString());
        DataCollector.main.DeathWrite(this, causeOfDeath);
        if (GetComponent<MeatFoodSource>() != null) GetComponent<MeatFoodSource>().CreatureToMeat();
        else DieNow();
    }

    internal void DieNow()
    {
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(navAgent.destination, 0.2f);
        }
    }
#endif

}