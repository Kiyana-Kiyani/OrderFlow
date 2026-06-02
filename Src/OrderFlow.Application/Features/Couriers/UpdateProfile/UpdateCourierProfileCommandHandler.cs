using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application.Abstractions;
using OrderFlow.Application.Abstractions.Authentication;
using OrderFlow.Application.Common.Exceptions; // فرض بر وجود NotFoundException اختصاصی تو

namespace OrderFlow.Application.Features.Couriers.UpdateProfile;

public class UpdateCourierProfileCommandHandler : IRequestHandler<UpdateCourierProfileCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUser _currentUser;

    public UpdateCourierProfileCommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateCourierProfileCommand request, CancellationToken cancellationToken)
    {
        // ۱. پیدا کردن پروفایل پیک لاگین شده بر اساس آیدی توکن او
        var courier = await _dbContext.Couriers
            .FirstOrDefaultAsync(c => c.Id == _currentUser.UserId, cancellationToken);

        if (courier is null)
        {
            throw new NotFoundException("Courier Profile", _currentUser.UserId);
        }

        // ۲. 🔥 استفاده از متدهای کپسوله‌شده دامین مدل برای اعمال تغییرات امن
        courier.UpdateName(request.Name);
        courier.UpdateVehicleType(request.VehicleType);

        // ۳. ذخیره تغییرات در دیتابیس
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}