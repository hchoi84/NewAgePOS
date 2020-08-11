using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata;

namespace NewAgePOS.Models
{
  public class LogRegContext : IdentityDbContext
  {
    public LogRegContext(DbContextOptions<LogRegContext> options)
      : base(options)
    {
    }

    public DbSet<EmployeeModel> Employees { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);

      IEnumerable<IMutableForeignKey> foreignKeys = 
        builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys());

      foreach (var fk in foreignKeys)
      {
        fk.DeleteBehavior = DeleteBehavior.Restrict;
      }
    }
  }
}
