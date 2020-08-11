using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOS.Models;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace NewAgePOS.Pages.Account
{
  public class LoginModel : PageModel
  {
    private readonly SignInManager<EmployeeModel> _signInManager;

    public LoginModel(SignInManager<EmployeeModel> signInManager)
    {
      _signInManager = signInManager;
    }

    [Required]
    [Display(Name = "Email Address")]
    public string EmailAddress { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [Display(Name = "Remember Me")]
    public bool RememberMe { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl)
    {
      if (!ModelState.IsValid) return Page();

      SignInResult signInResult = await _signInManager.PasswordSignInAsync(EmailAddress, Password, RememberMe, false);

      if (signInResult.Succeeded)
      {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
          return RedirectToPage(returnUrl);
        else
          return RedirectToPage("Sale");
      }

      ModelState.AddModelError(string.Empty, "Invalid login attempt");
      return Page();
    }
  }
}
