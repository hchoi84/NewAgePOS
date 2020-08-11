using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using EmailSenderLibrary;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using NewAgePOS.Models;

namespace NewAgePOS.Pages.Account
{
  public class ForgotPasswordModel : PageModel
  {
    private readonly UserManager<EmployeeModel> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger _logger;

    public ForgotPasswordModel(UserManager<EmployeeModel> userManager, IEmailSender emailSender, ILogger<ForgotPasswordModel> logger)
    {
      _userManager = userManager;
      _emailSender = emailSender;
      _logger = logger;
    }

    [BindProperty]
    [Required]
    [Display(Name = "Email Address")]
    [DataType(DataType.EmailAddress)]
    public string EmailAddress { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPost()
    {
      if (!ModelState.IsValid) return Page();

      EmployeeModel employee = await _userManager.FindByEmailAsync(EmailAddress);

      if (employee == null)
      {
        ModelState.AddModelError(string.Empty, "Invalid email address. Please ensure the email is correct and already registered");

        return Page();
      }

      if (!employee.EmailConfirmed)
      {
        ModelState.AddModelError(string.Empty, "You have not confirmed your email yet");

        return Page();
      }

      string token = await _userManager.GeneratePasswordResetTokenAsync(employee);

      var tokenLink = Url.Page("ResetPassword", "", new { emailAddress = EmailAddress, token }, Request.Scheme);

      // TODO: Remove on production
      //_logger.LogInformation(tokenLink);

      try
      {
        _emailSender.SendPasswordResetToken(employee.FullName, employee.Email, tokenLink);
      }
      catch (Exception e)
      {
        _logger.LogError(e.Message);
        ModelState.AddModelError(string.Empty, "Something went wrong. Please contact the Admin");

        return Page();
      }

      TempData["MessageTitle"] = "Password Reset Email Sent";
      TempData["Message"] = "Please check your email for password reset link";

      return RedirectToPage("Login");
    }
  }
}
