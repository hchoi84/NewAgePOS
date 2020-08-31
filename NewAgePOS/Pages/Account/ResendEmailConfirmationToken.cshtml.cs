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
  public class ResendEmailConfirmationTokenModel : PageModel
  {
    private readonly UserManager<EmployeeModel> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ResendEmailConfirmationTokenModel> _logger;

    public ResendEmailConfirmationTokenModel(UserManager<EmployeeModel> userManager, IEmailSender emailSender, ILogger<ResendEmailConfirmationTokenModel> logger)
    {
      _userManager = userManager;
      _emailSender = emailSender;
      _logger = logger;
    }

    [BindProperty]
    [Display(Name = "Email Address")]
    [DataType(DataType.EmailAddress)]
    public string EmailAddress { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
      EmployeeModel employee = await _userManager.FindByEmailAsync(EmailAddress);

      if (employee == null)
      {
        TempData["MessageTitle"] = "Email sent";
        TempData["Message"] = "Email confirmation token has been sent";

        return RedirectToPage("Login");
      }

      string token = await _userManager.GenerateEmailConfirmationTokenAsync(employee);

      string tokenLink = Url.Page("Register", "ConfirmEmail", new { userId = employee.Id, token }, Request.Scheme);

      try
      {
        _emailSender.ResendEmailConfirmationToken(employee.FullName, employee.Email, tokenLink);
      }
      catch (Exception e)
      {
        _logger.LogError(e.Message);
        ModelState.AddModelError(string.Empty, "Something went wrong. Please contact the Admin");

        return Page();
      }

      return RedirectToPage("Login");
    }
  }
}
