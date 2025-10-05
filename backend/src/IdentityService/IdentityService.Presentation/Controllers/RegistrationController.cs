using System.Text.Json;
using FluentResults;
using IdentityService.Application.Features.Users.CreateUserCommand;
using IdentityService.Presentation.ViewModels;
using IdentityService.Presentation.ViewModels.Abstractions;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Presentation.Controllers
{
    [Route("[controller]")]
    public class RegistrationController(IMediator mediator) : Controller
    {
        private readonly IMediator _mediator = mediator;

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost()]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(RegistrationViewModel viewModel)
        {
            CreateUserCommand command = new(viewModel.Email,
                viewModel.Password);

            var result = await _mediator.Send(command);
            if (result.IsFailed)
            {
                viewModel.Errors.AddRange(result.Errors.Select(e => e.Message).Distinct());
                TempData[nameof(ViewModelBase.Errors)] = JsonSerializer.Serialize(viewModel.Errors);
                return RedirectToAction(nameof(Index));
            }

            // TODO: Redirect to user page
            return RedirectToAction(nameof(Index));
        }
    }
}
