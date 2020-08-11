using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewAgePOS.Models;

namespace NewAgePOS.Pages.Account
{
  public class ResetPasswordModel : PageModel
  {
    private readonly UserManager<EmployeeModel> _userManager;

    public ResetPasswordModel(UserManager<EmployeeModel> userManager)
    {
      _userManager = userManager;
    }

    [BindProperty(SupportsGet = true)]
    public string EmailAddress { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public string Token { get; set; }

    [BindProperty]
    [Required]
    [Display(Name = "Password")]
    [DataType(DataType.Password)]
    [MinLength(6, ErrorMessage = "{0} must be at least {1} characters long and contain lower, upper, digit, and non-alphaneumeric")]
    public string Password { get; set; }

    [Required]
    [Display(Name = "Confirm Password")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Does not match with Password")]
    public string ConfirmPassword { get; set; }

    public async Task<IActionResult> OnGet()
    {
      if (Token == null || EmailAddress == null)
      {
        TempData["MessageTitle"] = "Invalid Link";
        TempData["Message"] = "The link to reset your password is invalid.";

        return RedirectToPage("Login");
      }

      EmployeeModel employee = await _userManager.FindByEmailAsync(EmailAddress);

      bool isValidToken = await _userManager.VerifyUserTokenAsync(employee, TokenOptions.DefaultProvider, "ResetPassword", Token);

      if (!isValidToken)
      {
        TempData["MessageTitle"] = "Invalid Token";
        TempData["Message"] = "The token you provided is invalid";

        return RedirectToPage("Login");
      }

      return Page();
    }

    public async Task<IActionResult> OnPost()
    {
      if (!ModelState.IsValid) return Page();

      EmployeeModel employee = await _userManager.FindByEmailAsync(EmailAddress);

      IdentityResult identityResult = await _userManager.ResetPasswordAsync(employee, Token, Password);

      if (!identityResult.Succeeded)
      {
        TempData["MessageTitle"] = "Error";
        TempData["Message"] = "Something went wrong and the password wasn't able to get reset. Please contact the Admin";
      }
      else
      {
        TempData["MessageTitle"] = "Success";
        TempData["Message"] = "Password has been reset sucessfully";
      }

      return RedirectToPage("Login");
    }
  }
}
