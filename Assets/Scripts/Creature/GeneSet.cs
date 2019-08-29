using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class GeneSet : SerializableDictionaryBase<Genotype, float>
{

    public static GeneSet GetFirstGenerationGenes()
    {
        GeneSet fgg = new GeneSet();
        foreach (string genotype in System.Enum.GetNames(typeof(Genotype)))
        {
            Genotype enumGenotype;
            System.Enum.TryParse(genotype, out enumGenotype);
            fgg.Add(enumGenotype, Random.Range(0.4f, 0.7f));
        }
        fgg[Genotype.MutationChance] = Random.Range(0.01f, 0.1f);
        return fgg;
    }

    private static bool RandomBool { get { return Random.Range(0, 2) == 1; } }

    public GeneSet() { }

    public GeneSet(Creature mother, Creature father)
    {
        FromParentGenes(mother.genes, father.genes);
    }

    public GeneSet(GeneSet motherGenes, GeneSet fatherGenes)
    {
        FromParentGenes(motherGenes, fatherGenes);
    }
    
    public void FromParentGenes(GeneSet motherGenes, GeneSet fatherGenes)
    {
        Clear();
        foreach (string genotype in System.Enum.GetNames(typeof(Genotype)))
        {
            Genotype enumGenotype;
            System.Enum.TryParse(genotype, out enumGenotype);

            bool useFatherGenes = RandomBool;
            bool mutate = Random.Range(0, Mathf.Lerp(256, 1, (motherGenes[Genotype.MutationChance] + fatherGenes[Genotype.MutationChance]) / 2)) == 0;

            float geneValue = useFatherGenes ? fatherGenes[Genotype.MutationChance] : motherGenes[Genotype.MutationChance];

            if (mutate)
            {
                geneValue += RandomBool ? geneValue : -geneValue;
            }

            Add(enumGenotype, Mathf.Clamp(geneValue, 0f, 1f));
        }
    }
}

class GeneSetConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(GeneSet));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);

        GeneSet importedSet = new GeneSet(); // new Dictionary<Genotype, float>
        foreach (KeyValuePair<string, JToken> entry in jo)
        {
            Genotype genotype;
            Enum.TryParse(entry.Key, out genotype);
            importedSet.Add(genotype, entry.Value.ToObject<float>());
        }

        return importedSet;
    }

    public override bool CanWrite
    {
        get { return false; }
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}