using MediatR;

namespace OrderFlow.Application.Features.Couriers.GetAvailableJobs
{
    public record GetAvailableJobsQuery : IRequest<IReadOnlyList<AvailableJobDto>>;

}