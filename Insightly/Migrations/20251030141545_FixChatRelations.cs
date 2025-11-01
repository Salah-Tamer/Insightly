using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Insightly.Migrations
{
    /// <inheritdoc />
    public partial class FixChatRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_AspNetUsers_OtherUserId1",
                table: "Chats");

            migrationBuilder.DropForeignKey(
                name: "FK_Chats_AspNetUsers_UserId1",
                table: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_Chats_OtherUserId1",
                table: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_Chats_UserId1",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "OtherUserId1",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Chats");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OtherUserId1",
                table: "Chats",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Chats",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_OtherUserId1",
                table: "Chats",
                column: "OtherUserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_UserId1",
                table: "Chats",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_AspNetUsers_OtherUserId1",
                table: "Chats",
                column: "OtherUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_AspNetUsers_UserId1",
                table: "Chats",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
