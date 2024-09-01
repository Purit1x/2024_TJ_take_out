using Quartz;

namespace takeout_tj.Job
{
    public class OrderCleanupJob : IJob
    {
        private readonly OrderCleanupService _orderCleanupService;
        private readonly ILogger<OrderCleanupJob> _logger;
        public OrderCleanupJob(OrderCleanupService orderCleanupService,ILogger<OrderCleanupJob> logger)
        {
            _orderCleanupService = orderCleanupService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            //_logger.LogInformation("OrderCleanupJob started executing.");

            try
            {
                await _orderCleanupService.RemoveExpiredOrders();
                await _orderCleanupService.RemovePaidOrders();
                //_logger.LogInformation("OrderCleanupJob finished executing.");
            }
            catch (Exception ex)
            {
                //_logger.LogError("An error occurred while executing OrderCleanupJob: {Message}", ex.Message);
            }
        }
    }
}

