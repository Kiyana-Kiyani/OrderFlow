using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;

namespace OrderFlow.Application.Features.Resturant.RemoveResturant
{
    public class RemoveRestaurantByIdCommandHandler : IRequestHandler<RemoveRestaurantByIdCommand, RemoveRestaurantByIdResponse>
    {
        private readonly IApplicationDbContext _dbContext;

        public RemoveRestaurantByIdCommandHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<RemoveRestaurantByIdResponse> Handle(RemoveRestaurantByIdCommand request, CancellationToken cancellationToken)
        {
            var deletedRowsCount = await _dbContext.Restaurants
                .Where(r => r.Id == request.Id)
                .ExecuteDeleteAsync(cancellationToken);

            return new RemoveRestaurantByIdResponse(Success: deletedRowsCount != 0);
        }
    }
}
