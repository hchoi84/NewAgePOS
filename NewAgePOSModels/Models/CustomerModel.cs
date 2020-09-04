using System.Globalization;

namespace NewAgePOSModels.Models
{
  public class CustomerModel
  {
    TextInfo ti = new CultureInfo("en-US", false).TextInfo;

    private string _firstName;
    private string _lastName;

    public int Id { get; set; }
    public string FirstName {
      get { return ti.ToTitleCase(_firstName); }
      set { _firstName = value; }
    }
    public string LastName {
      get { return ti.ToTitleCase(_lastName); }
      set { _lastName = value; } 
    }
    public string EmailAddress { get; set; }
    public string PhoneNumber { get; set; }
    public string FullName { 
      get { return $"{ FirstName } { LastName }"; } 
    }
  }
}
