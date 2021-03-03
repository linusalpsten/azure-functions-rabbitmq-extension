// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions;
using Microsoft.Azure.WebJobs.Extensions.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace WebJobs.Extensions.RabbitMQ.Tests
{
    public class RabbitMQConfigProviderTests
    {
        private static readonly IConfiguration _emptyConfig = new ConfigurationBuilder().Build();

        [Fact]
        public void Creates_Context_Correctly()
        {
            var options = new RabbitMQOptions { HostName = Constants.LocalHost, RoutingKey = "hello" };
            var loggerFactory = new LoggerFactory();
            var mockServiceFactory = new Mock<IRabbitMQServiceFactory>();
            var mockNameResolver = new Mock<INameResolver>();
            var config = new RabbitMQExtensionConfigProvider(new OptionsWrapper<RabbitMQOptions>(options), mockNameResolver.Object, mockServiceFactory.Object, (ILoggerFactory)loggerFactory, _emptyConfig);
            var attribute = new RabbitMQAttribute { HostName = "131.107.174.10", RoutingKey = "route" };

            var actualContext = config.CreateContext(attribute);

            RabbitMQAttribute attr = new RabbitMQAttribute
            {
                HostName = "131.107.174.10",
                RoutingKey = "route",
            };

            RabbitMQContext expectedContext = new RabbitMQContext
            {
                ResolvedAttribute = attr,
            };

            Assert.Equal(actualContext.ResolvedAttribute.HostName, expectedContext.ResolvedAttribute.HostName);
            Assert.Equal(actualContext.ResolvedAttribute.RoutingKey, expectedContext.ResolvedAttribute.RoutingKey);
        }

        [Theory]
        [InlineData(Constants.LocalHost, "route", null, null)]
        [InlineData(null, "hello", Constants.LocalHost, null)]
        [InlineData(null, null, Constants.LocalHost, "name")]
        public void Handles_Null_Attributes_And_Options(string attrHostname, string attrQueueName, string optHostname, string optQueueName)
        {
            RabbitMQAttribute attr = new RabbitMQAttribute
            {
                HostName = attrHostname,
                RoutingKey = attrQueueName,
            };

            RabbitMQOptions opt = new RabbitMQOptions
            {
                HostName = optHostname,
                RoutingKey = optQueueName,
            };

            var loggerFactory = new LoggerFactory();
            var mockServiceFactory = new Mock<IRabbitMQServiceFactory>();
            var mockNameResolver = new Mock<INameResolver>();
            var config = new RabbitMQExtensionConfigProvider(new OptionsWrapper<RabbitMQOptions>(opt), mockNameResolver.Object, mockServiceFactory.Object, (ILoggerFactory)loggerFactory, _emptyConfig);
            var actualContext = config.CreateContext(attr);

            if (optHostname == null && optQueueName == null)
            {
                Assert.Equal(actualContext.ResolvedAttribute.HostName, attrHostname);
                Assert.Equal(actualContext.ResolvedAttribute.RoutingKey, attrQueueName);
            }
            else if (attrHostname == null && optQueueName == null)
            {
                Assert.Equal(actualContext.ResolvedAttribute.HostName, optHostname);
                Assert.Equal(actualContext.ResolvedAttribute.RoutingKey, attrQueueName);
            }
            else
            {
                Assert.Equal(actualContext.ResolvedAttribute.HostName, optHostname);
                Assert.Equal(actualContext.ResolvedAttribute.RoutingKey, optQueueName);
            }
        }
    }
}
