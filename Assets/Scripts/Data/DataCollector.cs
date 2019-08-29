using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class DataCollector : MonoBehaviour
{
    public static DataCollector main { get { return GameObject.Find("DataCollector").GetComponent<DataCollector>(); } }

    private string dataPath;

    private string statsDatabasePath;
    private string birthDatabasePath;
    private string deathDatabasePath;
    
    private void Awake()
    {
        dataPath = Path.GetFullPath(Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, DateTime.Now.ToString("yyyyMMdd-HHmmss"))).FullName);

        statsDatabasePath = Path.Combine(dataPath, "stats.json");
        birthDatabasePath = Path.Combine(dataPath, "births.json");
        deathDatabasePath = Path.Combine(dataPath, "deaths.json");

        Debug.Log("Writing data to " + dataPath);
    }

    private void Start()
    {
        InvokeRepeating("StatsWrite", 0f, 1f);
    }

    internal JsonSerializerSettings GetCustomSerializerSettings()
    {

        JsonSerializerSettings settings = new JsonSerializerSettings();
        settings.Converters.Add(new SpeciesStatsConverter());
        settings.Converters.Add(new StatsDataPointConverter());
        settings.Converters.Add(new PeriodicDataPointConverter());
        settings.Converters.Add(new ParentStatsConverter());
        settings.Converters.Add(new GeneSetConverter());
        settings.Converters.Add(new CreatureBirthDataPointConverter());
        settings.Converters.Add(new CreatureDeathDataPointConverter());

        return settings;
    }

    internal void StatsWrite()
    {
        Append(statsDatabasePath, JsonConvert.SerializeObject(new PeriodicDataPoint()) + "\n");
    }

    internal void BirthWrite(Creature creature)
    {
        Append(birthDatabasePath, JsonConvert.SerializeObject(new CreatureBirthDataPoint(creature)) + "\n");
    }

    internal void DeathWrite(Creature creature, DeathCause cod)
    {
        Append(deathDatabasePath, JsonConvert.SerializeObject(new CreatureDeathDataPoint(creature, cod)) + "\n");
    }

    private void Append(string path, string data)
    {
        File.AppendAllText(path, data);
    }

    private void Write(string path, string data)
    {
        File.Delete(path);
        File.WriteAllText(path, data);
    }
}
