﻿using System.ComponentModel.DataAnnotations;

namespace NewAgePOSModels.Models
{
  public class CartDiscModel
  {
    public int SaleLineId { get; set; }

    [Display(Name = "Disc %")]
    [Range(0f, float.MaxValue)]
    public float DiscPct { get; set; }
  }
}