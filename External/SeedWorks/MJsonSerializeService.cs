﻿
namespace Muonroi.BuildingBlock.External.SeedWorks
{
    internal class MJsonSerializeService : IMJsonSerializeService
    {
        public string Serialize<T>(T obj)
        {
            return JsonSerializer.Serialize(obj);
        }

        public T? Deserialize<T>(string text)
        {
            return JsonSerializer.Deserialize<T>(text);
        }
    }
}