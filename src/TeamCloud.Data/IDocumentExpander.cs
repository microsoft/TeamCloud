/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Data
{
    public abstract class DocumentExpander : IDocumentExpander
    {
        private static readonly ConcurrentDictionary<Type, bool> expandableDocumentTypes = new ConcurrentDictionary<Type, bool>();
        private static readonly ConcurrentDictionary<Type, MethodInfo> expandMethods = new ConcurrentDictionary<Type, MethodInfo>();
        private static readonly ConcurrentDictionary<Type, Metric> expandMetrics = new ConcurrentDictionary<Type, Metric>();    

        private readonly TelemetryClient telemetryClient;
        private readonly Metric expandMetric;

        protected DocumentExpander(bool optional, TelemetryClient telemetryClient)
        {
            Optional = optional;

            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            this.expandMetric = telemetryClient.GetMetric("DocumentExpander", "ExpanderType", "DocumentType");
        }

        public bool Optional { get; }

        public virtual bool CanExpand(IContainerDocument document)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            return expandableDocumentTypes.GetOrAdd(document.GetType(), type => 
                typeof(IDocumentExpander<>).MakeGenericType(type).IsAssignableFrom(GetType()));
        }

        public virtual async Task ExpandAsync(IContainerDocument document)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            if (CanExpand(document))
            {
                var stopwatch = Stopwatch.StartNew();

                var expandMethod = expandMethods.GetOrAdd(document.GetType(), type =>
                    typeof(IDocumentExpander<>).MakeGenericType(type).GetMethod(nameof(ExpandAsync), new Type[] { type }));

                var expandTask = (Task)expandMethod.Invoke(this, new object[] { document });

                await expandTask.ConfigureAwait(false);

                if (!expandMetric.TrackValue(stopwatch.ElapsedMilliseconds, this.GetType().Name, document.GetType().Name))
                    telemetryClient.TrackTrace($"Data series or dimension cap was reached for metric {expandMetric.Identifier.MetricId}.", SeverityLevel.Error);
            }
            else
            {
                throw new NotImplementedException($"Missing document expander implementation IDocumentExpander<{document.GetType().Name}> at {GetType()}");
            }
        }
    }

    public interface IDocumentExpander
    {
        bool Optional { get; }

        bool CanExpand(IContainerDocument document);

        Task ExpandAsync(IContainerDocument document);
    }

    public interface IDocumentExpander<T> : IDocumentExpander
        where T : class, IContainerDocument, new()
    {
        public Task ExpandAsync(T document);
    }
}
