using System;
using Polly;
using Serilog;

namespace RetryPolicy
{
    static public class Model
    {
        static public Polly.Retry.RetryPolicy RetryTooManyRequest()
        {
            Polly.Retry.RetryPolicy retryPolicy = Policy
            .Handle<Exception>(ex => ex.Message.Contains("Too many requests"))
            .WaitAndRetryForever(retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            (exception, timespan) =>
            {
                Log.Error(exception.Message);
                Log.Error(exception.StackTrace);
                Log.Information("Start retray. Timespan = " + timespan);
            });       
            return retryPolicy;
        }
    }
}

