using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Velocity.API.Controllers;
using Velocity.API.Exceptions;
using Velocity.API.Models;
using Velocity.API.Repositories;
using Xunit;

namespace Velocity.API.Tests.Controllers;

public class OwnerControllerTests
{
    private readonly OwnerController _ownerController;
    private readonly Mock<IOwnerRepository> _ownerRepository = new Mock<IOwnerRepository>();

    private static readonly Guid _guid = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
    private readonly IDictionary<Guid, Owner> _data = new Dictionary<Guid, Owner>
    {
        {
            _guid,
            new Owner
            {
                Id = _guid,
                CompanyId = Guid.Parse("5adc4148-cc6a-4090-9de4-11c08584f5b5"),
                Name = "Test Person",
                Percentage = 49.5
            }
        }
    };
    
    public OwnerControllerTests()
    {
        _ownerRepository.Setup(x => x.UpsertOwner(It.IsAny<Owner>())).ThrowsAsync(new PercentageOverflowException());
        _ownerRepository.Setup(x => x.GetOwnerById(It.IsAny<Guid>())).ReturnsAsync((Owner)null);
        _ownerRepository.Setup(x => x.GetOwnerById(_guid)).ReturnsAsync(_data[_guid]);
        _ownerRepository.Setup(x => x.GetAllOwners()).ReturnsAsync(_data.Values);

        _ownerController = new OwnerController(_ownerRepository.Object);
    }

    #region UpsertOwner

    [Fact]
    public async Task UpsertOwner_Returns_OkResult_ContainingOwner_WhenRequestIsValid()
    {
        var owner = new Owner { Id = Guid.NewGuid() };
        
        _ownerRepository.Setup(x => x.UpsertOwner(owner)).ReturnsAsync(owner);
        
        var result = await _ownerController.UpsertOwner(owner) as ObjectResult;
        var value = result!.Value as Owner;

        result.StatusCode.Should().Be(StatusCodes.Status200OK);
        value.Should().NotBeNull();
        value!.Id.Should().Be(owner.Id);
    }
    
    [Fact]
    public async Task UpsertOwner_Returns_ObjectResult_ContainingErrorResponse_WhenRequestIsInvalid()
    {
        var owner = new Owner { Percentage = 101 };
        
        var result = await _ownerController.UpsertOwner(owner) as ObjectResult;
        var value = result!.Value as ErrorResponse;

        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        value.Should().NotBeNull();
        value!.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task UpsertOwner_Calls_OwnerRepository_UpsertOwner()
    {
        var owner = new Owner();
        
        await _ownerController.UpsertOwner(owner);
        
        _ownerRepository.Verify(x => x.UpsertOwner(owner), Times.Once);
    }
    
    #endregion
    
    #region GetOwnerById

    [Fact]
    public async Task GetOwnerById_Returns_OkResult_ContainingOwner_WhenIdIsValid()
    {
        var guid = _data.First().Key;
        
        var result = await _ownerController.GetOwnerById(guid) as ObjectResult;
        var value = result!.Value as Owner;

        result.StatusCode.Should().Be(StatusCodes.Status200OK);
        value.Should().NotBeNull();
        value!.Id.Should().Be(guid);
    }
    
    [Fact]
    public async Task GetOwnerById_Returns_OkResult_ContainingNull_WhenIdIsInvalid()
    {
        var guid = Guid.NewGuid();
        
        var result = await _ownerController.GetOwnerById(guid) as ObjectResult;
        var value = result!.Value;

        result.StatusCode.Should().Be(StatusCodes.Status200OK);
        value.Should().BeNull();
    }
    
    [Fact]
    public async Task GetOwnerById_Calls_OwnerRepository_GetOwnerById()
    {
        var guid = Guid.NewGuid();
        
        await _ownerController.GetOwnerById(guid);
        
        _ownerRepository.Verify(x => x.GetOwnerById(guid), Times.Once);
    }
    
    #endregion
    
    #region GetOwners
    
    [Fact]
    public async Task GetOwners_Returns_OkResult_ContainingIEnumerableOfOwner()
    {
        var result = await _ownerController.GetOwners() as ObjectResult;

        result!.StatusCode.Should().Be(StatusCodes.Status200OK);
        result.Value.Should().BeEquivalentTo(_data.Values);
    }
    
    [Fact]
    public async Task GetOwners_Calls_OwnerRepository_GetAllOwners_WhenNoCompanyIdProvided()
    {
        await _ownerController.GetOwners();
        
        _ownerRepository.Verify(x => x.GetAllOwners(), Times.Once);
        _ownerRepository.Verify(x => x.GetOwnersByCompany(It.IsAny<Guid>()), Times.Never);
    }
    
    [Fact]
    public async Task GetOwners_Calls_OwnerRepository_GetOwnersByCompany_WhenCompanyIdProvided()
    {
        var guid = Guid.NewGuid();
        
        await _ownerController.GetOwners(guid);
        
        _ownerRepository.Verify(x => x.GetOwnersByCompany(guid), Times.Once);
        _ownerRepository.Verify(x => x.GetAllOwners(), Times.Never);
    }
    
    #endregion
}