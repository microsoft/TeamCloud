using Xunit;

// disable parallizing for this test assembly to avoid issues
// based on singleton values e.g. ReferenceLink.BaseUrl

[assembly: CollectionBehavior(DisableTestParallelization = true)]
