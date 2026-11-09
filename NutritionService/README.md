<div align="center">

<img src="../assets/logo.svg" alt="NextRep Logo" width="280" />

# NutritionService — Macro-Aligned Meal Plans & gRPC Recommendation Engine

**Intelligent Dietary Prescription & Meal Recommendation Microservice Built with .NET 9, Vertical Slice Architecture, CQRS + MediatR, EF Core 9, RS256 JWT Authentication, and Polly-Resilient gRPC Integration**

</div>

---

## 1. Overview & Core Responsibilities

`NutritionService` (Port `:8085`) manages the nutritional domain of NextRep, transforming an athlete's metabolic calorie and macronutrient targets into actionable daily meals and structured meal plans:

- **Personalized gRPC-Driven Meal Recommendations:** Connects synchronously to `FitnessCalculationService` via gRPC (`CalorieTargetService.GetUserMetrics`) to retrieve the athlete's real-time calculated daily calorie target, then filters and ranks meals by meal type (`Breakfast`, `Lunch`, `Dinner`, `Snack`), caloric ceiling, and minimum protein thresholds.
- **Rich Relational Nutritional Graph (`NutritionDb`):** Models meals with complete macro breakdowns (`Calories`, `ProteinGrams`, `CarbsGrams`, `FatsGrams`, `FiberGrams`), preparation/cooking times, step-by-step instructions, ingredient quantities (`MealIngredients`), allergen warnings (`MealAllergens`), and dietary tags (`High-Protein`, `Keto`, `Vegan`, `Low-Carb`, etc.).
- **Resilient Inter-Service Communication:** Wraps outbound gRPC calls in **Polly resilience policies** ([`GrpcPolicies.cs`](./Common/GrpcPolicies.cs)) featuring exponential backoff retries and circuit-breaker protection.
- **Local RS256 Token Validation:** Verifies JWT Bearer tokens locally in `0 ms` using `keys/auth-service-public.pem`.

---

## 2. Architecture & gRPC Recommendation Flow

```mermaid
sequenceDiagram
    autonumber
    actor Athlete
    participant GW as ApiGateway (:5000)
    participant Nut as NutritionService (:8085)
    participant FCE as FitnessCalculationService (:8081 gRPC)
    participant SQL as NutritionDb (SQL Server)

    Athlete->>GW: GET /api/v1/nutrition/recommendations?mealType=Lunch&minProtein=35<br/>Authorization: Bearer <JWT>
    GW->>GW: Verify RS256 Signature + Redis MGET Revocation Check
    GW->>Nut: Forward authenticated request
    Nut->>Nut: Extract userId from JWT 'sub' claim

    rect rgb(235, 248, 240)
    Note over Nut,FCE: Synchronous gRPC Call (Protected by Polly Retry + Circuit Breaker)
    Nut->>FCE: CalorieTargetService.GetUserMetrics(userId)
    FCE-->>Nut: UserMetricsResponse { calorieTarget: 2450, isCalculated: true }
    end

    alt Metrics Not Calculated Yet (isCalculated == false)
        Nut-->>Athlete: 400 Bad Request (FCE_METRICS_NOT_CALCULATED)
    else Metrics Available
        Nut->>SQL: Query Meals + Ingredients + Allergens + Tags<br/>Filter by Calorie Budget, MealType & MinProtein
        SQL-->>Nut: Paginated RecommendedMealDto List
        Nut-->>Athlete: 200 OK (DailyCalorieTarget + Recommended Meals)
    end
```

---

## 3. Database Schema (`NutritionDb`)

`NutritionDb` is automatically migrated and seeded with curated athletic meals, ingredients, allergens, tags, and structured meal plans on startup via [`NutritionDbSeeder.cs`](./Common/Database/NutritionDbSeeder.cs).

```mermaid
erDiagram
    MealPlans ||--o{ MealPlanItems : "schedules"
    Meals ||--o{ MealPlanItems : "assigned to"
    Meals ||--o{ MealIngredients : "contains"
    Ingredients ||--o{ MealIngredients : "measured in"
    Meals ||--o{ MealAllergens : "flags"
    Allergens ||--o{ MealAllergens : "warns"
    Meals ||--o{ MealTags : "categorized by"
    Tags ||--o{ MealTags : "labels"

    Meals {
        guid Id PK
        string Name
        string Description
        string MealType "Breakfast | Lunch | Dinner | Snack"
        int Calories
        decimal ProteinGrams
        decimal CarbsGrams
        decimal FatsGrams
        decimal FiberGrams
        int PrepTimeMinutes
        int CookTimeMinutes
        string InstructionsJson
        string ImageUrl
    }
    MealPlans {
        guid Id PK
        string Name
        string Description
        int TargetCalories
        string GoalCategory
        int DurationDays
    }
    MealPlanItems {
        guid Id PK
        guid MealPlanId FK
        guid MealId FK
        int DayNumber
        string MealSlot
    }
    Ingredients {
        guid Id PK
        string Name
        string Category
    }
    MealIngredients {
        guid MealId PK,FK
        guid IngredientId PK,FK
        decimal Quantity
        string Unit
    }
    Allergens {
        guid Id PK
        string Name
        string Severity
    }
    MealAllergens {
        guid MealId PK,FK
        guid AllergenId PK,FK
    }
    Tags {
        guid Id PK
        string Name
    }
    MealTags {
        guid MealId PK,FK
        guid TagId PK,FK
    }
```

---

## 4. Complete REST API Reference

Base Route: `/api/v1/nutrition` (All endpoints require `Authorization: Bearer <token>`)

| Method | Endpoint | Query / Route Parameters | Description |
| :---: | :--- | :--- | :--- |
| `GET` | `/meals/{id:guid}` | `id` (Route `Guid`) | Returns full nutritional facts, macro breakdown, ingredients, allergens, tags, and preparation steps for a specific meal. |
| `GET` | `/meal-plans` | `pageNumber=1`, `pageSize=10` | Returns a paginated catalog of structured multi-day meal plans with their scheduled meals. |
| `GET` | `/meal-plans/by-calories` | `calories` (Required `int`) | Finds meal plans matching the specified daily calorie target tolerance window. |
| `GET` | `/recommendations` | `mealType?`, `maxCalories?`, `minProtein?`, `page=1`, `pageSize=10` | Fetches the authenticated user's calorie target from `FitnessCalculationService` via gRPC and returns personalized meal recommendations. |
| `GET` | `/recommendations/{userId}` | `userId` (Route), `mealType?`, `maxCalories?`, `minProtein?`, `page=1`, `pageSize=10` | Retrieves macro-aligned meal recommendations for a specific `userId`. |

---

## 5. Feature Slice Directory Structure

```text
NutritionService/
├── Common/
│   ├── Behaviors/ValidationBehavior.cs      # MediatR FluentValidation pipeline
│   ├── Database/                            # ApplicationDbContext, UnitOfWork, GenericRepository, Seeder
│   ├── Protos/calorie_target.proto          # gRPC client contract to FitnessCalculationService
│   └── GrpcPolicies.cs                      # Polly retry & circuit-breaker policies
├── Controllers/
│   └── MealPlansController.cs               # REST API endpoints (/api/v1/nutrition/*)
├── Features/
│   ├── BrowseMealPlans/                     # Paginated meal plan browsing & calorie filtering slices
│   ├── GetMealDetail/                       # Full meal macro, ingredient & allergen projection slice
│   └── GetMealRecommendations/              # gRPC-backed personalized recommendation slice
└── Models/                                  # Domain entities (Meal, MealPlan, Ingredient, Allergen, Tag)
```
