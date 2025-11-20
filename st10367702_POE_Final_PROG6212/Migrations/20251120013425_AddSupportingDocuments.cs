using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace st10367702_POE_Final_PROG6212.Migrations
{
    /// <inheritdoc />
    public partial class AddSupportingDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UploadedAt",
                table: "SupportingDocuments",
                newName: "UploadedOn");

            migrationBuilder.RenameColumn(
                name: "StoredFileName",
                table: "SupportingDocuments",
                newName: "FilePath");

            migrationBuilder.RenameColumn(
                name: "OriginalFileName",
                table: "SupportingDocuments",
                newName: "FileName");

            migrationBuilder.RenameColumn(
                name: "FileType",
                table: "SupportingDocuments",
                newName: "ContentType");

            migrationBuilder.CreateIndex(
                name: "IX_SupportingDocuments_ClaimId",
                table: "SupportingDocuments",
                column: "ClaimId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportingDocuments_Claims_ClaimId",
                table: "SupportingDocuments",
                column: "ClaimId",
                principalTable: "Claims",
                principalColumn: "ClaimId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupportingDocuments_Claims_ClaimId",
                table: "SupportingDocuments");

            migrationBuilder.DropIndex(
                name: "IX_SupportingDocuments_ClaimId",
                table: "SupportingDocuments");

            migrationBuilder.RenameColumn(
                name: "UploadedOn",
                table: "SupportingDocuments",
                newName: "UploadedAt");

            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "SupportingDocuments",
                newName: "StoredFileName");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "SupportingDocuments",
                newName: "OriginalFileName");

            migrationBuilder.RenameColumn(
                name: "ContentType",
                table: "SupportingDocuments",
                newName: "FileType");
        }
    }
}
