namespace IdentityService.Presentation.ViewModels.Abstractions;

public class ViewModelBase
{
    public List<string> Errors { get; } = [];

    public ViewModelBase() { }
}
