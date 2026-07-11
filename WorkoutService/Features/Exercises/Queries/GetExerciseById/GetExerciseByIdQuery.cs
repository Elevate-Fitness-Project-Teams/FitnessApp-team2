using MediatR;
using WorkoutService.Common;

namespace WorkoutService.Features.Exercises.Queries.GetExerciseById;

public record GetExerciseByIdQuery(Guid Id) : IRequest<Result<GetExerciseByIdResponse>>;

