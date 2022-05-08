using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Velocity.API.Exceptions;
using Velocity.API.Models;
using Velocity.API.Repositories;
using Xunit;

namespace Velocity.API.Tests.Repositories;

public class OwnerRepositoryTests
{
    private readonly OwnerRepository _ownerRepository;

    public OwnerRepositoryTests()
    {
        _ownerRepository = new OwnerRepository();
    }

    [Fact]
    public async Task UpsertOwner_Returns_Owner_WhenOwnerIsNew_AndSumDoesNotExceed100()
    {
        var owner = new Owner();
        
        var result = await _ownerRepository.UpsertOwner(owner);

        result.Should().Be(owner);
    }
    
    [Fact]
    public async Task UpsertOwner_Returns_Owner_WhenOwnerExists_AndSumDoesNotExceed100()
    {
        var owner = new Owner { Id = Guid.NewGuid() };
        
        await _ownerRepository.UpsertOwner(owner);
        
        owner.Percentage = 100;
        var result = await _ownerRepository.UpsertOwner(owner);

        result.Should().Be(owner);
    }
    
    [Fact]
    public async Task UpsertOwner_Throws_PercentageOverflowException_WhenOwnerIsNew_AndSumExceeds100()
    {
        var owner = new Owner { Percentage = 101 };

        Func<Task> action = async () => await _ownerRepository.UpsertOwner(owner);

        await action.Should().ThrowAsync<PercentageOverflowException>();
    }
    
    [Fact]
    public async Task UpsertOwner_Throws_PercentageOverflowException_WhenOwnerExists_AndSumExceeds100()
    {
        var owner = new Owner { Id = Guid.NewGuid(), CompanyId = Guid.NewGuid(), Percentage = 50 };
        
        await _ownerRepository.UpsertOwner(owner);

        owner.Percentage = 101;

        Func<Task> action = async () => await _ownerRepository.UpsertOwner(owner);

        await action.Should().ThrowAsync<PercentageOverflowException>();
    }
    
    [Fact]
    public async Task GetOwnerById_Returns_Owner_WhenFoundById()
    {
        var guid = Guid.NewGuid();
        await _ownerRepository.UpsertOwner(new Owner { Id = guid });
        
        var result = await _ownerRepository.GetOwnerById(guid);

        result.Should().NotBeNull();
        result.Should().BeAssignableTo(typeof(Owner));
        result.Id.Should().Be(guid);
    }
    
    [Fact]
    public async Task GetOwnerById_Returns_Null_WhenNotFoundById()
    {
        var guid = Guid.NewGuid();

        var result = await _ownerRepository.GetOwnerById(guid);

        result.Should().BeNull();
    }
    
    [Fact]
    public async Task GetAllOwners_Returns_IEnumerableOfOwner()
    {
        var result = await _ownerRepository.GetAllOwners();

        result.Should().NotBeNull();
        result.GetType().Should().BeAssignableTo(typeof(IEnumerable<Owner>));
    }
    
    [Fact]
    public async Task GetOwnersByCompany_Returns_IEnumerableOfOwner()
    {
        var guid = Guid.NewGuid();
        
        var result = await _ownerRepository.GetOwnersByCompany(guid);

        result.Should().NotBeNull();
        result.GetType().Should().BeAssignableTo(typeof(IEnumerable<Owner>));
        foreach (var owner in result)
        {
            owner.CompanyId.Should().Be(guid);
        }
    }
}