using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

public class ParentStats
{
    [JsonProperty]
    public string id;
    [JsonProperty]
    public uint gen;
    [JsonProperty]
    public uint cno;
    [JsonProperty]
    public uint cct;

    public ParentStats(Creature parent)
    {
        if (parent != null)
        {
            id = parent.ID;
            gen = parent.Generation;
            cno = parent.childNo;
            cct = parent.children;
        }
        else
        {
            id = "";
            gen = 0;
            cno = 0;
            cct = 0;
        }
    }

    public ParentStats(string _id, uint _gen, uint _cno, uint _cct)
    {
        id = _id;
        gen = _gen;
        cno = _cno;
        cct = _cct;
    }
}

class ParentStatsConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(ParentStats));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);

        return new ParentStats(jo["id"].ToObject<string>(), jo["gen"].ToObject<uint>(), jo["cno"].ToObject<uint>(), jo["cct"].ToObject<uint>());
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