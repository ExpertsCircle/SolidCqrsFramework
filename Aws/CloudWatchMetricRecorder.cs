using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.CloudWatch;
using Amazon.CloudWatch.Model;

namespace SolidCqrsFramework.Aws
{
    public class CloudWatchMetricRecorder
    {
        private readonly AmazonCloudWatchClient _cloudWatchClient;
        private readonly string _namespace;

        public CloudWatchMetricRecorder(string ns)
        {
            _namespace = ns;
            _cloudWatchClient = new AmazonCloudWatchClient();
        }

        public async Task RecordCloudWatchMetric(string metricName, double value, string eventName)
        {
            var request = new PutMetricDataRequest
            {
                Namespace = _namespace,
                MetricData = new List<MetricDatum>
                {
                    new MetricDatum
                    {
                        MetricName = metricName,
                        Unit = StandardUnit.Count,
                        Value = value,
                        Dimensions = new List<Dimension>
                        {
                            new Dimension
                            {
                                Name = "EventName",
                                Value = eventName
                            }
                        }
                    }
                }
            };

            await _cloudWatchClient.PutMetricDataAsync(request);
        }
    }
}
