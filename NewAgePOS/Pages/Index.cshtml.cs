using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace NewAgePOS.Pages
{
  public class IndexModel : PageModel
  {
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
      _logger = logger;
    }

    public IActionResult OnGet()
    {
      return Redirect("/Sale/Search");
      //return Redirect("/Product/Transfer/true?Codes=ANN0026");
    }
  }
}
