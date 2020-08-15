using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EmailSenderLibrary;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NewAgePOS.Models;
using NewAgePOS.Securities;

namespace NewAgePOS.Pages.Account
{
  public class RegisterModel : PageModel
  {
    private readonly UserManager<EmployeeModel> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<RegisterModel> _logger;

    public RegisterModel(UserManager<EmployeeModel> userManager, IEmailSender emailSender, ILogger<RegisterModel> logger, LogRegContext context)
    {
      _userManager = userManager;
      _emailSender = emailSender;
      _logger = logger;
    }

    [BindProperty]
    [Required]
    [Display(Name = "First Name")]
    [StringLength(30, MinimumLength = 2, ErrorMessage = "{0} must be between {2} to {1} characters")]
    [MaxLength(30)]
    public string FirstName { get; set; }

    [BindProperty]
    [Required]
    [Display(Name = "Last Name")]
    [StringLength(30, MinimumLength = 2, ErrorMessage = "{0} must be between {2} to {1} characters")]
    [MaxLength(30)]
    public string LastName { get; set; }

    [BindProperty]
    [Required]
    [Display(Name = "Email Address")]
    [DataType(DataType.EmailAddress)]
    [PageRemote(
      ErrorMessage = "Uh oh! Something's wrong!",
      AdditionalFields = "__RequestVerificationToken",
      HttpMethod = "post",
      PageHandler = "CheckEmail"
      )]
    public string EmailAddress { get; set; }

    [BindProperty]
    [Required]
    [Display(Name = "Password")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "{0} must be at least {1} characters long and contain lower, upper, digit, and special character")]
    public string Password { get; set; }

    [Required]
    [Display(Name = "Confirm Password")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Does not match with password")]
    public string ConfirmPassword { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnGetConfirmEmail(string userId, string token)
    {
      if (userId == null || token == null)
      {
        TempData["MessageTitle"] = "Error";
        TempData["Message"] = "The email confirmation token link is invalid";

        return RedirectToAction("Login");
      }

      EmployeeModel employee = await _userManager.FindByIdAsync(userId);

      if (employee == null)
      {
        TempData["MessageTitle"] = "Error";
        TempData["Message"] = "No user found";

        return RedirectToAction("Login");
      }

      IdentityResult result = await _userManager.ConfirmEmailAsync(employee, token);

      if (!result.Succeeded)
      {
        TempData["MessageTitle"] = "Error";
        TempData["Message"] = "Something went wrong while confirming";

        return RedirectToPage("Login");
      }

      TempData["MessageTitle"] = "Email Confirmed";
      TempData["Message"] = "You may now login";

      return RedirectToPage("Login");
    }

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

      var tokenLink = Url.Page("Register", "ConfirmEmail", new { userId = employee.Id, token }, Request.Scheme);

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
      string validDomain = Secrets.ValidDomain;
      string userEnteredDomain = emailAddress.Split('@')[1].ToLower();

      if (userEnteredDomain != validDomain)
        return new JsonResult($"Only { validDomain } email addresses are allowed");

      EmployeeModel employee = await _userManager.FindByEmailAsync(emailAddress);

      if (employee != null)
        return new JsonResult("Email address already in use");

      return new JsonResult(true);
    }

    public async Task<(EmployeeModel, string)> RegisterUserAsync()
    {
      Claim newClaim;

      EmployeeModel employee = new EmployeeModel
      {
        FirstName = FirstName,
        LastName = LastName,
        Email = EmailAddress,
        UserName = EmailAddress,
      };

      bool isFirstUser = !_userManager.Users.Any();

      IdentityResult identityResult = await _userManager.CreateAsync(employee, Password);

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
