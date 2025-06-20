using System;
using Microsoft.EntityFrameworkCore;
using WsRaisedHandsModern.Api.Data.CMSReporting.Entities;

namespace WsRaisedHandsModern.Api.Data.CMSReporting
{
    public class CmsReportingDbContext : DbContext
    {
        public CmsReportingDbContext(DbContextOptions<CmsReportingDbContext> options)
            : base(options) { }
    
        // public DbSet<RaisedHandsSalvation> RaisedHandsSalvation { get; set; }
        public DbSet<tblFormSalvation> TblFormSalvation { get; set; } // <-- Add this line
    
        // Add other tables as needed
    
        public override int SaveChanges() =>
            throw new InvalidOperationException("Read-only context: SaveChanges is disabled.");

    }
}