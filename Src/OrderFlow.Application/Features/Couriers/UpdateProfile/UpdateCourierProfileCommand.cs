using MediatR;
using OrderFlow.Domain.Enums;

namespace OrderFlow.Application.Features.Couriers.UpdateProfile;

public record UpdateCourierProfileCommand(string Name, VehicleType VehicleType) : IRequest;