using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NewAgePOSLibrary.Models
{
  public class CustomerModel
  {
    public int Id { get; set; }

    [Required]
    [DisplayName("First Name")]
    [MinLength(2, ErrorMessage = "Min {1} Characters")]
    public string FirstName { get; set; }

    [Required]
    [DisplayName("Last Name")]
    [MinLength(2, ErrorMessage = "Min {1} Characters")]
    public string LastName { get; set; }

    [Required]
    [DisplayName("Email Address")]
    [DataType(DataType.EmailAddress)]
    public string EmailAddress { get; set; }

    [Required]
    [DisplayName("Phone Number")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "10 digit numbers only")]
    public string PhoneNumber { get; set; }
  }
}
