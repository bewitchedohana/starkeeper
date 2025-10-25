using System.ComponentModel.DataAnnotations;

namespace IdentityService.ViewModels;

public class AuthorizeViewModel
{
    [Display(Name = "Application Name")]
    public string? ApplicationName { get; set; }

    [Display(Name = "Scope")]
    public string? Scope { get; set; }
}
