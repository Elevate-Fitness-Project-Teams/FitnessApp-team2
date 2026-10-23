using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProgressTrackingService.Migrations
{
    /// <inheritdoc />
    public partial class ProgressSchemaV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkoutSessions");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutLogs_SessionId",
                table: "WorkoutLogs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Achievements");

            migrationBuilder.AddColumn<double>(
                name: "CurrentWeight",
                table: "UserStatistics",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "InitialWeight",
                table: "UserStatistics",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "TotalMinutes",
                table: "UserStatistics",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Streaks",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Achievements",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CriteriaType",
                table: "Achievements",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Threshold",
                table: "Achievements",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "UserBiometrics",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HeightCm = table.Column<double>(type: "float", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBiometrics", x => x.UserId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogs_SessionId",
                table: "WorkoutLogs",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogs_UserId_CompletedAt",
                table: "WorkoutLogs",
                columns: new[] { "UserId", "CompletedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_WeightHistories_UserId_Date",
                table: "WeightHistories",
                columns: new[] { "UserId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_UserId_AchievementId",
                table: "UserAchievements",
                columns: new[] { "UserId", "AchievementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_Code",
                table: "Achievements",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserBiometrics");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutLogs_SessionId",
                table: "WorkoutLogs");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutLogs_UserId_CompletedAt",
                table: "WorkoutLogs");

            migrationBuilder.DropIndex(
                name: "IX_WeightHistories_UserId_Date",
                table: "WeightHistories");

            migrationBuilder.DropIndex(
                name: "IX_UserAchievements_UserId_AchievementId",
                table: "UserAchievements");

            migrationBuilder.DropIndex(
                name: "IX_Achievements_Code",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "CurrentWeight",
                table: "UserStatistics");

            migrationBuilder.DropColumn(
                name: "InitialWeight",
                table: "UserStatistics");

            migrationBuilder.DropColumn(
                name: "TotalMinutes",
                table: "UserStatistics");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Streaks");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "CriteriaType",
                table: "Achievements");

            migrationBuilder.DropColumn(
                name: "Threshold",
                table: "Achievements");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Achievements",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "WorkoutSessions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkoutId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutSessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogs_SessionId",
                table: "WorkoutLogs",
                column: "SessionId");
        }
    }
}
