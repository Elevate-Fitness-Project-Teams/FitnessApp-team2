using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FitnessCalculationService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalculatedMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Bmr = table.Column<double>(type: "float", nullable: false),
                    Tdee = table.Column<double>(type: "float", nullable: false),
                    CalorieTarget = table.Column<double>(type: "float", nullable: false),
                    CalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalculatedMetrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FitnessPlanConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Goal = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FitnessPlanConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserFitnessStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false),
                    Height = table.Column<double>(type: "float", nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Goal = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ActivityLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFitnessStats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPlanHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PlanId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeactivatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPlanHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAssignedPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FitnessPlanConfigId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAssignedPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAssignedPlans_FitnessPlanConfigs_FitnessPlanConfigId",
                        column: x => x.FitnessPlanConfigId,
                        principalTable: "FitnessPlanConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "FitnessPlanConfigs",
                columns: new[] { "Id", "Description", "Goal", "Name", "PlanId", "Status" },
                values: new object[,]
                {
                    { 1, "A plan designed for LoseWeight with a Weak metabolic rate.", "LoseWeight", "LoseWeight Plan - Weak", "LW-W", "Weak" },
                    { 2, "A plan designed for LoseWeight with a Normal metabolic rate.", "LoseWeight", "LoseWeight Plan - Normal", "LW-N", "Normal" },
                    { 3, "A plan designed for LoseWeight with a Hard metabolic rate.", "LoseWeight", "LoseWeight Plan - Hard", "LW-H", "Hard" },
                    { 4, "A plan designed for GetFitter with a Weak metabolic rate.", "GetFitter", "GetFitter Plan - Weak", "GF-W", "Weak" },
                    { 5, "A plan designed for GetFitter with a Normal metabolic rate.", "GetFitter", "GetFitter Plan - Normal", "GF-N", "Normal" },
                    { 6, "A plan designed for GetFitter with a Hard metabolic rate.", "GetFitter", "GetFitter Plan - Hard", "GF-H", "Hard" },
                    { 7, "A plan designed for GainWeight with a Weak metabolic rate.", "GainWeight", "GainWeight Plan - Weak", "GW-W", "Weak" },
                    { 8, "A plan designed for GainWeight with a Normal metabolic rate.", "GainWeight", "GainWeight Plan - Normal", "GW-N", "Normal" },
                    { 9, "A plan designed for GainWeight with a Hard metabolic rate.", "GainWeight", "GainWeight Plan - Hard", "GW-H", "Hard" },
                    { 10, "A plan designed for GainMoreFlexible with a Weak metabolic rate.", "GainMoreFlexible", "GainMoreFlexible Plan - Weak", "FL-W", "Weak" },
                    { 11, "A plan designed for GainMoreFlexible with a Normal metabolic rate.", "GainMoreFlexible", "GainMoreFlexible Plan - Normal", "FL-N", "Normal" },
                    { 12, "A plan designed for GainMoreFlexible with a Hard metabolic rate.", "GainMoreFlexible", "GainMoreFlexible Plan - Hard", "FL-H", "Hard" },
                    { 13, "A plan designed for LearnTheBasic with a Weak metabolic rate.", "LearnTheBasic", "LearnTheBasic Plan - Weak", "LB-W", "Weak" },
                    { 14, "A plan designed for LearnTheBasic with a Normal metabolic rate.", "LearnTheBasic", "LearnTheBasic Plan - Normal", "LB-N", "Normal" },
                    { 15, "A plan designed for LearnTheBasic with a Hard metabolic rate.", "LearnTheBasic", "LearnTheBasic Plan - Hard", "LB-H", "Hard" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CalculatedMetrics_UserId",
                table: "CalculatedMetrics",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FitnessPlanConfigs_Goal_Status",
                table: "FitnessPlanConfigs",
                columns: new[] { "Goal", "Status" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAssignedPlans_FitnessPlanConfigId",
                table: "UserAssignedPlans",
                column: "FitnessPlanConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAssignedPlans_UserId",
                table: "UserAssignedPlans",
                column: "UserId",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_UserFitnessStats_UserId",
                table: "UserFitnessStats",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPlanHistories_UserId_AssignedAt",
                table: "UserPlanHistories",
                columns: new[] { "UserId", "AssignedAt" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalculatedMetrics");

            migrationBuilder.DropTable(
                name: "UserAssignedPlans");

            migrationBuilder.DropTable(
                name: "UserFitnessStats");

            migrationBuilder.DropTable(
                name: "UserPlanHistories");

            migrationBuilder.DropTable(
                name: "FitnessPlanConfigs");
        }
    }
}
