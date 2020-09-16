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
using NewAgePOS.Models;
using NewAgePOSModels.Securities;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace NewAgePOS.Pages.Account
{
  public class LoginModel : PageModel
  {
    private readonly SignInManager<EmployeeModel> _signInManager;
    private readonly UserManager<EmployeeModel> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly LogRegContext _context;

    public LoginModel(SignInManager<EmployeeModel> signInManager, UserManager<EmployeeModel> userManager, IEmailSender emailSender, LogRegContext context)
    {
      _signInManager = signInManager;
      _userManager = userManager;
      _emailSender = emailSender;
      _context = context;
    }

    [BindProperty]
    [Required]
    [Display(Name = "Email Address")]
    public string EmailAddress { get; set; }

    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [BindProperty]
    [Display(Name = "Remember Me")]
    public bool RememberMe { get; set; }

    [BindProperty(SupportsGet = true)]
    public string ReturnUrl { get; set; }

    public async Task<IActionResult> OnGet()
    {
      EmployeeModel employee = _context.Employees.FirstOrDefault();
      string errorMessage = "";

      if (employee == null)
      {
        (employee, errorMessage) = await RegisterUserAsync();

        if (!string.IsNullOrEmpty(errorMessage))
        {
          ModelState.AddModelError(string.Empty, errorMessage);
          return Page();
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(employee);

        var tokenLink = Url.Page("login", "confirmemail", new { userId = employee.Id, token }, Request.Scheme);

        //TODO: Remove on production
        //_logger.LogInformation(tokenLink);

        try
        {
          _emailSender.SendEmailConfirmationToken(employee.FullName, employee.Email, tokenLink);
        }
        catch (Exception e)
        {
          ModelState.AddModelError(string.Empty, "Something went wrong. Please contact the Admin");

          return Page();
        }

        TempData["MessageTitle"] = "Registration Success!";
        TempData["Message"] = "Please check your email for confirmation link. Allow up to 5 minutes for the email to arrive.";
      }

      return Page();
    }

    private async Task<(EmployeeModel, string)> RegisterUserAsync()
    {
      Claim newClaim;

      EmployeeModel employee = new EmployeeModel
      {
        FirstName = "Admin",
        LastName = "Admin",
        Email = Secrets.SenderEmail,
        UserName = Secrets.SenderEmail
      };

      IdentityResult identityResult = await _userManager.CreateAsync(employee, Secrets.SenderPassword);

      if (!identityResult.Succeeded)
      {
        List<string> errorMsg = new List<string>();

        foreach (var error in identityResult.Errors)
        {
          errorMsg.Add(error.Description);
        }

        return (null, string.Join("; ", errorMsg));
      }

      newClaim = new Claim(ClaimTypeEnum.Admin.ToString(), "true");

      identityResult = await _userManager.AddClaimAsync(employee, newClaim);

      if (!identityResult.Succeeded) return (null, "Failed to add Claim");

      return (employee, "");
    }

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

      return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAsync()
    {
      if (!ModelState.IsValid) return Page();

      SignInResult signInResult = await _signInManager.PasswordSignInAsync(EmailAddress, Password, RememberMe, false);

      if (signInResult.Succeeded)
      {
        if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
          return Redirect(ReturnUrl);
        else
          return RedirectToPage("/Sale/Search");
      }

      ModelState.AddModelError(string.Empty, "Invalid login attempt");
      return Page();
    }

    public async Task<IActionResult> OnPostLogOutAsync()
    {
      await _signInManager.SignOutAsync();

      return RedirectToPage("/Account/Login");
    }
  }
}
