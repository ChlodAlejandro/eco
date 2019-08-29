using System.Collections.Generic;
using System.Linq;

public static class CreatureDatabase
{

    public static List<Creature> creatures { get; private set; } = new List<Creature>();
    public static Dictionary<string, List<Creature>> species { get; private set; } = new Dictionary<string, List<Creature>>();

    public static void RegisterCreature(Creature creature)
    {
        creatures.Add(creature);
        if (species.ContainsKey(creature.speciesName))
        {
            species[creature.speciesName].Add(creature);
        }
        else
        {
            List<Creature> newSpeciesList = new List<Creature>();
            newSpeciesList.Add(creature);
            species.Add(creature.speciesName, newSpeciesList);
        }
    }

    public static List<Creature> GetLive(Creature specieSample)
    {
        if (species.ContainsKey(specieSample.speciesName))
            return species[specieSample.speciesName].Where(c => c != null).ToArray().ToList();
        else
            return new List<Creature>();
    }

    public static List<Creature> GetLive(string speciesName) {
        if (species.ContainsKey(speciesName))
            return species[speciesName].Where(c => c != null).ToArray().ToList();
        else
            return new List<Creature>();
    }

    public static int GetCreatureCount()
    {
        return creatures.Count;
    }

    public static int GetLiveCreatureCount()
    {
        return creatures.Select(c => c != null).ToArray().Length;
    }

    public static int GetCreatureCount(string specieName) {
        if (species.ContainsKey(specieName))
            return species[specieName].Count;
        else
            return 0;
    }

    public static int GetLiveCreatureCount(string specieName)
    {
        if (species.ContainsKey(specieName))
            return species[specieName].Select(c => c != null).ToArray().Length;
        else
            return 0;
    }

    public static int GetCreatureCount(Creature specieSample)
    {
        if (species.ContainsKey(specieSample.speciesName))
            return species[specieSample.speciesName].Count;
        else
            return 0;
    }

    public static int GetLiveCreatureCount(Creature specieSample)
    {
        if (species.ContainsKey(specieSample.speciesName))
            return species[specieSample.speciesName].Select(c => c != null).ToArray().Length;
        else
            return 0;
    }
}
