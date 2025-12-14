using Liberte360Pay.Metrics;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace SampleLiberteTesting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MetricsController : ControllerBase
    {
        private readonly IMetricsCollector _metricsCollector;

        public MetricsController(IMetricsCollector metricsCollector)
        {
            _metricsCollector = metricsCollector;
        }

        [HttpGet("api-metrics")]
        public IActionResult GetApiMetrics()
        {
            var metrics = _metricsCollector.GetMetrics();

            var summary = metrics.Select(kvp => new
            {
                Endpoint = kvp.Key,
                TotalCalls = kvp.Value.TotalCalls,
                SuccessRate = kvp.Value.TotalCalls > 0
                    ? (double)kvp.Value.SuccessfulCalls / kvp.Value.TotalCalls * 100
                    : 0,
                AverageDurationMs = kvp.Value.AverageDurationMs,
                AverageRequestSize = kvp.Value.AverageRequestSize,
                AverageResponseSize = kvp.Value.AverageResponseSize,
                ErrorCount = kvp.Value.ErrorCount,
                ErrorCodes = kvp.Value.ErrorCodes.ToArray()
            });

            return Ok(summary);
        }
    }
}
