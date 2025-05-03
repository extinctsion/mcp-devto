using System.Net.Http.Json;
using MCP.DevTo.Models;
using Microsoft.Extensions.Configuration;
using MCP.DevTo.Helpers;

namespace Service
{
    public class DevToService : IDevToService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string BaseUrl = "https://dev.to/api";

        public DevToService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _apiKey = configuration["DevTo:ApiKey"] ?? throw new ArgumentNullException("DevTo:ApiKey", "API key is missing in configuration.");
        }

        private async Task<T> FetchFromApi<T>(string path, Dictionary<string, string> queryParams = null)
        {
            var query = queryParams != null ? $"?{string.Join("&", queryParams.Select(x => $"{x.Key}={x.Value}"))}" : "";
            var response = await _httpClient.GetAsync($"{path}{query}");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<T>();
            if (result == null)
                throw new InvalidOperationException("Failed to deserialize the response content.");
            return result;
        }

        public async Task<string> GetLatestArticles()
        {
            // Default values as per interface contract
            int page = 1;
            int perPage = 10;
            var queryParams = new Dictionary<string, string>
            {
                { "page", page.ToString() },
                { "per_page", perPage.ToString() }
            };
            var articles = await FetchFromApi<List<Article>>("/articles/latest", queryParams);
            return FormatHelper.FormatArticles(articles);
        }

        public async Task<string> GetTopArticles()
        {
            var articles = await FetchFromApi<List<Article>>("/articles");
            return FormatHelper.FormatArticles(articles.Take(10));
        }

        public async Task<string> GetArticlesByTag(string tag)
        {
            var queryParams = new Dictionary<string, string> { { "tag", tag } };
            var articles = await FetchFromApi<List<Article>>("/articles", queryParams);
            return FormatHelper.FormatArticles(articles.Take(10));
        }

        public async Task<string> GetArticleById(string id)
        {
            var article = await FetchFromApi<Article>($"/articles/{id}");
            return FormatHelper.FormatArticleDetails(article);
        }

        // Implement other interface methods similarly...

        public async Task<string> CreateArticle(string title, string bodyMarkdown, string tags = "", bool published = false)
        {
            var article = new
            {
                article = new
                {
                    title,
                    body_markdown = bodyMarkdown,
                    published,
                    tags
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/articles")
            {
                Content = JsonContent.Create(article)
            };
            request.Headers.Add("api-key", _apiKey);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<Article>();

            if (result == null)
                throw new InvalidOperationException("Failed to deserialize the created article response.");

            return $"Article created successfully with ID: {result.Id}\nURL: {result.Url}";
        }

        public async Task<string> SearchArticles(string query, int page = 1)
        {
            var queryParams = new Dictionary<string, string>
            {
                { "q", query },
                { "page", page.ToString() }
            };
            var articles = await FetchFromApi<List<Article>>("/articles/search", queryParams);
            return FormatHelper.FormatArticles(articles);
        }

        public async Task<string> GetArticleDetails(int articleId)
        {
            var article = await FetchFromApi<Article>($"/articles/{articleId}");
            return FormatHelper.FormatArticleDetails(article);
        }

        public async Task<string> GetArticlesByUsername(string username)
        {
            var queryParams = new Dictionary<string, string> { { "username", username } };
            var articles = await FetchFromApi<List<Article>>("/articles", queryParams);
            return FormatHelper.FormatArticles(articles);
        }

        public async Task<string> GetUserInfo(string username)
        {
            var user = await FetchFromApi<User>($"/users/by_username?url={username}");
            return FormatHelper.FormatUserProfile(user);
        }

        public async Task<string> UpdateArticle(int articleId, string? title = null, string? bodyMarkdown = null, string? tags = null, bool? published = null)
        {
            var article = new
            {
                article = new
                {
                    title = title,
                    body_markdown = bodyMarkdown,
                    published = published,
                    tags = tags
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Put, $"/articles/{articleId}")
            {
                Content = JsonContent.Create(article)
            };
            request.Headers.Add("api-key", _apiKey);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<Article>();

            if (result == null)
                throw new InvalidOperationException("Failed to deserialize the updated article response.");

            return $"Article updated successfully\nURL: {result.Url}";
        }
    }
}
