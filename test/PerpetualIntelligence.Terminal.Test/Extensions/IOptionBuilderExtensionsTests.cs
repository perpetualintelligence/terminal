/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PerpetualIntelligence.Terminal.Hosting;
using PerpetualIntelligence.Terminal.Mocks;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace PerpetualIntelligence.Terminal.Extensions
{
    public class IOptionBuilderExtensionsTests
    {
        [Fact]
        public void ValidationAttribute_Adds_Singleton()
        {
            var serviceCollection = new ServiceCollection();
            var mockBuilder = new Mock<IOptionBuilder>();
            mockBuilder.Setup(x => x.Services).Returns(serviceCollection);

            // Ensure builder is returned
            var result = mockBuilder.Object.ValidationAttribute<MockValidationAttribute>();
            result.Should().BeSameAs(mockBuilder.Object);

            serviceCollection.Should().ContainSingle();
            ServiceDescriptor addedAttr = serviceCollection.First();
            addedAttr.ServiceType.Should().Be<ValidationAttribute>();
            addedAttr.ImplementationInstance.Should().BeOfType<MockValidationAttribute>();
            addedAttr.Lifetime.Should().Be(ServiceLifetime.Singleton);
        }

        [Fact]
        public void ValidationAttribute_With_Args_Adds_Singleton()
        {
            var serviceCollection = new ServiceCollection();
            var mockBuilder = new Mock<IOptionBuilder>();
            mockBuilder.Setup(x => x.Services).Returns(serviceCollection);

            // Ensure builder is returned
            var result = mockBuilder.Object.ValidationAttribute<MockValidationAttribute>("test1", 123);
            result.Should().BeSameAs(mockBuilder.Object);

            serviceCollection.Should().ContainSingle();
            ServiceDescriptor addedAttr = serviceCollection.First();
            addedAttr.ServiceType.Should().Be<ValidationAttribute>();
            addedAttr.ImplementationInstance.Should().BeOfType<MockValidationAttribute>();
            addedAttr.Lifetime.Should().Be(ServiceLifetime.Singleton);

            MockValidationAttribute mockAttr = (MockValidationAttribute)addedAttr.ImplementationInstance!;
            mockAttr.Property1.Should().Be("test1");
            mockAttr.Property2.Should().Be(123);
        }

        [Fact]
        public void ValidationAttribute_WithInvalidType_Throws()
        {
            var mockBuilder = new Mock<IOptionBuilder>();
            mockBuilder.Setup(x => x.Services).Returns(new ServiceCollection());
            Action act = () => mockBuilder.Object.ValidationAttribute(typeof(MockArgumentChecker));
            act.Should().Throw<InvalidCastException>();
        }
    }
}