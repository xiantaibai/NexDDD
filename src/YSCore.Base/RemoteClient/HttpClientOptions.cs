using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace YSCore.Base.RemoteClient
{
    public class HttpClientOptions
    {
        public JsonSerializerOptions? JsonSerializerSettings { get; set; } = new JsonSerializerOptions();

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    }
}
