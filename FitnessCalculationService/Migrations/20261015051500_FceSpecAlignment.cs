using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessCalculationService.Migrations
{
    /// <inheritdoc />
    public partial class FceSpecAlignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserFitnessStats_UserId",
                table: "UserFitnessStats");

            migrationBuilder.DropColumn(
                name: "DeactivatedAt",
                table: "UserPlanHistories");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "UserPlanHistories",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedPlanId",
                table: "UserPlanHistories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Bmr",
                table: "UserPlanHistories",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "CalorieTarget",
                table: "UserPlanHistories",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Goal",
                table: "UserPlanHistories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreviousPlanId",
                table: "UserPlanHistories",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousStatus",
                table: "UserPlanHistories",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "UserPlanHistories",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Tdee",
                table: "UserPlanHistories",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "TriggeredBy",
                table: "UserPlanHistories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "UserFitnessStats",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "UserFitnessStats",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "SourceEventId",
                table: "UserFitnessStats",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "UserAssignedPlans",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedDurationWeeks",
                table: "FitnessPlanConfigs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "FitnessPlanConfigs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "MaxCalorie",
                table: "FitnessPlanConfigs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MinCalorie",
                table: "FitnessPlanConfigs",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "ProgramType",
                table: "FitnessPlanConfigs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "WorkoutsPerWeek",
                table: "FitnessPlanConfigs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "CalculatedMetrics",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastWeightEventAt",
                table: "CalculatedMetrics",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceStatsId",
                table: "CalculatedMetrics",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "CalculatedMetrics",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000000000000"),
                columns: new[] { "Description", "EstimatedDurationWeeks", "IsActive", "MaxCalorie", "MinCalorie", "Name", "ProgramType", "WorkoutsPerWeek" },
                values: new object[] { "A gentle caloric deficit program focused on low-impact movement and metabolic recovery.", 12, true, 1800.0, 1200.0, "Lose Weight Plan - Weak", "Low-Impact Cardio + Bodyweight", 3 });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000000000001"),
                columns: new[] { "Description", "EstimatedDurationWeeks", "IsActive", "MaxCalorie", "MinCalorie", "Name", "ProgramType", "WorkoutsPerWeek" },
                values: new object[] { "A balanced fat-loss program combining progressive resistance training and cardiovascular work.", 10, true, 2500.0, 1801.0, "Lose Weight Plan - Normal", "Cardio + Strength", 4 });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000000000002"),
                columns: new[] { "Description", "EstimatedDurationWeeks", "IsActive", "MaxCalorie", "MinCalorie", "Name", "ProgramType", "WorkoutsPerWeek" },
                values: new object[] { "High-intensity athletic conditioning designed for maximum calorie expenditure and lean mass retention.", 8, true, 4500.0, 2501.0, "Lose Weight Plan - Hard", "HIIT + Strength Circuits", 5 });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000100000000"),
                columns: new[] { "Description", "EstimatedDurationWeeks", "IsActive", "MaxCalorie", "MinCalorie", "Name", "ProgramType", "WorkoutsPerWeek" },
                values: new object[] { "Baseline aerobic and muscular conditioning to build daily physical vitality.", 8, true, 1800.0, 1200.0, "Get Fitter Plan - Weak", "Full-Body Conditioning", 3 });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000100000001"),
                columns: new[] { "Description", "EstimatedDurationWeeks", "IsActive", "MaxCalorie", "MinCalorie", "Name", "ProgramType", "WorkoutsPerWeek" },
                values: new object[] { "Dynamic functional fitness emphasizing stamina, mobility, and core strength.", 8, true, 2500.0, 1801.0, "Get Fitter Plan - Normal", "Functional Strength + Cardio", 4 });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000100000002"),
                columns: new[] { "Description", "EstimatedDurationWeeks", "IsActive", "MaxCalorie", "MinCalorie", "Name", "ProgramType", "WorkoutsPerWeek" },
                values: new object[] { "Comprehensive athletic performance conditioning targeting speed, power, and high work capacity.", 6, true, 4500.0, 2501.0, "Get Fitter Plan - Hard", "Athletic Performance", 5 });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000200000000"),
                columns: new[] { "Description", "EstimatedDurationWeeks", "IsActive", "MaxCalorie", "MinCalorie", "Name", "ProgramType", "WorkoutsPerWeek" },
                values: new object[] { "Introductory hypertrophy and controlled surplus program to stimulate healthy mass accrual.", 12, true, 1800.0, 1200.0, "Gain Weight Plan - Weak", "Foundational Hypertrophy", 3 });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000200000001"),
                columns: new[] { "Description", "EstimatedDurationWeeks", "IsActive", "MaxCalorie", "MinCalorie", "Name", "ProgramType", "WorkoutsPerWeek" },
                values: new object[] { "Standard muscle hypertrophy split with progressive overload and optimal volume distribution.", 12, true, 2500.0, 1801.0, "Gain Weight Plan - Normal", "Hypertrophy Split", 4 });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000200000002"),
                columns: new[] { "Description", "EstimatedDurationWeeks", "IsActive", "MaxCalorie", "MinCalorie", "Name", "ProgramType", "WorkoutsPerWeek" },
                values: new object[] { "High-volume strength and muscle-building program supported by high caloric demand.", 10, true, 4500.0, 2501.0, "Gain Weight Plan - Hard", "Progressive Overload Strength", 5 });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000300000000"),
                columns: new[] { "Description", "EstimatedDurationWeeks", "IsActive", "MaxCalorie", "MinCalorie", "Name", "ProgramType", "WorkoutsPerWeek" },
                values: new object[] { "Gentle joint mobility and active stretching routines for improved range of motion.", 6, true, 1800.0, 1200.0, "Gain More Flexible Plan - Weak", "Mobility + Stretching", 3 });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000300000001"),
                columns: new[] { "Description", "EstimatedDurationWeeks", "IsActive", "MaxCalorie", "MinCalorie", "Name", "ProgramType", "WorkoutsPerWeek" },
                values: new object[] { "Vinyasa-inspired mobility flows combined with stability exercises for supple joints.", 6, true, 2500.0, 1801.0, "Gain More Flexible Plan - Normal", "Yoga Flow + Mobility", 4 });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000300000002"),
                columns: new[] { "Description", "EstimatedDurationWeeks", "IsActive", "MaxCalorie", "MinCalorie", "Name", "ProgramType", "WorkoutsPerWeek" },
                values: new object[] { "Challenging flexibility training integrating dynamic strength, end-range control, and deep isometric holds.", 8, true, 4500.0, 2501.0, "Gain More Flexible Plan - Hard", "Power Yoga + Pilates", 5 });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000400000000"),
                columns: new[] { "Description", "EstimatedDurationWeeks", "IsActive", "MaxCalorie", "MinCalorie", "Name", "ProgramType", "WorkoutsPerWeek" },
                values: new object[] { "Essential fundamental movement patterns (squat, hinge, push, pull) with low physical strain.", 4, true, 1800.0, 1200.0, "Learn the Basic Plan - Weak", "Intro Bodyweight Basics", 2 });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000400000001"),
                columns: new[] { "Description", "EstimatedDurationWeeks", "IsActive", "MaxCalorie", "MinCalorie", "Name", "ProgramType", "WorkoutsPerWeek" },
                values: new object[] { "Comprehensive introduction to gym equipment, resistance training form, and training cadence.", 4, true, 2500.0, 1801.0, "Learn the Basic Plan - Normal", "Fundamentals Full-Body", 3 });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000400000002"),
                columns: new[] { "Description", "EstimatedDurationWeeks", "IsActive", "MaxCalorie", "MinCalorie", "Name", "ProgramType", "WorkoutsPerWeek" },
                values: new object[] { "Accelerated foundation bootcamp establishing high motor competency and general physical preparedness.", 6, true, 4500.0, 2501.0, "Learn the Basic Plan - Hard", "Fundamentals + Conditioning", 4 });

            migrationBuilder.CreateIndex(
                name: "IX_UserFitnessStats_UserId_RecordedAt",
                table: "UserFitnessStats",
                columns: new[] { "UserId", "RecordedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_FitnessPlanConfigs_PlanId",
                table: "FitnessPlanConfigs",
                column: "PlanId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserFitnessStats_UserId_RecordedAt",
                table: "UserFitnessStats");

            migrationBuilder.DropIndex(
                name: "IX_FitnessPlanConfigs_PlanId",
                table: "FitnessPlanConfigs");

            migrationBuilder.DropColumn(
                name: "AssignedPlanId",
                table: "UserPlanHistories");

            migrationBuilder.DropColumn(
                name: "Bmr",
                table: "UserPlanHistories");

            migrationBuilder.DropColumn(
                name: "CalorieTarget",
                table: "UserPlanHistories");

            migrationBuilder.DropColumn(
                name: "Goal",
                table: "UserPlanHistories");

            migrationBuilder.DropColumn(
                name: "PreviousPlanId",
                table: "UserPlanHistories");

            migrationBuilder.DropColumn(
                name: "PreviousStatus",
                table: "UserPlanHistories");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "UserPlanHistories");

            migrationBuilder.DropColumn(
                name: "Tdee",
                table: "UserPlanHistories");

            migrationBuilder.DropColumn(
                name: "TriggeredBy",
                table: "UserPlanHistories");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "UserFitnessStats");

            migrationBuilder.DropColumn(
                name: "SourceEventId",
                table: "UserFitnessStats");

            migrationBuilder.DropColumn(
                name: "EstimatedDurationWeeks",
                table: "FitnessPlanConfigs");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "FitnessPlanConfigs");

            migrationBuilder.DropColumn(
                name: "MaxCalorie",
                table: "FitnessPlanConfigs");

            migrationBuilder.DropColumn(
                name: "MinCalorie",
                table: "FitnessPlanConfigs");

            migrationBuilder.DropColumn(
                name: "ProgramType",
                table: "FitnessPlanConfigs");

            migrationBuilder.DropColumn(
                name: "WorkoutsPerWeek",
                table: "FitnessPlanConfigs");

            migrationBuilder.DropColumn(
                name: "LastWeightEventAt",
                table: "CalculatedMetrics");

            migrationBuilder.DropColumn(
                name: "SourceStatsId",
                table: "CalculatedMetrics");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CalculatedMetrics");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserPlanHistories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivatedAt",
                table: "UserPlanHistories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserFitnessStats",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserAssignedPlans",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "CalculatedMetrics",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000000000000"),
                columns: new[] { "Description", "Name" },
                values: new object[] { "A plan designed for LoseWeight with a Weak metabolic rate.", "LoseWeight Plan - Weak" });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000000000001"),
                columns: new[] { "Description", "Name" },
                values: new object[] { "A plan designed for LoseWeight with a Normal metabolic rate.", "LoseWeight Plan - Normal" });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000000000002"),
                columns: new[] { "Description", "Name" },
                values: new object[] { "A plan designed for LoseWeight with a Hard metabolic rate.", "LoseWeight Plan - Hard" });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000100000000"),
                columns: new[] { "Description", "Name" },
                values: new object[] { "A plan designed for GetFitter with a Weak metabolic rate.", "GetFitter Plan - Weak" });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000100000001"),
                columns: new[] { "Description", "Name" },
                values: new object[] { "A plan designed for GetFitter with a Normal metabolic rate.", "GetFitter Plan - Normal" });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000100000002"),
                columns: new[] { "Description", "Name" },
                values: new object[] { "A plan designed for GetFitter with a Hard metabolic rate.", "GetFitter Plan - Hard" });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000200000000"),
                columns: new[] { "Description", "Name" },
                values: new object[] { "A plan designed for GainWeight with a Weak metabolic rate.", "GainWeight Plan - Weak" });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000200000001"),
                columns: new[] { "Description", "Name" },
                values: new object[] { "A plan designed for GainWeight with a Normal metabolic rate.", "GainWeight Plan - Normal" });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000200000002"),
                columns: new[] { "Description", "Name" },
                values: new object[] { "A plan designed for GainWeight with a Hard metabolic rate.", "GainWeight Plan - Hard" });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000300000000"),
                columns: new[] { "Description", "Name" },
                values: new object[] { "A plan designed for GainMoreFlexible with a Weak metabolic rate.", "GainMoreFlexible Plan - Weak" });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000300000001"),
                columns: new[] { "Description", "Name" },
                values: new object[] { "A plan designed for GainMoreFlexible with a Normal metabolic rate.", "GainMoreFlexible Plan - Normal" });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000300000002"),
                columns: new[] { "Description", "Name" },
                values: new object[] { "A plan designed for GainMoreFlexible with a Hard metabolic rate.", "GainMoreFlexible Plan - Hard" });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000400000000"),
                columns: new[] { "Description", "Name" },
                values: new object[] { "A plan designed for LearnTheBasic with a Weak metabolic rate.", "LearnTheBasic Plan - Weak" });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000400000001"),
                columns: new[] { "Description", "Name" },
                values: new object[] { "A plan designed for LearnTheBasic with a Normal metabolic rate.", "LearnTheBasic Plan - Normal" });

            migrationBuilder.UpdateData(
                table: "FitnessPlanConfigs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-000400000002"),
                columns: new[] { "Description", "Name" },
                values: new object[] { "A plan designed for LearnTheBasic with a Hard metabolic rate.", "LearnTheBasic Plan - Hard" });

            migrationBuilder.CreateIndex(
                name: "IX_UserFitnessStats_UserId",
                table: "UserFitnessStats",
                column: "UserId",
                unique: true);
        }
    }
}
