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
        private static readonly ConcurrentDictionary<string, bool> expandableDocumentTypes = new ConcurrentDictionary<string, bool>();
        private static readonly ConcurrentDictionary<string, MethodInfo> expandMethods = new ConcurrentDictionary<string, MethodInfo>();
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

        private string GetLookupKey(IContainerDocument document)
            => $"{this.GetType()}|{document.GetType()}";

        public virtual bool CanExpand(IContainerDocument document)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

           return expandableDocumentTypes.GetOrAdd(GetLookupKey(document), _ => 
                typeof(IDocumentExpander<>).MakeGenericType(document.GetType()).IsAssignableFrom(GetType()));
        }

        public virtual async Task ExpandAsync(IContainerDocument document)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            if (CanExpand(document))
            {
                var stopwatch = Stopwatch.StartNew();

                var expandMethod = expandMethods.GetOrAdd(GetLookupKey(document), _ =>
                    typeof(IDocumentExpander<>).MakeGenericType(document.GetType()).GetMethod(nameof(ExpandAsync), new Type[] { document.GetType() }));

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
