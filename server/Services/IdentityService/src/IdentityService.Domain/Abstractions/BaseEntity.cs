using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityService.Domain.Abstractions;

public class BaseEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; private set; } = Guid.NewGuid();

    public DateTime CreatedOn { get; protected set; } = DateTime.UtcNow;

    public DateTime? ModifiedOn { get; set; } = DateTime.UtcNow;

    protected BaseEntity()
    {
        
    }
}
