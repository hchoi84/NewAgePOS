using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EmailSenderLibrary;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using NewAgePOS.Models;
using NewAgePOS.ViewModels;
using NewAgePOSModels.Securities;

namespace NewAgePOS.Pages.Account
{

  public class RegisterModel : PageModel
  {
    private readonly UserManager<EmployeeModel> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(UserManager<EmployeeModel> userManager, IEmailSender emailSender, ILogger<RegisterModel> logger)
    {
      _userManager = userManager;
      _emailSender = emailSender;
      _logger = logger;
    }

    [BindProperty]
    public RegisterViewModel RegisterVM { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
      if (!ModelState.IsValid) return Page();

      (EmployeeModel employee, string errorMessage) = await RegisterUserAsync();

      if (employee == null)
      {
        ModelState.AddModelError(string.Empty, errorMessage);

        return Page();
      }

      var token = await _userManager.GenerateEmailConfirmationTokenAsync(employee);

      var tokenLink = Url.Page("Login", "ConfirmEmail", new { userId = employee.Id, token }, Request.Scheme);

      //TODO: Remove on production
      //_logger.LogInformation(tokenLink);

      try
      {
        _emailSender.SendEmailConfirmationToken(employee.FullName, employee.Email, tokenLink);
      }
      catch (Exception e)
      {
        _logger.LogError(e.Message);
        ModelState.AddModelError(string.Empty, "Something went wrong. Please contact the Admin");

        return Page();
      }

      TempData["MessageTitle"] = "Registration Success!";
      TempData["Message"] = "Please check your email for confirmation link. Allow up to 5 minutes for the email to arrive.";

      return RedirectToPage("Login");
    }

    public async Task<JsonResult> OnPostCheckEmail(string emailAddress)
    {
      string userEnteredDomain = emailAddress.Split('@')[1].ToLower();

      if (userEnteredDomain != Secrets.Domain)
        return new JsonResult($"Only { Secrets.Domain } email addresses are allowed");

      EmployeeModel employee = await _userManager.FindByEmailAsync(emailAddress);

      if (employee != null)
        return new JsonResult("Email address already in use");

      return new JsonResult(true);
    }

    private async Task<(EmployeeModel, string)> RegisterUserAsync()
    {
      Claim newClaim;

      EmployeeModel employee = new EmployeeModel
      {
        FirstName = RegisterVM.FirstName,
        LastName = RegisterVM.LastName,
        Email = RegisterVM.EmailAddress,
        UserName = RegisterVM.EmailAddress,
      };

      bool isFirstUser = !_userManager.Users.Any();

      IdentityResult identityResult = await _userManager.CreateAsync(employee, RegisterVM.Password);

      if (!identityResult.Succeeded)
      {
        List<string> errorMsg = new List<string>();

        foreach (var error in identityResult.Errors)
        {
          errorMsg.Add(error.Description);
        }

        return (null, string.Join("; ", errorMsg));
      }

      if (isFirstUser)
        newClaim = new Claim(ClaimTypeEnum.Manager.ToString(), "true");
      else
        newClaim = new Claim(ClaimTypeEnum.Employee.ToString(), "true");

      identityResult = await _userManager.AddClaimAsync(employee, newClaim);

      if (!identityResult.Succeeded) return (null, "Failed to add Claim");

      return (employee, "Success");
    }
  }
}
