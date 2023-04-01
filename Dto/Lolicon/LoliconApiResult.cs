namespace SoraBot.Dto.Lolicon
{
    internal class LoliconApiResult<TResponse> where TResponse : class, new()
    {
        public string Error { get; set; } = string.Empty;
        public TResponse Data { get; set; } = new();
    }
}
