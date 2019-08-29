using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

public class CreatureDeathDataPoint
{
    [JsonProperty]
    public string sn;
    [JsonProperty]
    public uint gen;
    [JsonProperty]
    public uint cno;
    [JsonProperty]
    public uint cct;
    [JsonProperty]
    public float lfs;
    [JsonProperty]
    public DeathCause cod;

    public CreatureDeathDataPoint(Creature creature, DeathCause causeOfDeath)
    {
        sn = creature.speciesName;
        gen = creature.Generation;
        cno = creature.childNo;
        cct = creature.children;
        lfs = Time.time - creature.birthTime;
        cod = causeOfDeath;
    }

    public CreatureDeathDataPoint(string _sn, uint _gen, uint _cno, uint _cct, float _lfs, DeathCause _cod)
    {
        sn = _sn;
        gen = _gen;
        cno = _cno;
        cct = _cct;
        lfs = _lfs;
        cod = _cod;
    }


}

class CreatureDeathDataPointConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(CreatureBirthDataPoint));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);

        return new CreatureDeathDataPoint(jo["sn"].ToObject<string>(), jo["gen"].ToObject<uint>(), jo["cno"].ToObject<uint>(), jo["cct"].ToObject<uint>(), jo["lfs"].ToObject<float>(), jo["cod"].ToObject<DeathCause>());
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
