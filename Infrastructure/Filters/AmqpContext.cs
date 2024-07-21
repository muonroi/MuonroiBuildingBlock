﻿namespace MuonroiBuildingBlock.Infrastructure.Filters
{
    public class AmqpContext : IAmqpContext
    {
        private readonly Dictionary<string, object> _headers = [];

        public void ClearHeaders()
        {
            _headers.Clear();
        }

        public void AddHeaders(IDictionary<string, object> headers)
        {
            foreach (KeyValuePair<string, object> header in headers)
            {
                _headers.Add(header.Key, header.Value);
            }
        }

        public string? GetHeaderByKey(string headerKey)
        {
            return _headers.TryGetValue(headerKey, out object? value) ? value != null ? Encoding.Default.GetString((byte[])value) : null : null;
        }
    }
}