using Microsoft.Extensions.Logging;
using SoraBot.Dto.Pixiv;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SoraBot.PixivApi
{
    public class PixivClient
    {
        private static readonly string USER_AGENT = "PixivIOSApp/7.13.4 (iOS 14.5; iPhone11,2)";
        private static readonly string REDIRECT_URI = "https://app-api.pixiv.net/web/v1/users/auth/pixiv/callback";
        private static readonly string LOGIN_URL = "https://app-api.pixiv.net/web/v1/login";
        private static readonly string API_URL = "https://app-api.pixiv.net/v1";
        private static readonly string AUTH_TOKEN_URL = "https://oauth.secure.pixiv.net/auth/token";
        private static readonly string CLIENT_NAME = "pixiv-ios";
        private static readonly string CLIENT_ID = "KzEZED7aC0vird8jWyHM38mXjNTY";
        private static readonly string CLIENT_SECRET = "W9JZoJe00qPvJsiyCGT3CCtC6ZUtdpKpzMbNlUGP";
        private static readonly string HASH_SECRET = "28c1fdd170a5204386cb1313c7077b34f83e4aaf4aa829ce78c231e05b0bae2c";

        private readonly ILogger<PixivClient> _logger;
        private readonly HttpClient _httpClient;
        private AuthToken? _token;

        public PixivClient(ILogger<PixivClient> logger)
        {
            _logger = logger;

            var proxy = new WebProxy("socks5://127.0.0.1:10808");
            var clientHandler = new HttpClientHandler { Proxy = proxy };
            _httpClient = new HttpClient(clientHandler);

            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(USER_AGENT);
            _httpClient.DefaultRequestHeaders.Add("App-OS", "ios");
            _httpClient.DefaultRequestHeaders.Add("App-OS-Version", "14.5");
            _httpClient.DefaultRequestHeaders.Add("App-Version", "7.13.4");
        }

        private static string CreateCodeVerifier()
        {
            var characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~";
            var random = new Random();
            var builder = new StringBuilder(32);

            for (var i = 0; i < 32; i++)
            {
                builder.Append(characters[random.Next(0, 65)]);
            }

            return builder.ToString();
        }

        private static string CreateCodeChallenge(string codeVerifier)
        {
            var digest = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
            return Convert.ToBase64String(digest).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static (string, string) CreateClientTimeHash()
        {
            var time = DateTime.Now.ToString("yyyy-MM-ddThh:mm:sszzz");
            var data = time + HASH_SECRET;

            var digest = MD5.HashData(Encoding.ASCII.GetBytes(data));
            var hash = Convert.ToHexString(digest).ToLower();

            return (time, hash);
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task LoginAsync()
        {
            var verifier = CreateCodeVerifier();
            var challenge = CreateCodeChallenge(verifier);

            var url = $"{LOGIN_URL}?code_challenge={challenge}&code_challenge_method=S256&client={CLIENT_NAME}";

            Console.WriteLine($"Login Url: {url}");
            Console.Write("Code: ");
            var code = Console.ReadLine();

            if (string.IsNullOrEmpty(code))
                return;

            var data = new MultipartFormDataContent
            {
                { new StringContent(CLIENT_ID), "client_id" },
                { new StringContent(CLIENT_SECRET), "client_secret" },
                { new StringContent(code), "code" },
                { new StringContent(verifier), "code_verifier" },
                { new StringContent("authorization_code"), "grant_type" },
                { new StringContent("true"), "include_policy" },
                { new StringContent(REDIRECT_URI), "redirect_uri" },
            };

            var timeHash = CreateClientTimeHash();

            data.Headers.Add("X-Client-Time", timeHash.Item1);
            data.Headers.Add("X-Client-Hash", timeHash.Item2);

            var response = await _httpClient.PostAsync(AUTH_TOKEN_URL, data);
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AuthResult>(json);

            if (result == null)
            {
                _logger.LogError("Failed to deserialize response.");
                return;
            }

            if (string.IsNullOrEmpty(result.AccessToken))
            {
                _logger.LogError("Failed to login.");
                return;
            }

            _token = new AuthToken
            {
                AccessToken = result.AccessToken,
                ExpiresIn = result.ExpiresIn,
                TokenType = result.TokenType,
                RefreshToken = result.RefreshToken,
                Timestamp = DateTime.Now,
            };
        }

        /// <summary>
        /// 刷新Token
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task RefreshTokenAsync()
        {
            if (_token == null)
                return;

            if (string.IsNullOrEmpty(_token.RefreshToken))
                return;

            // 清除旧Token
            _token.AccessToken = string.Empty;

            var data = new MultipartFormDataContent
            {
                { new StringContent(CLIENT_ID), "client_id" },
                { new StringContent(CLIENT_SECRET), "client_secret" },
                { new StringContent("refresh_token"), "grant_type" },
                { new StringContent("true"), "include_policy" },
                { new StringContent(_token.RefreshToken), "refresh_token" },
            };

            var timeHash = CreateClientTimeHash();

            data.Headers.Add("X-Client-Time", timeHash.Item1);
            data.Headers.Add("X-Client-Hash", timeHash.Item2);

            var response = await _httpClient.PostAsync(AUTH_TOKEN_URL, data);
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AuthResult>(json);

            if (result == null)
            {
                _logger.LogError("Failed to deserialize response.");
                return;
            }

            if (string.IsNullOrEmpty(result.AccessToken))
            {
                _logger.LogError("Failed to refresh token.");
                return;
            }

            _token = new AuthToken
            {
                AccessToken = result.AccessToken,
                ExpiresIn = result.ExpiresIn,
                TokenType = result.TokenType,
                RefreshToken = result.RefreshToken,
                Timestamp = DateTime.Now,
            };
        }

        /// <summary>
        /// 将Token保存到文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task SaveTokenAsync(string path)
        {
            if (_token == null)
                return;

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_token, options);

            await File.WriteAllTextAsync(path, json);
        }

        /// <summary>
        /// 从文件加载Token
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task LoadTokenAsync(string path)
        {
            var json = await File.ReadAllTextAsync(path);
            _token = JsonSerializer.Deserialize<AuthToken>(json);
        }

        /// <summary>
        /// 清除Token
        /// </summary>
        public void ClearToken()
        {
            _token = null;
        }

        /// <summary>
        /// 检查Token是否可用
        /// </summary>
        /// <returns></returns>
        public bool IsTokenValid()
        {
            if (_token == null)
                return false;

            if (string.IsNullOrEmpty(_token.AccessToken))
                return false;

            var escaped = DateTime.Now - _token.Timestamp;

            // 检查Token是否已过期
            if (escaped.TotalSeconds > _token.ExpiresIn)
                return false;

            return true;
        }

        /// <summary>
        /// 调用接口
        /// </summary>
        /// <param name="api">接口路径，不是全路径。</param>
        /// <returns>JSON</returns>
        public async Task<string> GetAsync(string api)
        {
            // Token无效则尝试刷新一次
            if (!IsTokenValid())
            {
                _logger.LogInformation("刷新 Pixiv token");
                await RefreshTokenAsync();
            }

            // 还是无效就直返回空结果
            if (!IsTokenValid())
            {
                _logger.LogError("Pixiv token 无效");
                return string.Empty;
            }

            var url = $"{API_URL}/{api}";

            _logger.LogInformation("请求接口 {url}", url);

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var timeHash = CreateClientTimeHash();

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token!.AccessToken);
            request.Headers.Add("X-Client-Time", timeHash.Item1);
            request.Headers.Add("X-Client-Hash", timeHash.Item2);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return string.Empty;

            var json = await response.Content.ReadAsStringAsync();

            return json;
        }

        /// <summary>
        /// 调用接口
        /// </summary>
        /// <param name="api">接口路径，不是全路径。</param>
        /// <param name="retry">失败后重试次数</param>
        /// <returns></returns>
        public async Task<string> GetAsync(string api, int retry)
        {
            var count = Math.Max(1, retry);
            var refreshToken = false;

            while (count > 0)
            {
                var result = await GetAsync(api);

                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }

                if (!refreshToken)
                {
                    _logger.LogInformation("刷新 Pixiv token");
                    await RefreshTokenAsync();
                    refreshToken = true;
                }

                count--;
            }

            return string.Empty;
        }

        /// <summary>
        /// 从服务器下载图片
        /// </summary>
        /// <param name="url">图片地址，只能从<c>i.pximg.net</c>下载。</param>
        /// <returns>图片数据</returns>
        public async Task<byte[]> DownloadImageAsync(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            var timeHash = CreateClientTimeHash();

            request.Headers.Referrer = new Uri("https://app-api.pixiv.net/");
            request.Headers.Add("X-Client-Time", timeHash.Item1);
            request.Headers.Add("X-Client-Hash", timeHash.Item2);

            var response = await _httpClient.SendAsync(request);
            var data = await response.Content.ReadAsByteArrayAsync();

            return data;
        }
    }
}
