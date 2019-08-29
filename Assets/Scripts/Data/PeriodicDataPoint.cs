using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PeriodicDataPoint
{
    [JsonProperty]
    public float t;
    [JsonProperty]
    public float fps;
    [JsonProperty]
    public int tcc;
    [JsonProperty]
    public int lcc;
    [JsonProperty]
    public Dictionary<string, SpeciesStats> ss = new Dictionary<string, SpeciesStats>();

    public PeriodicDataPoint()
    {
        t = Time.time;
        fps = 1.0f / Time.deltaTime;
        tcc = CreatureDatabase.GetCreatureCount();
        lcc = CreatureDatabase.GetLiveCreatureCount();
        
        foreach (string species in CreatureDatabase.species.Keys)
        {
            ss.Add(species, new SpeciesStats(species));
        }
    }

    [JsonConstructor]
    public PeriodicDataPoint(float _t, float _fps, int _tcc, int _lcc, Dictionary<string, SpeciesStats> _ss)
    {
        t = _t;
        fps = _fps;
        tcc = _tcc;
        lcc = _lcc;
        ss = _ss;
    }
}

class PeriodicDataPointConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(PeriodicDataPoint));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);

        return new PeriodicDataPoint(jo["t"].ToObject<float>(), jo["fps"].ToObject<float>(), jo["tcc"].ToObject<int>(), jo["lcc"].ToObject<int>(), jo["ss"].ToObject<Dictionary<string, SpeciesStats>>());
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