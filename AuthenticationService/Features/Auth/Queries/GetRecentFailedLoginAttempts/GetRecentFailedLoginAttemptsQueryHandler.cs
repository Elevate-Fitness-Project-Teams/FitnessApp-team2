using AuthenticationService.Common;
using AuthenticationService.Data;
using AuthenticationService.Data.Entities;
using MediatR;

namespace AuthenticationService.Features.Auth.Queries.GetRecentFailedLoginAttempts;

public class GetRecentFailedLoginAttemptsQueryHandler : IRequestHandler<GetRecentFailedLoginAttemptsQuery, Result<int>>
{
    private readonly IGeneralRepo<LoginAttempt> _repo;

    public GetRecentFailedLoginAttemptsQueryHandler(IGeneralRepo<LoginAttempt> repo)
    {
        _repo = repo;
    }

    public async Task<Result<int>> Handle(GetRecentFailedLoginAttemptsQuery request, CancellationToken cancellationToken)
    {
        return Result<int>.Success(await _repo.CountAsync(
            x => x.Email == request.Email && !x.IsSuccess && x.AttemptedAt >= request.CutoffTime,
            cancellationToken));
    }
}