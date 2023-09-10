/*
    Copyright (c) 2023 Perpetual Intelligence L.L.C. All Rights Reserved.

    For license, terms, and data policies, go to:
    https://terms.perpetualintelligence.com/articles/intro.html
*/

using FluentAssertions;
using PerpetualIntelligence.Terminal.Mocks;
using System;
using System.Threading.Tasks;
using Xunit;

namespace PerpetualIntelligence.Terminal.Stores.InMemory
{
    public class InMemoryCommandDescriptorStoreTests
    {
        public InMemoryCommandDescriptorStoreTests()
        {
            cmdStore = new InMemoryCommandStore(MockCommands.Commands);
        }

        [Fact]
        public async Task FindByIdShouldErrorIfNotFoundAsync()
        {
            Func<Task> act = async () => await cmdStore.FindByIdAsync("invalid_id");
            await act.Should().ThrowAsync<Exception>().WithMessage("The given key 'invalid_id' was not present in the dictionary.");
        }

        [Fact]
        public async Task TryFindByIdShouldNotErrorIfFoundAsync()
        {
            var result = await cmdStore.FindByIdAsync("id1");
            result.Should().NotBeNull();
            result.Id.Should().Be("id1");
        }

        [Fact]
        public async Task AllShouldReturnAllCommands()
        {
            var result = await cmdStore.AllAsync();
            result.Should().NotBeNull();
            result.Count.Should().Be(5);
            result.Keys.Should().Contain("id1");
            result.Keys.Should().Contain("id2");
            result.Keys.Should().Contain("id3");
            result.Keys.Should().Contain("id4");
            result.Keys.Should().Contain("id5");
        }

        private InMemoryCommandStore cmdStore = null!;
    }
}