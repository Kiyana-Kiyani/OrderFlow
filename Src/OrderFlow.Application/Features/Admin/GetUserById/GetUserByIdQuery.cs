using MediatR;

namespace OrderFlow.Application.Features.Admin.GetUserById
{
    public record GetUserByIdQuery(Guid UserId) : IRequest<GetUserByIdResponse>;
}
