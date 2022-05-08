using Velocity.API.Exceptions;
using Velocity.API.Models;

namespace Velocity.API.Repositories;

public interface IOwnerRepository
{
    Task<Owner> UpsertOwner(Owner owner);
    Task<Owner> GetOwnerById(Guid id);
    Task<IEnumerable<Owner>> GetAllOwners();
    Task<IEnumerable<Owner>> GetOwnersByCompany(Guid companyId);
}

public class OwnerRepository : IOwnerRepository
{
    private static readonly IDictionary<Guid, Owner> Data = new Dictionary<Guid, Owner>();

    public async Task<Owner> UpsertOwner(Owner owner)
    {
        var companyOwners = Data.Values.Where(x => x.CompanyId == owner.CompanyId).ToArray();
        
        if (Data.ContainsKey(owner.Id))
        {
            var existingOwner = companyOwners.First(x => x.Id == owner.Id);

            if (companyOwners.Sum(x => x.Percentage) + owner.Percentage - existingOwner.Percentage > 100)
                throw new PercentageOverflowException();

            Data[owner.Id].Name = owner.Name;
            Data[owner.Id].Percentage = owner.Percentage;
        }
        else
        {
            if (companyOwners.Sum(x => x.Percentage) + owner.Percentage > 100)
                throw new PercentageOverflowException();
            
            Data.Add(owner.Id, owner);
        }

        return await Task.FromResult(owner);
    }

    public async Task<Owner> GetOwnerById(Guid id)
    {
        return await Task.FromResult(Data.ContainsKey(id) ? Data[id] : null);
    }
    
    public async Task<IEnumerable<Owner>> GetAllOwners()
    {
        return await Task.FromResult(Data.Values);
    }
    
    public async Task<IEnumerable<Owner>> GetOwnersByCompany(Guid companyId)
    {
        return await Task.FromResult(Data.Values.Where(x => x.CompanyId == companyId));
    }
}