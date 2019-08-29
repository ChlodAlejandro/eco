using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpeciesStats
{
    [JsonProperty]
    public int tcc;
    [JsonProperty]
    public int lcc;
    [JsonProperty]
    public StatsDataPoint ntr;
    [JsonProperty]
    public StatsDataPoint hyd;
    [JsonProperty]
    public StatsDataPoint rpu;
    [JsonProperty]
    public StatsDataPoint hlt;
    
    public SpeciesStats(string species)
    {
        tcc = CreatureDatabase.GetCreatureCount(species);
        lcc = CreatureDatabase.GetLiveCreatureCount(species);
        List<Creature> live = CreatureDatabase.GetLive(species);
        ntr = new StatsDataPoint(live.Select(c => c.Nutrition).ToArray());
        hyd = new StatsDataPoint(live.Select(c => c.Hydration).ToArray());
        rpu = new StatsDataPoint(live.Select(c => c.ReproductiveUrge).ToArray());
        hlt = new StatsDataPoint(live.Select(c => c.Health).ToArray());
    }

    [JsonConstructor]
    public SpeciesStats(int _tcc, int _lcc, StatsDataPoint _ntr, StatsDataPoint _hyd, StatsDataPoint _rpu, StatsDataPoint _hlt)
    {
        tcc = _tcc;
        lcc = _tcc;
        ntr = _ntr;
        hyd = _hyd;
        rpu = _rpu;
        hlt = _hlt;
    }
}

class SpeciesStatsConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(Dictionary<string, SpeciesStats>));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);

        // jo is an Object of all specie names and their respective SpeciesStats

        Dictionary<string, SpeciesStats> result = new Dictionary<string, SpeciesStats>();

        foreach (KeyValuePair<string, JToken> specie in jo)
        {
            JToken ss = specie.Key;
            result.Add(specie.Key, new SpeciesStats(
                ss["tcc"].ToObject<int>(),
                ss["lcc"].ToObject<int>(),
                ss["ntr"].ToObject<StatsDataPoint>(),
                ss["hyd"].ToObject<StatsDataPoint>(),
                ss["rpu"].ToObject<StatsDataPoint>(),
                ss["hlt"].ToObject<StatsDataPoint>())
            );
        }

        return result;
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