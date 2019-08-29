using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class CreatureSight
{
    
    public Creature SourceCreature { get; private set; }
    public Creature[] Creatures { get; private set; }
    public Creature[] SameCreature
    {
        get
        {
            List<Creature> sameSpecies = new List<Creature>();
            foreach (Creature creature in Creatures)
            {
                if (creature.speciesName == SourceCreature.speciesName)
                {
                    sameSpecies.Add(creature);
                }
            }
            return sameSpecies.ToArray();
        }
    }
    public Creature[] Predators
    {
        get
        {
            List<Creature> predators = new List<Creature>();
            foreach (Creature creature in Creatures)
            {
                if (creature.dietType == DietType.Carnivore || creature.dietType == DietType.Omnivore)
                {
                    predators.Add(creature);
                }
            }
            return predators.ToArray();
        }
    }
    public FoodSource[] Food { get; private set; }
    public FoodSource[] EdibleFood {
        get
        {
            List<FoodSource> edibleFood = new List<FoodSource>();
            foreach (FoodSource foodSource in Food)
            {
                if (!foodSource.reserved)
                {
                    if (SourceCreature.dietType == DietType.Omnivore)
                    {
                        if (foodSource.type == FoodType.Foliage && ((FoliageFoodSource)foodSource).remainingNutrition > 1f)
                            edibleFood.Add(foodSource);
                        else
                            edibleFood.Add(foodSource);
                    }
                    else if (SourceCreature.dietType == DietType.Carnivore && foodSource.type == FoodType.Meat)
                    {
                        edibleFood.Add(foodSource);
                    }
                    else if (SourceCreature.dietType == DietType.Herbivore && foodSource.type == FoodType.Foliage)
                    {
                        if (((FoliageFoodSource)foodSource).remainingNutrition > 1f)
                            edibleFood.Add(foodSource);
                    }
                }
            }
            return edibleFood.ToArray();
        }
    }
    public WaterBlock[] Water { get; private set; }

    public CreatureSight(Creature seeingCreature, Collider[] sphereOfView)
    {
        SourceCreature = seeingCreature;

        // temporary lists
        List<Creature> creatures = new List<Creature>();
        List<FoodSource> food = new List<FoodSource>();
        List<WaterBlock> water = new List<WaterBlock>();

        foreach (Collider collider in sphereOfView)
        {
            if (collider == seeingCreature.GetComponent<Collider>()) continue;
            Creature creature = collider.GetComponent<Creature>();
            FoodSource foodSource = collider.GetComponent<FoodSource>();
            WaterBlock waterBlock = collider.GetComponent<WaterBlock>();

            if (creature != null)
            {
                if (creature == seeingCreature) continue;
                creatures.Add(creature);
            }
            if (foodSource != null)
            {
                if (foodSource.GetComponent<Creature>() != null && foodSource.GetComponent<Creature>().speciesName == SourceCreature.speciesName) continue;
                food.Add(foodSource);
                
            }
            if (waterBlock != null)
            {
                water.Add(waterBlock);
            }
        }


        Creatures = creatures.ToArray();
        Food = food.ToArray();
        Water = water.ToArray();
    }

    public Creature ClosestPredator { get { return Closest<Creature>(SourceCreature, Predators.ToList().Select(w => w.gameObject).ToArray()); } }

    public Creature ClosestSameCreature { get { return Closest<Creature>(SourceCreature, SameCreature.ToList().Select(w => w.gameObject).ToArray()); } }

    public FoodSource ClosestFood { get {
            switch(SourceCreature.dietType)
            {
                case DietType.Herbivore:
                    {
                        return Closest<FoliageFoodSource>(SourceCreature, EdibleFood.Select(f => f.gameObject).ToArray());
                    }
                case DietType.Carnivore:
                    {
                        FoodSource[] livingMeat = EdibleFood.Where(food => food.GetComponent<Creature>() != null).ToArray();
                        FoodSource[] rawMeat = EdibleFood.Where(food => food.GetComponent<Creature>() == null).ToArray();

                        if (rawMeat.Length > 0)
                            return Closest<MeatFoodSource>(SourceCreature, rawMeat.Select(f => f.gameObject).ToArray());
                        else
                            return Closest<MeatFoodSource>(SourceCreature, livingMeat.Select(f => f.gameObject).ToArray());
                    }
                default:
                    {
                        FoodSource[] livingMeat = EdibleFood.Where(food => food.GetComponent<Creature>() != null).ToArray();
                        FoodSource[] rawFood = EdibleFood.Where(food => food.GetComponent<Creature>() == null).ToArray();
                        FoodSource[] rawMeat = rawFood.Where(food => food.type == FoodType.Meat).ToArray();
                        FoodSource[] rawPlant = rawFood.Where(food => food.type == FoodType.Foliage).ToArray();

                        if (rawMeat.Length > 0)
                            return Closest<MeatFoodSource>(SourceCreature, rawMeat.Select(f => f.gameObject).ToArray());
                        else if (rawPlant.Length > 0)
                            return Closest<FoliageFoodSource>(SourceCreature, rawPlant.Select(f => f.gameObject).ToArray());
                        else
                            return Closest<MeatFoodSource>(SourceCreature, livingMeat.Select(f => f.gameObject).ToArray());
                    }
            }
        } }

    public WaterBlock ClosestWater { get { return Closest<WaterBlock>(SourceCreature, Water.ToList().Select(w => w.gameObject).ToArray()); } }

    internal static T Closest<T>(Creature reference, GameObject[] targets)
    {
        float[] result = targets.Where(target => target != null).Select(target => Vector3.Distance(reference.transform.position, target.transform.position)).ToArray();
        if (result.Length == 0) return default;
        float maxValue = result.Min();
        int maxIndex = result.ToList().IndexOf(maxValue);
        return targets[maxIndex].GetComponent<T>();
    }

    internal static GameObject Closest(Creature reference, GameObject[] targets)
    {
        float[] result = targets.Where(target => target != null).Select(target => Vector3.Distance(reference.transform.position, target.transform.position)).ToArray();
        if (result.Length == 0) return default;
        float maxValue = result.Min();
        int maxIndex = result.ToList().IndexOf(maxValue);
        return targets[maxIndex];
    }
}
