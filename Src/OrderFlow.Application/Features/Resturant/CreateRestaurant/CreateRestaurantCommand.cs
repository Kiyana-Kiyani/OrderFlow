using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.Application.Features.Resturant.CreateRestaurant
{
    public record CreateRestaurantCommand(string Name, string Address, string? Description, Guid OwnerId)
        : IRequest<CreateRestaurantResponse>;

}
