using Prometheus;
using System.Threading.Tasks;

namespace IntegrationSystem.Services
{
    public class MetricsService
    {
        private readonly Counter _ordersProcessed;
        private readonly Gauge _currentActiveOrders;

        public MetricsService()
        {
            _ordersProcessed = Metrics.CreateCounter("integration_orders_processed_total", "Antal ordrar som har processats");
            _currentActiveOrders = Metrics.CreateGauge("integration_current_active_orders", "Antal aktiva ordrar");
        }

        public void IncrementOrdersProcessed()
        {
            _ordersProcessed.Inc();
        }

        public void SetCurrentActiveOrders(int count)
        {
            _currentActiveOrders.Set(count);
        }
    }
}
