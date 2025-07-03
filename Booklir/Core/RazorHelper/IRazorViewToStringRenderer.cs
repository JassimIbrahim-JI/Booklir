namespace Booklir.Core.RazorHelper
{
    public interface IRazorViewToStringRenderer
    {
        Task<string> RenderAsync<TModel>(string viewName, TModel model);
    }
}
