using Hangfire;
using InvoiceFlow.Application.Notifications.Jobs;

namespace InvoiceFlow.Infrastructure.Jobs;

public static class HangfireReminderScheduler
{
    public static void RegisterRecurringJobs()
    {
        RecurringJob.AddOrUpdate<SendPaymentReminderJob>(
            "send-payment-reminders",
            job => job.ExecuteAsync(CancellationToken.None),
            Cron.Hourly);
    }
}
