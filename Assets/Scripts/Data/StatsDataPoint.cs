using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using UnityEngine;

public class StatsDataPoint
{
    [JsonProperty]
    public float m; // min
    [JsonProperty]
    public float M; // max
    [JsonProperty]
    public float A; // avg
    
    public StatsDataPoint(float[] values)
    {
        m = M = A = 0;
        if (values == null)
            return;

        m = Mathf.Min(values);
        M = Mathf.Max(values);

        float At = 0f;
        foreach (float f in values)
        {
            At += f;
        }

        A = At / values.Length;
    }

    [JsonConstructor]
    public StatsDataPoint(float _m, float _M, float _A)
    {
        m = _m;
        M = _M;
        A = _A;
    }
}

class StatsDataPointConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(StatsDataPoint));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);

        return new StatsDataPoint(jo["m"].ToObject<float>(), jo["M"].ToObject<float>(), jo["A"].ToObject<float>());
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