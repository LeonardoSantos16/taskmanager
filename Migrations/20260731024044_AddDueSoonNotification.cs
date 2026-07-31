using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace taskmanager.Migrations
{
    /// <inheritdoc />
    public partial class AddDueSoonNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueSoonNotifiedAt",
                table: "TaskItem",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notification_TaskItem_TaskItemId",
                        column: x => x.TaskItemId,
                        principalTable: "TaskItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notification_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notification_TaskItemId",
                table: "Notification",
                column: "TaskItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserId",
                table: "Notification",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropColumn(
                name: "DueSoonNotifiedAt",
                table: "TaskItem");
        }
    }
}
