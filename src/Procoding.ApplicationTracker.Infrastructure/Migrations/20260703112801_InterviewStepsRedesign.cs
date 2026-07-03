using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Procoding.ApplicationTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InterviewStepsRedesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "InterviewSteps");

            migrationBuilder.DropColumn(
                name: "InteviewStepType",
                table: "InterviewSteps");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "InterviewSteps",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OccurredOn",
                table: "InterviewSteps",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Outcome",
                table: "InterviewSteps",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "InterviewSteps",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "InterviewSteps");

            migrationBuilder.DropColumn(
                name: "OccurredOn",
                table: "InterviewSteps");

            migrationBuilder.DropColumn(
                name: "Outcome",
                table: "InterviewSteps");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "InterviewSteps");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "InterviewSteps",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InteviewStepType",
                table: "InterviewSteps",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
