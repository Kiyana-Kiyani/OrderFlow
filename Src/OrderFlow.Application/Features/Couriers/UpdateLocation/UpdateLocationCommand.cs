using MediatR;

namespace OrderFlow.Application.Features.Couriers.UpdateLocation
{
    public record UpdateLocationCommand(double Latitude, double Longitude) : IRequest<UpdateLocationResponse>;
}
