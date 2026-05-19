using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;
using OrderFlow.Domain.Enums; // Adjust to wherever your OrderStatus enum lives

namespace OrderFlow.Application.Features.Couriers.GetAvailableJobs;

public class GetAvailableJobsQueryHandler : IRequestHandler<GetAvailableJobsQuery, IReadOnlyList<AvailableJobDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAvailableJobsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AvailableJobDto>> Handle(GetAvailableJobsQuery request, CancellationToken cancellationToken)
    {
        // Jobs are available for couriers when a restaurant marks them as ReadyForPickup
        var availableJobs = await _dbContext.CustomerOrders
            .Where(o => o.Status == OrderStatus.ReadyForPickup)
            .OrderBy(o => o.CreatedAt) // First-in, first-out sequencing
            .Select(x => new AvailableJobDto(
                x.Id,
                x.RestaurantId,
                x.RestaurantName,
                x.TotalAmount,
                x.CreatedAt // Or a distinct ReadyForPickup timestamp property if tracked
            ))
            .ToListAsync(cancellationToken);

        return availableJobs;
    }
}