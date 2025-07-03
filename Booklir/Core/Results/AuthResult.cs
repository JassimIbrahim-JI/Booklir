namespace Booklir.Core.Results
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}
