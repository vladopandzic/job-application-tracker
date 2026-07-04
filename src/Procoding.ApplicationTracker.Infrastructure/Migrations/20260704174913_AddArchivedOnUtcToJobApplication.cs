using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Procoding.ApplicationTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddArchivedOnUtcToJobApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedOnUtc",
                table: "JobApplications",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchivedOnUtc",
                table: "JobApplications");
        }
    }
}
