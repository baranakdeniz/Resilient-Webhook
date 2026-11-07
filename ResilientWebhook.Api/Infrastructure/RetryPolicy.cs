namespace ResilientWebhook.Api.Infrastructure
{
    public static class RetryPolicy
    {



        public const int maxRetries = 3;

        public static async Task ExecuteAsync(Func<Task> action , CancellationToken cancellationToken)
        {
            int attempts = 0;
            while (true)
            {
                try
                {
                    await action();//if the process is successful
                    return;
                }
                catch (Exception ex) when (IsTransient(ex) && attempts < maxRetries)
                {
                    attempts++;
                    var delayMs = (int)Math.Pow(2, attempts) * 1000; // Exponential backoff

                    var jitter = new Random().Next(0, 500); // Add jitter to avoid thundering herd problem

                    Console.WriteLine($"Geçici hata: {ex.Message}. {attempts}. kez tekrar deneniyor ({delayMs + jitter} ms sonra)...");

                    await Task.Delay(delayMs + jitter , cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Hata: {ex.Message}. Tüm denemeler başarısız oldu.");
                    throw;
                }

            }


        }

        private static bool IsTransient(Exception ex)
        {

            return ex is TimeoutException ||
             ex is HttpRequestException ||
             (ex.InnerException is TimeoutException);


        }
    }
}

