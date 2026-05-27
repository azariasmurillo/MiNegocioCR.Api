using MiNegocioCR.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiNegocioCR.Api.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260526120000_RemoveRepairOrderDiscountPercent")]
    partial class RemoveRepairOrderDiscountPercent
    {
    }
}
