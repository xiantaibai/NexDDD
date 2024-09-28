using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using YSCore.Base.Exceptions;

namespace YSCore.Base.RemoteClient
{
    public abstract class RemoteClientBase
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        protected RemoteClientBase(HttpClient httpClient, IOptions<RemoteClientOptions> options)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            _jsonSerializerOptions = options.Value.JsonSerializerSettings ?? new JsonSerializerOptions();
        }

        // 通用发送请求方法
        public async Task<HttpResponseMessage> SendAsync(
            HttpMethod method,
            string url,
            object? data = default,
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


        public async virtual Task<TResponse?> SendAsync<TResponse>(HttpMethod method, string url, object? data = null, IDictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
        {
            var response = await SendAsync(method, url, data, headers, cancellationToken);
            var responseData = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<TResponse>(responseData, _jsonSerializerOptions);
                return result;
            }
            else
            {
                throw ExceptionHelper.AppFriendly("请求失败:{0}", responseData);
            }
        }

        // GET 请求
        public Task<TResponse?> GetAsync<TResponse>(
            string url,
            IDictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default)
        {
            return SendAsync<TResponse>(HttpMethod.Get, url, null, headers, cancellationToken);
        }

        // POST 请求
        public Task<TResponse?> PostAsync<TResponse>(
            string url,
            object data,
            IDictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default)
        {
            return SendAsync<TResponse>(HttpMethod.Post, url, data, headers, cancellationToken);
        }

        // PUT 请求
        public Task<TResponse?> PutAsync<TResponse>(
            string url,
            object data,
            IDictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default)
        {
            return SendAsync<TResponse>(HttpMethod.Put, url, data, headers, cancellationToken);
        }

        // DELETE 请求
        public Task<TResponse?> DeleteAsync<TResponse>(
            string url,
            IDictionary<string, string>? headers = null,
            CancellationToken cancellationToken = default)
        {
            return SendAsync<TResponse>(HttpMethod.Delete, url, null, headers, cancellationToken);
        }
    }
}
