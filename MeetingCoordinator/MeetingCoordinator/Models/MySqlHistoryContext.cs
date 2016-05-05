using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Migrations.History;
using System.Linq;
using System.Web;

namespace MeetingCoordinator.Models
{
  public class MySqlHistoryContext : HistoryContext
  {
    public MySqlHistoryContext(
      DbConnection existingConnection,
      string defaultSchema)
    : base(existingConnection, defaultSchema)
    {
    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
      modelBuilder.Properties<String>().Configure(c => c.HasColumnType("longtext"));
      base.OnModelCreating(modelBuilder);
    }
  }
}