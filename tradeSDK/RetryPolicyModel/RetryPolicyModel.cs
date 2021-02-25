using System;
using Polly;

namespace RetryPolicyModel
{
    static public class RetryPolicyModel
    {
        static Polly.Retry.RetryPolicy getRetry()
        {
                Polly.Retry.RetryPolicy retryPolicy = Policy
                .Handle<Exception>(ex => ex.Message.Contains("Too many requests"))
                .WaitAndRetryForever(retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
        );
            return retryPolicy;
    }
    }
}
