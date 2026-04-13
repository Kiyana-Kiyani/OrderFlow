using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Application.Features.MenuItems.GetMenuItems;

namespace OrderFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuItemsController : ControllerBase
    {
      private readonly ISender _sender;

        public MenuItemsController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll(CancellationToken cancellationToken) 
        { 
            var response = _sender.Send(new GetMenuItemsQuery(), cancellationToken);


        }



    }
}
