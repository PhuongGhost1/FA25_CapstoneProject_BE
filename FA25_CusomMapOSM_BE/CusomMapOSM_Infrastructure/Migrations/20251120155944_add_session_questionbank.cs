using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CusomMapOSM_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class add_session_questionbank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "question_banks",
                columns: table => new
                {
                    question_bank_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    workspace_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    map_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    bank_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tags = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    total_questions = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    is_template = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    is_public = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_question_banks", x => x.question_bank_id);
                    table.ForeignKey(
                        name: "FK_question_banks_maps_map_id",
                        column: x => x.map_id,
                        principalTable: "maps",
                        principalColumn: "map_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_question_banks_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_question_banks_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "workspace_id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "questions",
                columns: table => new
                {
                    question_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    question_bank_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    location_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    question_type = table.Column<int>(type: "int", nullable: false),
                    question_text = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    question_image_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    question_audio_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    points = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    time_limit = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    correct_answer_text = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    correct_latitude = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    correct_longitude = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    acceptance_radius_meters = table.Column<int>(type: "int", nullable: true),
                    hint_text = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    explanation = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    display_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_questions", x => x.question_id);
                    table.ForeignKey(
                        name: "FK_questions_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "locations",
                        principalColumn: "location_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_questions_question_banks_question_bank_id",
                        column: x => x.question_bank_id,
                        principalTable: "question_banks",
                        principalColumn: "question_bank_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    session_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    map_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    question_bank_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    host_user_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    session_code = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    session_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    session_type = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    max_participants = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    allow_late_join = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    show_leaderboard = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    show_correct_answers = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    shuffle_questions = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    shuffle_options = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    enable_hints = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    points_for_speed = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    scheduled_start_time = table.Column<DateTime>(type: "datetime", nullable: true),
                    actual_start_time = table.Column<DateTime>(type: "datetime", nullable: true),
                    end_time = table.Column<DateTime>(type: "datetime", nullable: true),
                    total_participants = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    total_responses = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.session_id);
                    table.ForeignKey(
                        name: "FK_sessions_maps_map_id",
                        column: x => x.map_id,
                        principalTable: "maps",
                        principalColumn: "map_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sessions_question_banks_question_bank_id",
                        column: x => x.question_bank_id,
                        principalTable: "question_banks",
                        principalColumn: "question_bank_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sessions_users_host_user_id",
                        column: x => x.host_user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "question_options",
                columns: table => new
                {
                    question_option_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    question_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    option_text = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    option_image_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_correct = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    display_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_question_options", x => x.question_option_id);
                    table.ForeignKey(
                        name: "FK_question_options_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "question_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "session_participants",
                columns: table => new
                {
                    session_participant_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    session_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    display_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_guest = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    joined_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    left_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    total_score = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    total_correct = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    total_answered = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    average_response_time = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValue: 0m),
                    rank = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    device_info = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ip_address = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_participants", x => x.session_participant_id);
                    table.ForeignKey(
                        name: "FK_session_participants_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "sessions",
                        principalColumn: "session_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_session_participants_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "session_questions",
                columns: table => new
                {
                    session_question_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    session_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    question_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    queue_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    points_override = table.Column<int>(type: "int", nullable: true),
                    time_limit_override = table.Column<int>(type: "int", nullable: true),
                    time_limit_extensions = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    started_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    ended_at = table.Column<DateTime>(type: "datetime", nullable: true),
                    total_responses = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    correct_responses = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_session_questions", x => x.session_question_id);
                    table.ForeignKey(
                        name: "FK_session_questions_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "questions",
                        principalColumn: "question_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_session_questions_sessions_session_id",
                        column: x => x.session_id,
                        principalTable: "sessions",
                        principalColumn: "session_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "student_responses",
                columns: table => new
                {
                    student_response_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    session_question_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    session_participant_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    question_option_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    response_text = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    response_latitude = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    response_longitude = table.Column<decimal>(type: "decimal(10,7)", nullable: true),
                    is_correct = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    points_earned = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    response_time_seconds = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValue: 0m),
                    used_hint = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    distance_error_meters = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    submitted_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_responses", x => x.student_response_id);
                    table.ForeignKey(
                        name: "FK_student_responses_question_options_question_option_id",
                        column: x => x.question_option_id,
                        principalTable: "question_options",
                        principalColumn: "question_option_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_student_responses_session_participants_session_participant_id",
                        column: x => x.session_participant_id,
                        principalTable: "session_participants",
                        principalColumn: "session_participant_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_student_responses_session_questions_session_question_id",
                        column: x => x.session_question_id,
                        principalTable: "session_questions",
                        principalColumn: "session_question_id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_question_banks_map_id",
                table: "question_banks",
                column: "map_id");

            migrationBuilder.CreateIndex(
                name: "IX_question_banks_user_id",
                table: "question_banks",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_question_banks_workspace_id",
                table: "question_banks",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "IX_question_options_question_id",
                table: "question_options",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_questions_location_id",
                table: "questions",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "IX_questions_question_bank_id",
                table: "questions",
                column: "question_bank_id");

            migrationBuilder.CreateIndex(
                name: "IX_SessionParticipant_IsActive",
                table: "session_participants",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_SessionParticipant_SessionId",
                table: "session_participants",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_SessionParticipant_SessionId_TotalScore",
                table: "session_participants",
                columns: new[] { "session_id", "total_score" });

            migrationBuilder.CreateIndex(
                name: "IX_SessionParticipant_UserId",
                table: "session_participants",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UX_SessionParticipant_SessionId_UserId",
                table: "session_participants",
                columns: new[] { "session_id", "user_id" },
                unique: true,
                filter: "user_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SessionQuestion_QuestionId",
                table: "session_questions",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_SessionQuestion_SessionId",
                table: "session_questions",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_SessionQuestion_Status",
                table: "session_questions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "UX_SessionQuestion_SessionId_QueueOrder",
                table: "session_questions",
                columns: new[] { "session_id", "queue_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Session_HostUserId",
                table: "sessions",
                column: "host_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Session_HostUserId_Status",
                table: "sessions",
                columns: new[] { "host_user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_Session_MapId",
                table: "sessions",
                column: "map_id");

            migrationBuilder.CreateIndex(
                name: "IX_Session_QuestionBankId",
                table: "sessions",
                column: "question_bank_id");

            migrationBuilder.CreateIndex(
                name: "IX_Session_SessionType",
                table: "sessions",
                column: "session_type");

            migrationBuilder.CreateIndex(
                name: "IX_Session_Status",
                table: "sessions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "UX_Session_SessionCode",
                table: "sessions",
                column: "session_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentResponse_IsCorrect",
                table: "student_responses",
                column: "is_correct");

            migrationBuilder.CreateIndex(
                name: "IX_StudentResponse_QuestionOptionId",
                table: "student_responses",
                column: "question_option_id");

            migrationBuilder.CreateIndex(
                name: "IX_StudentResponse_SessionParticipantId",
                table: "student_responses",
                column: "session_participant_id");

            migrationBuilder.CreateIndex(
                name: "IX_StudentResponse_SessionQuestionId",
                table: "student_responses",
                column: "session_question_id");

            migrationBuilder.CreateIndex(
                name: "IX_StudentResponse_SessionQuestionId_IsCorrect",
                table: "student_responses",
                columns: new[] { "session_question_id", "is_correct" });

            migrationBuilder.CreateIndex(
                name: "UX_StudentResponse_SessionQuestionId_ParticipantId",
                table: "student_responses",
                columns: new[] { "session_question_id", "session_participant_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "student_responses");

            migrationBuilder.DropTable(
                name: "question_options");

            migrationBuilder.DropTable(
                name: "session_participants");

            migrationBuilder.DropTable(
                name: "session_questions");

            migrationBuilder.DropTable(
                name: "questions");

            migrationBuilder.DropTable(
                name: "sessions");

            migrationBuilder.DropTable(
                name: "question_banks");
        }
    }
}
