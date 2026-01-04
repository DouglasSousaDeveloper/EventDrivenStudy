namespace Shared.RabbitMq;

public static class RabbitMqTopology
{
    //Order
    public const string OrderExchange = "order.exchange";
    public const string OrderCreatedRoutingKey = "order.created";
    public const string OrderCreatedDlqRoutingKey = "order.created.dlq";

    //Billing
    public const string BillingQueue = "billing.queue";
    public const string BillingRetryQueue = "billing.retry.queue";
    public const string BillingDlqQueue = "billing.dlq";
    public const string BillingRetryExchange = "billing.retry.exchange";


    // Notification
    public const string NotificationQueue = "notification.queue";
    public const string NotificationRetryQueue = "notification.retry.queue";
    public const string NotificationDlqQueue = "notification.dlq";
    public const string NotificationRetryExchange = "notification.retry.exchange";

    public const int RetryDelayMilliseconds = 5000;
    public const int MaxRetryCount = 3;
}