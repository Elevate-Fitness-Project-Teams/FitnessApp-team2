using AuthenticationService.Data;
using AuthenticationService.Data.Entities;
using MediatR;

namespace AuthenticationService.Features.Auth.Commands.LogLoginAttempt;

public class LogLoginAttemptCommandHandler : IRequestHandler<LogLoginAttemptCommand>
{
    private readonly IGeneralRepo<LoginAttempt> _repo;
    private readonly IUnitOfWork _unitOfWork;

    public LogLoginAttemptCommandHandler(IGeneralRepo<LoginAttempt> repo, IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(LogLoginAttemptCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.ExecuteAsync(async () =>
        {
            var attempt = new LoginAttempt
            {
                Email = request.Email,
                AttemptedAt = DateTime.UtcNow,
                IsSuccess = request.IsSuccess,
                IpAddress = request.IpAddress
            };
            await _repo.AddAsync(attempt, cancellationToken);
        }, cancellationToken);
    }
}