using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace NewAgePOS.Pages.Sale
{
  public class MessageModel : PageModel
  {
    [BindProperty(SupportsGet = true)]
    public int SaleId { get; set; }
    public void OnGet()
    {
    }
  }
}
