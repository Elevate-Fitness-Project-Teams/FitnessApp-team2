using MediatR;

namespace ProgressTrackingService.Tests.Infrastructure;

public class MockMediatorBuilder
{
    private readonly Dictionary<Type, Func<object, Task<object>>> _handlers = new();

    public MockMediatorBuilder RegisterHandler<TRequest, TResponse>(Func<TRequest, Task<TResponse>> handler)
    {
        _handlers[typeof(TRequest)] = async req =>
        {
            var res = await handler((TRequest)req);
            return res!;
        };
        return this;
    }

    public IMediator Build() => new MockMediator(_handlers);

    private class MockMediator : IMediator
    {
        private readonly Dictionary<Type, Func<object, Task<object>>> _handlers;

        public MockMediator(Dictionary<Type, Func<object, Task<object>>> handlers)
        {
            _handlers = handlers;
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var reqType = request.GetType();
            if (_handlers.TryGetValue(reqType, out var handler))
            {
                var result = await handler(request);
                return (TResponse)result;
            }
            throw new NotImplementedException($"No mock registered for {reqType.Name}");
        }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest =>
            Task.CompletedTask;

        public Task<object?> Send(object request, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public Task Publish(object notification, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification =>
            Task.CompletedTask;

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }
}
