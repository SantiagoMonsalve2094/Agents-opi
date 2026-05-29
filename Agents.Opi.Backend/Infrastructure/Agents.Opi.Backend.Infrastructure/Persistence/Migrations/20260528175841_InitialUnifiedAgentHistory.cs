using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Agents.Opi.Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialUnifiedAgentHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agent_users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    display_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_seen_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agent_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "conversations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agent_type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    initial_input = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversations", x => x.id);
                    table.ForeignKey(
                        name: "FK_conversations_agent_users_user_id",
                        column: x => x.user_id,
                        principalTable: "agent_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "conversation_phase_outputs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    conversation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agent_type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    phase = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    input = table.Column<string>(type: "text", nullable: false),
                    previous_output = table.Column<string>(type: "text", nullable: false),
                    output = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversation_phase_outputs", x => x.id);
                    table.ForeignKey(
                        name: "FK_conversation_phase_outputs_conversations_conversation_id",
                        column: x => x.conversation_id,
                        principalTable: "conversations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_agent_users_email",
                table: "agent_users",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "IX_agent_users_external_id",
                table: "agent_users",
                column: "external_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_conversation_phase_outputs_conversation_id_phase",
                table: "conversation_phase_outputs",
                columns: new[] { "conversation_id", "phase" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_conversations_user_id_agent_type_updated_at",
                table: "conversations",
                columns: new[] { "user_id", "agent_type", "updated_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "conversation_phase_outputs");

            migrationBuilder.DropTable(
                name: "conversations");

            migrationBuilder.DropTable(
                name: "agent_users");
        }
    }
}
