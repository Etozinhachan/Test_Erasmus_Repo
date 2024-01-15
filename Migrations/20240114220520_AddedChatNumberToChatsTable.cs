using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace testingStuff.Migrations
{
    /// <inheritdoc />
    public partial class AddedChatNumberToChatsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "chat_number",
                table: "Chats",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "chat_number",
                table: "Chats");
        }
    }
}
