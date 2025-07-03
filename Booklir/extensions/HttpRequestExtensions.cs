namespace Booklir.extensions
{    

    public static class HttpRequestExtensions
    {
        
        // IsAjaxRequest extension method for HttpRequest 
        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            // Check for the X-Requested-With header
            if (request.Headers.TryGetValue("X-Requested-With", out var headerValues))
            {
                return headerValues.Any(v =>
               v.Contains("XMLHttpRequest", StringComparison.OrdinalIgnoreCase));
            }

            // If the X-Requested-With header is not present, check for legacy systems
            // 2. Check query parameter (legacy systems)
            if (request.Query.TryGetValue("X-Requested-With", out var queryValues))
            {
                return queryValues.Any(v => v.Contains("XMLHttpRequest", StringComparison.OrdinalIgnoreCase));
            }

            return false;
        }
    }
}
