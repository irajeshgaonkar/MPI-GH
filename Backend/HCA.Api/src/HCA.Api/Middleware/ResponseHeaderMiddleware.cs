namespace HCA.Api.Middleware
{
    public class ResponseHeaderMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task Invoke(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers.XContentTypeOptions = "nosniff";
                context.Response.Headers.XFrameOptions = "DENY";
                context.Response.Headers.CacheControl = "no-store";
                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}
