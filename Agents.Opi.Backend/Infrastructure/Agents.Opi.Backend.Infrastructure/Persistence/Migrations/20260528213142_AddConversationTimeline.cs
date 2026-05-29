using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Agents.Opi.Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConversationTimeline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "conversation_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conversation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agent_type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    phase = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    role = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversation_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_conversation_messages_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_conversation_messages_conversation_id_created_at",
                table: "conversation_messages",
                columns: new[] { "conversation_id", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "conversation_messages");
        }
    }
}
