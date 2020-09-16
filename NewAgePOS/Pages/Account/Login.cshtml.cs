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

    public async Task OnGet()
    {
      EmployeeModel emp = _context.Employees.FirstOrDefault();

      if (emp == null)
      {
        string password = Secrets.SenderPassword;

        EmployeeModel employee = new EmployeeModel
        {
          FirstName = "Admin",
          LastName = "Admin",
          Email = Secrets.SenderEmail,
          UserName = Secrets.SenderEmail
        };

        await _userManager.CreateAsync(employee, password);

        Claim newClaim = new Claim(ClaimTypeEnum.Admin.ToString(), "true");
        await _userManager.AddClaimAsync(employee, newClaim);

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(employee);
        var tokenLink = Url.Page("Register", "ConfirmEmail", new { userId = employee.Id, token }, Request.Scheme);

        _emailSender.SendEmailConfirmationToken(employee.FullName, employee.Email, tokenLink);
      }
    }

    public async Task<IActionResult> OnGetConfirmEmail(string userId, string token)
    {
      EmployeeModel employee = await _userManager.FindByIdAsync(userId);

      await _userManager.ConfirmEmailAsync(employee, token);

      TempData["MessageTitle"] = "Email Confirmed";
      TempData["Message"] = "You may now login";

      return RedirectToPage("Login");
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
