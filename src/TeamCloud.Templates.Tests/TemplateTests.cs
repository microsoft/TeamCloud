/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace TeamCloud.Templates.Tests
{
    public class TemplateTests
    {
        [Fact]
        public void GetResourceTemplateWithoutMerge()
        {
            var templateName = $"{this.GetType().FullName}_Template.txt";
            var template = Assembly.GetExecutingAssembly().GetManifestResourceTemplate(templateName);

            Assert.NotNull(template);
            Assert.NotEmpty(template);
        }

        [Fact]
        public void GetResourceTemplateWithMergeString()
        {
            var templateData = Guid.NewGuid().ToString();
            var templateName = $"{this.GetType().FullName}_Template.txt";
            var template = Assembly.GetExecutingAssembly().GetManifestResourceTemplate(templateName, templateData);

            Assert.NotNull(template);
            Assert.NotEmpty(template);
            Assert.Contains(templateData, template);
        }

        [Fact]
        public void GetResourceTemplateWithMergeValueType()
        {
            var templateData = int.MaxValue;
            var templateName = $"{this.GetType().FullName}_Template.txt";
            var template = Assembly.GetExecutingAssembly().GetManifestResourceTemplate(templateName, templateData);

            Assert.NotNull(template);
            Assert.NotEmpty(template);
            Assert.Contains(templateData.ToString(), template);
        }

        [Fact]
        public void GetResourceTemplateWithMergeClassType()
        {
            var templateData = new TemplateData();
            var templateName = $"{this.GetType().FullName}_Template.txt";
            var template = Assembly.GetExecutingAssembly().GetManifestResourceTemplate(templateName, templateData);

            Assert.NotNull(template);
            Assert.NotEmpty(template);

            Assert.Contains(templateData.Id.ToString(), template);
            Assert.Contains(templateData.Timestamp.ToString(), template);
            Assert.Contains(templateData.User.Id.ToString(), template);
            Assert.Contains(templateData.User.Name.ToString(), template);

            foreach (var item in templateData.Items)
            {
                Assert.Contains(item.Id.ToString(), template);
                Assert.Contains(item.Price.ToString("c"), template);
            }
        }

        [Fact]
        public void GetResourceTemplateWithMergeDynamicType()
        {
            var templateData = new { id = Guid.NewGuid(), timestamp = DateTime.UtcNow, user = new { id = Guid.NewGuid(), name = "Peter Parker" } };
            var templateName = $"{this.GetType().FullName}_Template.txt";
            var template = Assembly.GetExecutingAssembly().GetManifestResourceTemplate(templateName, templateData);

            Assert.NotNull(template);
            Assert.NotEmpty(template);

            Assert.Contains(templateData.id.ToString(), template);
            Assert.Contains(templateData.timestamp.ToString(), template);
            Assert.Contains(templateData.user.id.ToString(), template);
            Assert.Contains(templateData.user.name.ToString(), template);
        }

        [Fact]
        public void GetResourceTemplateWithMergeDynamicTypeMixed()
        {
            var templateData = new { id = Guid.NewGuid(), timestamp = DateTime.UtcNow, user = new TemplateUser() };
            var templateName = $"{this.GetType().FullName}_Template.txt";
            var template = Assembly.GetExecutingAssembly().GetManifestResourceTemplate(templateName, templateData);

            Assert.NotNull(template);
            Assert.NotEmpty(template);

            Assert.Contains(templateData.id.ToString(), template);
            Assert.Contains(templateData.timestamp.ToString(), template);
            Assert.Contains(templateData.user.Id.ToString(), template);
            Assert.Contains(templateData.user.Name.ToString(), template);
        }

        private class TemplateData
        {
            public Guid Id { get; } = Guid.NewGuid();

            public DateTime Timestamp { get; } = DateTime.UtcNow;

            public TemplateUser User { get; } = new TemplateUser();

            public IEnumerable<TemplateItem> Items { get; } = TemplateItem.Create(10).ToArray();
        }

        private class TemplateUser
        {
            public Guid Id { get; } = Guid.NewGuid();

            public string Name { get; } = "Peter Parker";
        }

        private class TemplateItem
        {
            private static readonly Random random = new Random();

            public static IEnumerable<TemplateItem> Create(uint count)
            {
                for (uint i = 0; i < count; i++)
                    yield return new TemplateItem();
            }

            public static decimal RandomPrice()
            {
                var scale = (byte)random.Next(29);
                var sign = random.Next(2) == 1;

                return new decimal(NextFragment(),
                                   NextFragment(),
                                   NextFragment(),
                                   sign,
                                   scale);

                int NextFragment()
                {
                    var firstBits = random.Next(0, 1 << 4) << 28;
                    var lastBits = random.Next(0, 1 << 28);

                    return firstBits | lastBits;
                }
            }

            public Guid Id { get; } = Guid.NewGuid();

            public decimal Price { get; } = RandomPrice();
        }
    }
}
