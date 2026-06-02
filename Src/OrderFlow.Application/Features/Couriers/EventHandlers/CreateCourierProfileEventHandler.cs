using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Common.Events;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enums;

namespace OrderFlow.Application.Features.Couriers.EventHandlers
{
    public class CreateCourierProfileEventHandler : INotificationHandler<UserRoleAssignedEvent>
    {
        private readonly IApplicationDbContext _dbContext;

        public CreateCourierProfileEventHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Handle(UserRoleAssignedEvent notification, CancellationToken cancellationToken)
        {
            // اگر نقشی که اضافه شده Courier نیست، کلاً بی‌خیال شو
            if (notification.Role != "Courier")
                return;

            // بررسی می‌کنیم که آیا قبلاً پروفایلی برای این آیدی ساخته شده یا نه؟
            var profileExists = await _dbContext.Couriers
                .AnyAsync(c => c.Id == notification.UserId, cancellationToken);

            if (profileExists)
                return;

            // ساخت پروفایل بیزینسی پیک با اطلاعات پیش‌فرض
            // (در آینده می‌توانی نام واقعی کاربر را از دیتابیس بخوانی یا آپدیت کنی)
            var newCourier = new Courier(
                id: notification.UserId,
                name: "New Courier", // نام موقت
                vehicleType: VehicleType.Bicycle // وسیله نقلیه پیش‌فرض
            );

            _dbContext.Couriers.Add(newCourier);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

