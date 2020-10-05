using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NewAgePOSLibrary.Data;
using NewAgePOSModels.Models;

namespace NewAgePOS.Pages.Transfer
{
  public class SearchModel : PageModel
  {
    private readonly ISQLData _sqlDb;

    public SearchModel(ISQLData sqlDb)
    {
      _sqlDb = sqlDb;
    }

    [BindProperty(SupportsGet = true)]
    public int? SearchStatusId { get; set; } = 99;

    public List<SelectListItem> SearchStatuses { get; set; } = new List<SelectListItem>
    {
      new SelectListItem { Text = StatusEnum.Pending.ToString(), Value = StatusEnum.Pending.GetTypeCode().ToString() },
      new SelectListItem { Text = StatusEnum.Ready.ToString(), Value = StatusEnum.Ready.GetTypeCode().ToString() },
      new SelectListItem { Text = StatusEnum.Picking.ToString(), Value = StatusEnum.Picking.GetTypeCode().ToString() },
      new SelectListItem { Text = StatusEnum.Complete.ToString(), Value = StatusEnum.Complete.GetTypeCode().ToString() },
    };

    public IEnumerable<TransferRequestModel> TransferRequests { get; set; }

    public IActionResult OnGet()
    {
      if (SearchStatusId == null) 
      {
        TempData["Message"] = "Please select a status";
        return Page(); 
      }

      TransferRequests = _sqlDb.TransferRequests_GetByStatus((StatusEnum)SearchStatusId.Value);
      foreach (var tr in TransferRequests)
      {
        int itemsCount = _sqlDb.TransferRequestItems_GetByTransferRequestId(tr.Id).Count();
        tr.Description += $" ({itemsCount} Items)";
      }

      return Page();
    }

    public IActionResult OnPostDeleteTransferRequest(int id, string description)
    {
      _sqlDb.TransferRequests_Delete(id);

      TempData["Message"] = $"{ description } has been deleted";
      return RedirectToPage();
    }

    public IActionResult OnPostUpdateStatus(int id, string description, StatusEnum currentStatus)
    {
      List<string> messages = new List<string>();
      var transferRequest = _sqlDb.TransferRequests_GetById(id);

      if (transferRequest.Status == currentStatus)
      {
        int currentStatusCode = (int)transferRequest.Status;
        int newStatusCode = ++currentStatusCode;
        transferRequest.Status = (StatusEnum)newStatusCode;
        _sqlDb.TransferRequests_Update(transferRequest);
        messages.Add($"{ description } status has been updated to { transferRequest.Status }");
      }
      else
      {
        messages.Add($"{ description } status has not been updated");
        messages.Add($"Current status code { currentStatus } is different from DB { transferRequest.Status }. Someone could have already updated it");
      }
        TempData["Message"] = string.Join(Environment.NewLine, messages);
        return RedirectToPage();
    }
  }
}
