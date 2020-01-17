/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using TeamCloud.Azure.Deployments;
using TeamCloud.Azure.Tests.Deployments.Templates;
using Xunit;

namespace TeamCloud.Azure.Tests.Deployments
{
    public class AzureTemplateTests : HttpTestContext
    {
        private readonly string[] TemplateParameterTypes = new string[]
        {
            "array",
            "bool",
            "int",
            "object",
            "secureObject",
            "securestring",
            "string"
        };


        [Fact]
        public async Task CreateSimpleTemplate()
        {
            var template = await AzureDeploymentTemplate
                .CreateAsync<SimpleTemplate>()
                .ConfigureAwait(false);

            Assert.NotNull(template.Template);
            Assert.NotNull(template.Parameters);
            Assert.NotNull(template.LinkedTemplates);

            Assert.Contains(template.Parameters.Keys, parameterName => TemplateParameterTypes.Contains(parameterName));
        }



        [Fact]
        public async Task CreateComplexTemplate()
        {
            var template = await AzureDeploymentTemplate
                .CreateAsync<ComplexTemplate>()
                .ConfigureAwait(false);

            Assert.NotNull(template.Template);
            Assert.NotNull(template.Parameters);
            Assert.NotNull(template.LinkedTemplates);

            var linkedTemplates = new string[] { "Linked1.json", "Linked2.json" };

            Assert.Contains(template.LinkedTemplates.Keys, templateName => linkedTemplates.Contains(templateName));
        }


    }
}
