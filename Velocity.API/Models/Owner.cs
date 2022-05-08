using System.ComponentModel.DataAnnotations;

namespace Velocity.API.Models;

public class Owner
{
    public Guid Id { get; set; }
    
    public Guid CompanyId { get; set; }
    
    [Required(AllowEmptyStrings = false)]
    public string Name { get; set; }
    
    [Range(0, 100)]
    public double Percentage { get; set; }
}