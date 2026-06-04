using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;
using OrderFlow.Domain.Enums;

namespace OrderFlow.Application.Features.Couriers.GetAvailableJobs
{
    public class GetAvailableJobsQueryHandler : IRequestHandler<GetAvailableJobsQuery, IReadOnlyList<AvailableJobDto>>
    {
        private readonly IApplicationDbContext _dbContext;

        public GetAvailableJobsQueryHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<AvailableJobDto>> Handle(GetAvailableJobsQuery request, CancellationToken cancellationToken)
        {
            var availableJobs = await _dbContext.CustomerOrders
                .Where(o => o.Status == OrderStatus.ReadyForPickup)
                .OrderBy(o => o.CreatedAt)
                .Select(x => new AvailableJobDto(
                    x.Id,
                    x.RestaurantId,
                    x.RestaurantName,
                    x.TotalAmount,
                    x.CreatedAt
                ))
                .ToListAsync(cancellationToken);

            return availableJobs;
        }
    }
}