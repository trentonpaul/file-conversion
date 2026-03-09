using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileConversion.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateConversionJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileId",
                table: "Jobs");

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "Jobs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UploadFilePath",
                table: "Jobs",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "UploadFilePath",
                table: "Jobs");

            migrationBuilder.AddColumn<Guid>(
                name: "FileId",
                table: "Jobs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
