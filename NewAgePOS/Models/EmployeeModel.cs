using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace NewAgePOS.Models
{
  public class EmployeeModel : IdentityUser
  {
    [Required]
    [Display(Name = "First Name")]
    [StringLength(30, MinimumLength = 2, ErrorMessage = "{0} must be between {2} to {1} characters")]
    [MaxLength(30)]
    public string FirstName { get; set; }

    [Required]
    [Display(Name = "Last Name")]
    [StringLength(30, MinimumLength = 2, ErrorMessage = "{0} must be between {2} to {1} characters")]
    [MaxLength(30)]
    public string LastName { get; set; }

    public DateTime StartDate { get; set; } = DateTime.Now;
    public DateTime EndDate { get; set; } = DateTime.Now;

    public string FullName {
      get { return $"{ FirstName } { LastName }"; }
    }
  }
}
