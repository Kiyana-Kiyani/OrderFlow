using MediatR;

namespace OrderFlow.Application.Features.Restaurant.MarkOrderReadyForPickup;

public record MarkOrderReadyForPickupCommand(Guid OrderId) : IRequest<MarkOrderReadyForPickupResponse>;