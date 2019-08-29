using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

public class CreatureBirthDataPoint
{
    [JsonProperty]
    public string sn;
    [JsonProperty]
    public GeneSet gns;
    [JsonProperty]
    public uint gen;
    [JsonProperty]
    public uint cno;
    [JsonProperty]
    public ParentStats fth;
    [JsonProperty]
    public ParentStats mth;

    public CreatureBirthDataPoint(Creature creature)
    {
        sn = creature.name;
        gns = creature.genes;
        gen = creature.Generation;
        cno = creature.childNo;
        fth = new ParentStats(creature.father);
        mth = new ParentStats(creature.mother);
    }

    [JsonConstructor]
    public CreatureBirthDataPoint(string _sn, GeneSet _gns, uint _gen, uint _cno, ParentStats _fth, ParentStats _mth)
    {
        sn = _sn;
        gns = _gns;
        gen = _gen;
        cno = _cno;
        fth = _fth;
        mth = _mth;
    }
}

class CreatureBirthDataPointConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(CreatureBirthDataPoint));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);

        return new CreatureBirthDataPoint(jo["sn"].ToObject<string>(), jo["gns"].ToObject<GeneSet>(), jo["gen"].ToObject<uint>(), jo["cno"].ToObject<uint>(), jo["fth"].ToObject<ParentStats>(), jo["mth"].ToObject<ParentStats>());
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