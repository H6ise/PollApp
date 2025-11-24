using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PollApp.Migrations
{
    /// <inheritdoc />
    public partial class addPollCreaterUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VotedAt",
                table: "Votes",
                newName: "VoteDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "Polls",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastViewed",
                table: "Polls",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Polls",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Polls");

            migrationBuilder.DropColumn(
                name: "LastViewed",
                table: "Polls");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Polls");

            migrationBuilder.RenameColumn(
                name: "VoteDate",
                table: "Votes",
                newName: "VotedAt");
        }
    }
}
