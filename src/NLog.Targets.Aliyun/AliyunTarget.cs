using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Aliyun.Api.LogService;
using Aliyun.Api.LogService.Domain.Log;
using Aliyun.Api.LogService.Infrastructure.Protocol.Http;

using NLog.Config;

namespace NLog.Targets.Aliyun
{
    [Target("Aliyun")]
    public class AliyunTarget : AsyncTaskTarget
    {
        private HttpLogServiceClient _client;

        [RequiredParameter]
        public string Endpoint { get; set; }

        [RequiredParameter]
        public string Project { get; set; }

        [RequiredParameter]
        public string AccessKeyId { get; set; }

        [RequiredParameter]
        public string AccessKey { get; set; }

        [RequiredParameter]
        public string LogStore { get; set; }

        public string Source { get; set; }

        public string Topic { get; set; }

        /// <inheritdoc />
        protected override void InitializeTarget()
        {
            _client = LogServiceClientBuilders
                .HttpBuilder
                .Endpoint(Endpoint, Project)
                .Credential(AccessKeyId, AccessKey)
                .Build();

            base.InitializeTarget();
        }

        /// <inheritdoc />
        protected override void CloseTarget()
        {
            _client = null;

            base.CloseTarget();
        }

        /// <inheritdoc />
        protected override async Task WriteAsyncTask(LogEventInfo logEvent, CancellationToken cancellationToken)
        {
            var logStore = RenderLogEvent(LogStore, logEvent);
            var logGroupInfo = ConvertToLogGroupInfo(logEvent);

            await LogServiceClientExtensions.PostLogStoreLogsAsync(_client, logStore, logGroupInfo);
        }

        private LogGroupInfo ConvertToLogGroupInfo(LogEventInfo logEvent)
        {
            var logInfo = new LogInfo
            {
                Time = new DateTimeOffset(logEvent.TimeStamp)
            };

            var topic = GetTopic(Topic, logEvent);
            var source = GetSource(Source, logEvent);
            var contents = new Dictionary<string, string>();
            var includeProperties = ShouldIncludeProperties(logEvent);

            if (includeProperties)
            {
                var properties = GetAllProperties(logEvent);

                foreach (var prop in properties)
                {
                    if (string.IsNullOrEmpty(prop.Key))
                    {
                        continue;
                    }

                    if (!contents.ContainsKey(prop.Key))
                    {
                        contents.Add(prop.Key, prop.Value?.ToString());
                    }
                }
            }
            else
            {
                foreach (var property in ContextProperties)
                {
                    if (string.IsNullOrEmpty(property.Name))
                    {
                        continue;
                    }

                    var content = RenderLogEvent(property.Layout, logEvent);

                    if (string.IsNullOrWhiteSpace(content) && !property.IncludeEmptyValue)
                    {
                        continue;
                    }

                    if (!contents.ContainsKey(property.Name))
                    {
                        contents.Add(property.Name, content);
                    }
                }
            }

            if (!includeProperties && ContextProperties.Count == 0)
            {
                var message = RenderLogEvent(Layout, logEvent) ?? string.Empty;

                // In the case that no property was set, fallback to default configuration.
                contents = new Dictionary<string, string>
                {
                    { "message", message },
                    { "level", logEvent.Level.Name },
                    { "sequence", logEvent.SequenceID.ToString() }
                };

                if (null != logEvent.Exception)
                {
                    contents.Add("exception", logEvent.Exception.ToString());
                }
            }

            logInfo.Contents = contents;

            return new LogGroupInfo
            {
                Topic = topic,
                Source = source,
                Logs = new List<LogInfo> { logInfo }
            };
        }

        private string GetTopic(string configuredTopic, LogEventInfo logEvent)
        {
            var topic = configuredTopic;

            if (!string.IsNullOrWhiteSpace(topic))
            {
                topic = RenderLogEvent(topic, logEvent);
            }
            else if (null == topic)
            {
                topic = logEvent.LoggerName;
            }

            return topic;
        }

        private string GetSource(string configuredSource, LogEventInfo logEvent)
        {
            var source = configuredSource;

            try
            {
                if (!string.IsNullOrWhiteSpace(source))
                {
                    source = RenderLogEvent(source, logEvent);
                }
                else if (null == source)
                {
                    source = Dns.GetHostName();

                    var hostInfo = Dns.GetHostAddresses(Dns.GetHostName());
                    var ipAddress = hostInfo.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);

                    if (null != ipAddress)
                    {
                        source = ipAddress?.ToString();
                    }
                }
            }
            catch
            {
                // Empty.
            }

            return source;
        }
    }
}
