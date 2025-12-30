namespace Validation_Demo.MiddleWare
{
    public class TimingMiddleWare
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TimingMiddleWare> _logger;

        public TimingMiddleWare(RequestDelegate next, ILogger<TimingMiddleWare> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                await _next(context);
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation(" Request {Method} {Path} took {ElapsedMs} ms ", context.Request.Method, context.Request.Path, sw.ElapsedMilliseconds);
            }
        }
    }
}
