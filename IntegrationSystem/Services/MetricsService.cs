using Prometheus;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace IntegrationSystem.Services
{
    public class MetricsService
    {
        private readonly Counter _ordersProcessed;
        private readonly Gauge _currentActiveOrders;
        private readonly Histogram _orderProcessingDuration;
        private readonly Summary _orderProcessingSummary;

        public MetricsService()
        {
            _ordersProcessed = Metrics.CreateCounter("integration_orders_processed_total", "Antal ordrar som har processats");
            _currentActiveOrders = Metrics.CreateGauge("integration_current_active_orders", "Antal aktiva ordrar");

            _orderProcessingDuration = Metrics.CreateHistogram(
                "integration_order_processing_duration_seconds",
                "Tidsfördelning för orderhantering i sekunder",
                new HistogramConfiguration
                {
                    Buckets = Histogram.ExponentialBuckets(0.1, 2, 8)
                });

            _orderProcessingSummary = Metrics.CreateSummary("integration_order_duration_summary_seconds", "Sammanställning av orderhanteringstid");
        }

        public void IncrementOrdersProcessed() => _ordersProcessed.Inc();

        public void SetCurrentActiveOrders(int count) => _currentActiveOrders.Set(count);

        public async Task TrackOrderSummaryAsync(Func<Task> action)
        {
            var stopwatch = Stopwatch.StartNew();
            await action();
            stopwatch.Stop();

            _orderProcessingSummary.Observe(stopwatch.Elapsed.TotalSeconds);
        }
    }
}
