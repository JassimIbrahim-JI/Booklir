using RazorLight;

namespace Booklir.Core.RazorHelper
{
    public class RazorViewToStringRenderer : IRazorViewToStringRenderer
    {
        private readonly RazorLightEngine _engine;

        public RazorViewToStringRenderer()
        {
            _engine = new RazorLightEngineBuilder()
              .UseFileSystemProject(Path.Combine(Directory.GetCurrentDirectory(), "Views"))
              .UseMemoryCachingProvider()
              .Build();
        }

        public Task<string> RenderAsync<TModel>(string viewName, TModel model)
        {
            // viewName should be like "Emails/Newsletter.cshtml"
            return _engine.CompileRenderAsync(viewName, model);
        }
    }
}
