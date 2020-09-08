using System.ComponentModel.DataAnnotations;

namespace NewAgePOS.ViewModels.Sale
{
  public class GuestViewModel
  {
    public int Id { get; set; }

    [Required]
    [Display(Name = "First Name")]
    [MinLength(2, ErrorMessage = "Min {1} Characters")]
    public string FirstName { get; set; }

    [Required]
    [Display(Name = "Last Name")]
    [MinLength(2, ErrorMessage = "Min {1} Characters")]
    public string LastName { get; set; }

    [Required]
    [Display(Name = "Email Address")]
    [DataType(DataType.EmailAddress)]
    public string EmailAddress { get; set; }

    [Required]
    [Display(Name = "Phone Number")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "10 digit numbers only")]
    [RegularExpression(@"\d{10}")]
    public string PhoneNumber { get; set; }
  }
}
