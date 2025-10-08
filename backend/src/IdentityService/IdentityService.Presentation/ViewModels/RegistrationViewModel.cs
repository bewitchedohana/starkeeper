using IdentityService.Presentation.ViewModels.Abstractions;

namespace IdentityService.Presentation.ViewModels;

public class RegistrationViewModel : ViewModelBase
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
