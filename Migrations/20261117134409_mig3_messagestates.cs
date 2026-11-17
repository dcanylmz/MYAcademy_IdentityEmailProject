using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MYAcademy_IdentityEmailProject.Migrations
{
    /// <inheritdoc />
    public partial class mig3_messagestates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Categories_CategoryID",
                table: "Messages");

            migrationBuilder.AlterColumn<string>(
                name: "SenderEmail",
                table: "Messages",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ReceiverEmail",
                table: "Messages",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "MessageSubject",
                table: "Messages",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "CategoryID",
                table: "Messages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeletedByReceiver",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeletedBySender",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsExternal",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPermanentlyDeletedByReceiver",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPermanentlyDeletedBySender",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsStarredByReceiver",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsStarredBySender",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ParentMessageID",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CategoryName",
                table: "Categories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "CategoryColor",
                table: "Categories",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "#8B7FD6");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReceiverEmail",
                table: "Messages",
                column: "ReceiverEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderEmail",
                table: "Messages",
                column: "SenderEmail");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Categories_CategoryID",
                table: "Messages",
                column: "CategoryID",
                principalTable: "Categories",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Categories_CategoryID",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_ReceiverEmail",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_SenderEmail",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsDeletedByReceiver",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsDeletedBySender",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsExternal",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsPermanentlyDeletedByReceiver",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsPermanentlyDeletedBySender",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsStarredByReceiver",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsStarredBySender",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "ParentMessageID",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "CategoryColor",
                table: "Categories");

            migrationBuilder.AlterColumn<string>(
                name: "SenderEmail",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "ReceiverEmail",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "MessageSubject",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<int>(
                name: "CategoryID",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CategoryName",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Categories_CategoryID",
                table: "Messages",
                column: "CategoryID",
                principalTable: "Categories",
                principalColumn: "CategoryID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
