using MediatR;
using WorkoutService.Common;

namespace WorkoutService.Features.Exercises.Queries.GetExerciseById;

public record GetExerciseByIdQuery(int Id) : IRequest<Result<GetExerciseByIdResponse>>;
