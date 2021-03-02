using System;
using System.Threading.Tasks;
using Polly;
using Serilog;

namespace RetryPolicy
{
    static public class Model
    {
        public static Polly.Retry.AsyncRetryPolicy RetryToManyReq()
        {
            Polly.Retry.AsyncRetryPolicy retryPolicy = Policy
                .Handle<Exception>(ex => ex.Message.Contains("Too many requests"))
                .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.5, retryAttempt)),
                (exception, timespan) =>
                {
                    Log.Warning(exception.Message);
                    Log.Warning("Start retray. Timespan = " + timespan);
                });

            return retryPolicy;
        }

        public static Polly.Retry.AsyncRetryPolicy Retry()
        {
            Polly.Retry.AsyncRetryPolicy retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt)),
                (exception, timespan) =>
                {
                    Log.Warning(exception.Message);
                    Log.Warning("Start retray. Timespan = " + timespan);
                });

            return retryPolicy;
        }
    }
}

