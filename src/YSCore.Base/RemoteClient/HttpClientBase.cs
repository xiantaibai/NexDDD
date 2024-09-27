using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace YSCore.Base.RemoteClient
{
    public abstract class HttpClientBase
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        protected HttpClientBase(HttpClient httpClient,IOptions<HttpClientOptions> options)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            _httpClient.Timeout = options.Value.Timeout;

            _jsonSerializerOptions = options.Value.JsonSerializerSettings ?? new JsonSerializerOptions();
        }

        // 通用发送请求方法
        public async Task<HttpResponseMessage?> SendAsync<TRequest>(
            HttpMethod method,
            string url,
            TRequest? data = default,
            IDictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(method, url);

            // 如果有请求数据，则进行序列化
            if (data != null)
            {
                var jsonData = JsonSerializer.Serialize(data, _jsonSerializerOptions);
                request.Content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            }

            // 添加自定义请求头
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    if (!request.Headers.Contains(header.Key))
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }
            }

            // 发送请求并获取响应
            using var response = await _httpClient.SendAsync(request, cancellationToken);

            // 检查响应状态
            return response;
        }
    }
}
