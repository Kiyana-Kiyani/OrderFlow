using MediatR;
using Microsoft.Extensions.Logging;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Common.Events;

namespace OrderFlow.Application.Features.Admin.AssignRoleToUser
{
    public class AssignRoleToUserCommandHandler : IRequestHandler<AssignRoleToUserCommand>
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AssignRoleToUserCommandHandler> _logger;
        private readonly ICurrentUser _currentUser;
        private readonly IPublisher _publisher; // 👈 پابلیشر MediatR اضافه شد
        private readonly IApplicationDbContext _dbContext;

        public AssignRoleToUserCommandHandler(IAdminService adminService, ILogger<AssignRoleToUserCommandHandler> logger, ICurrentUser currentUser, IPublisher publisher, IApplicationDbContext dbContext)
        {
            _adminService = adminService;
            _logger = logger;
            _currentUser = currentUser;
            _publisher = publisher;
            _dbContext = dbContext;
        }

        public async Task Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
        {
            using var transaction = await _dbContext.BeginTransactionAsync(cancellationToken);
            try
            {
                await _adminService.AssignRoleToUserAsync(request.UserId, request.Role, cancellationToken);

                _logger.LogWarning("Admin {AdminId} assigned role '{Role}' to User {TargetUserId}.",
                    _currentUser.UserId, request.Role, request.UserId);

                await _publisher.Publish(new UserRoleAssignedEvent(request.UserId, request.Role), cancellationToken);

            }
            catch (Exception ex)
            {
                // در صورت بروز هرگونه خطایی در لایه آیدنتیتی یا هندلر مدیا‌آر، کل تغییرات لغو می‌شود
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to assign role and sync profile for user {UserId}", request.UserId);
                throw; // پرتاب مجدد خطا برای لایه API
            }
        }
    }
}
