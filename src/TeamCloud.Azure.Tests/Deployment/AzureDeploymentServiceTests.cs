/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NSubstitute;
using TeamCloud.Azure.Deployment;
using TeamCloud.Azure.Tests.Deployment.Templates;
using Xunit;

namespace TeamCloud.Azure.Tests.Deployment
{
    public class AzureDeploymentServiceTests : HttpTestContext
    {
        [Fact]
        public async Task WaitForDeployment()
        {
            var azureSessionFactory = Substitute.For<IAzureSessionService>();
            azureSessionFactory.AcquireTokenAsync(AzureAuthorities.AzureResourceManager).Returns(Guid.NewGuid().ToString());

            var azureDeploymentArtifactContainer = Substitute.For<IAzureDeploymentArtifactsContainer>();
            azureDeploymentArtifactContainer.Location.Returns("http://storage.com");
            azureDeploymentArtifactContainer.Token.Returns("?token");

            var azureDeploymentArtifactStorage = Substitute.For<IAzureDeploymentArtifactsProvider>();
            azureDeploymentArtifactStorage.UploadArtifactsAsync(default, default).ReturnsForAnyArgs(azureDeploymentArtifactContainer);

            var azureDeploymentOptions = Substitute.For<IAzureDeploymentOptions>();
            azureDeploymentOptions.Region.Returns("West Europe");

            var deploymentService = new AzureDeploymentService(azureDeploymentOptions, azureSessionFactory, azureDeploymentArtifactStorage);
            var deploymentTemplate = await AzureDeploymentTemplate.CreateAsync<SimpleTemplate>().ConfigureAwait(false);

            var deployment = await deploymentService
                .DeployTemplateAsync(deploymentTemplate, Guid.Empty)
                .ConfigureAwait(false);

            using (WithResponses(nameof(WaitForDeployment)))
            {
                var deploymentState = await deployment
                    .WaitAsync()
                    .ConfigureAwait(false);

                Assert.Equal(AzureDeploymentState.Succeeded, deploymentState);
            }
        }

        [Fact]
        public async Task WaitForDeploymentWithThrowOnError()
        {
            var azureSessionFactory = Substitute.For<IAzureSessionService>();
            azureSessionFactory.AcquireTokenAsync(AzureAuthorities.AzureResourceManager).Returns(Guid.NewGuid().ToString());

            var azureDeploymentArtifactContainer = Substitute.For<IAzureDeploymentArtifactsContainer>();
            azureDeploymentArtifactContainer.Location.Returns("http://storage.com");
            azureDeploymentArtifactContainer.Token.Returns("?token");

            var azureDeploymentArtifactStorage = Substitute.For<IAzureDeploymentArtifactsProvider>();
            azureDeploymentArtifactStorage.UploadArtifactsAsync(default, default).ReturnsForAnyArgs(azureDeploymentArtifactContainer);

            var azureDeploymentOptions = Substitute.For<IAzureDeploymentOptions>();
            azureDeploymentOptions.Region.Returns("West Europe");

            var deploymentService = new AzureDeploymentService(azureDeploymentOptions, azureSessionFactory, azureDeploymentArtifactStorage);
            var deploymentTemplate = await AzureDeploymentTemplate.CreateAsync<SimpleTemplate>().ConfigureAwait(false);

            var deployment = await deploymentService
                .DeployTemplateAsync(deploymentTemplate, Guid.Empty)
                .ConfigureAwait(false);

            using (WithResponses(nameof(WaitForDeploymentWithThrowOnError)))
            {
                _ = Assert.ThrowsAsync<ApplicationException>(async () =>
                {
                    var deploymentState = await deployment
                        .WaitAsync(true)
                        .ConfigureAwait(false);

                }).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task WaitForDeploymentWithCleanUp()
        {
            var azureSessionFactory = Substitute.For<IAzureSessionService>();
            azureSessionFactory.AcquireTokenAsync(AzureAuthorities.AzureResourceManager).Returns(Guid.NewGuid().ToString());

            var azureDeploymentArtifactContainer = Substitute.For<IAzureDeploymentArtifactsContainer>();
            azureDeploymentArtifactContainer.Location.Returns("http://storage.com");
            azureDeploymentArtifactContainer.Token.Returns("?token");

            var azureDeploymentArtifactStorage = Substitute.For<IAzureDeploymentArtifactsProvider>();
            azureDeploymentArtifactStorage.UploadArtifactsAsync(default, default).ReturnsForAnyArgs(azureDeploymentArtifactContainer);

            var azureDeploymentOptions = Substitute.For<IAzureDeploymentOptions>();
            azureDeploymentOptions.Region.Returns("West Europe");

            var deploymentService = new AzureDeploymentService(azureDeploymentOptions, azureSessionFactory, azureDeploymentArtifactStorage);
            var deploymentTemplate = await AzureDeploymentTemplate.CreateAsync<SimpleTemplate>().ConfigureAwait(false);

            var deployment = await deploymentService
                .DeployTemplateAsync(deploymentTemplate, Guid.Empty)
                .ConfigureAwait(false);

            using (WithResponses(nameof(WaitForDeploymentWithCleanUp)))
            {
                var deploymentState = await deployment
                    .WaitAsync(cleanUp: true)
                    .ConfigureAwait(false);

                Assert.Equal(AzureDeploymentState.Succeeded, deploymentState);

                Assert.Contains(CallLog, call => call.Request.Method == HttpMethod.Delete
                    && call.Request.RequestUri.ToString().Contains(deployment.ResourceId, StringComparison.OrdinalIgnoreCase));
            }
        }

        [Fact]
        public async Task GetDeploymentOutput()
        {
            var azureSessionFactory = Substitute.For<IAzureSessionService>();
            azureSessionFactory.AcquireTokenAsync(AzureAuthorities.AzureResourceManager).Returns(Guid.NewGuid().ToString());

            var azureDeploymentArtifactContainer = Substitute.For<IAzureDeploymentArtifactsContainer>();
            azureDeploymentArtifactContainer.Location.Returns("http://storage.com");
            azureDeploymentArtifactContainer.Token.Returns("?token");

            var azureDeploymentArtifactStorage = Substitute.For<IAzureDeploymentArtifactsProvider>();
            azureDeploymentArtifactStorage.UploadArtifactsAsync(default, default).ReturnsForAnyArgs(azureDeploymentArtifactContainer);

            var azureDeploymentOptions = Substitute.For<IAzureDeploymentOptions>();
            azureDeploymentOptions.Region.Returns("West Europe");

            var deploymentService = new AzureDeploymentService(azureDeploymentOptions, azureSessionFactory, azureDeploymentArtifactStorage);
            var deploymentTemplate = await AzureDeploymentTemplate.CreateAsync<SimpleTemplate>().ConfigureAwait(false);

            var deployment = await deploymentService
                .DeployTemplateAsync(deploymentTemplate, Guid.Empty)
                .ConfigureAwait(false);

            using (WithResponses(nameof(GetDeploymentOutput)))
            {
                var deploymentOutput = await deployment
                    .GetOutputAsync()
                    .ConfigureAwait(false);

                Assert.NotNull(deploymentOutput);
                Assert.NotEmpty(deploymentOutput);

                Assert.IsType<string>(deploymentOutput["stringOutput"]);
                Assert.IsType<int>(deploymentOutput["intOutput"]);
                Assert.IsType<bool>(deploymentOutput["boolOutput"]);
                Assert.IsType<JArray>(deploymentOutput["arrayOutput"]);
                Assert.IsType<JObject>(deploymentOutput["objectOutput"]);
            }
        }

        [Fact]
        public async Task GetDeploymentOutputWhileRunning()
        {
            var azureSessionFactory = Substitute.For<IAzureSessionService>();
            azureSessionFactory.AcquireTokenAsync(AzureAuthorities.AzureResourceManager).Returns(Guid.NewGuid().ToString());

            var azureDeploymentArtifactContainer = Substitute.For<IAzureDeploymentArtifactsContainer>();
            azureDeploymentArtifactContainer.Location.Returns("http://storage.com");
            azureDeploymentArtifactContainer.Token.Returns("?token");

            var azureDeploymentArtifactStorage = Substitute.For<IAzureDeploymentArtifactsProvider>();
            azureDeploymentArtifactStorage.UploadArtifactsAsync(default, default).ReturnsForAnyArgs(azureDeploymentArtifactContainer);

            var azureDeploymentOptions = Substitute.For<IAzureDeploymentOptions>();
            azureDeploymentOptions.Region.Returns("West Europe");

            var deploymentService = new AzureDeploymentService(azureDeploymentOptions, azureSessionFactory, azureDeploymentArtifactStorage);
            var deploymentTemplate = await AzureDeploymentTemplate.CreateAsync<SimpleTemplate>().ConfigureAwait(false);

            var deployment = await deploymentService
                .DeployTemplateAsync(deploymentTemplate, Guid.Empty)
                .ConfigureAwait(false);

            using (WithResponses(nameof(GetDeploymentOutputWhileRunning)))
            {
                _ = Assert.ThrowsAsync<ApplicationException>(async () =>
                {

                    var deploymentOutput = await deployment
                          .GetOutputAsync()
                          .ConfigureAwait(false);

                }).ConfigureAwait(false);
            }
        }
    }
}
