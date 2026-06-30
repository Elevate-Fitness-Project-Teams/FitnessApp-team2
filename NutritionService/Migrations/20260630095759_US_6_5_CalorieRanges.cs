using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NutritionService.Migrations
{
    /// <inheritdoc />
    public partial class US_6_5_CalorieRanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MealPlans_CalorieTarget",
                table: "MealPlans");

            migrationBuilder.RenameColumn(
                name: "CalorieTarget",
                table: "MealPlans",
                newName: "TargetCalorieRangeMin");

            migrationBuilder.AddColumn<int>(
                name: "TargetCalorieRangeMax",
                table: "MealPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MealPlans_CalorieRanges",
                table: "MealPlans",
                columns: new[] { "TargetCalorieRangeMin", "TargetCalorieRangeMax" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MealPlans_CalorieRanges",
                table: "MealPlans");

            migrationBuilder.DropColumn(
                name: "TargetCalorieRangeMax",
                table: "MealPlans");

            migrationBuilder.RenameColumn(
                name: "TargetCalorieRangeMin",
                table: "MealPlans",
                newName: "CalorieTarget");

            migrationBuilder.CreateIndex(
                name: "IX_MealPlans_CalorieTarget",
                table: "MealPlans",
                column: "CalorieTarget");
        }
    }
}
