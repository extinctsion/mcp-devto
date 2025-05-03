namespace Service
{
    public interface IDevToService
    {
        Task<string> GetLatestArticles();
        Task<string> GetTopArticles();
        Task<string> GetArticlesByTag(string tag);
        Task<string> GetArticleById(string id);
        Task<string> SearchArticles(string query, int page = 1);
        Task<string> GetArticleDetails(int articleId);
        Task<string> GetArticlesByUsername(string username);
        Task<string> GetUserInfo(string username);
        Task<string> CreateArticle(string title, string bodyMarkdown, string tags = "", bool published = false);
        Task<string> UpdateArticle(int articleId, string? title = null, string? bodyMarkdown = null, string? tags = null, bool? published = null);
    }
}
