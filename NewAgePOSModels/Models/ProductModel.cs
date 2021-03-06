﻿using System;

namespace NewAgePOSModels.Models
{
  public class ProductModel
  {
    public int Id { get; set; }
    public string Sku { get; set; }
    public string Upc { get; set; }
    public string Mpn { get; set; }
    public float Cost { get; set; }
    public float Price { get; set; }
    public string AllName { get; set; }
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
  }
}
