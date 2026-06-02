using FluentAssertions;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Contracts.IntegrationEvents;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enums;
using OrderFlow.Infrastructure.Persistence;
using OrderFlow.IntegrationTests.Fixtures;
using System.Net;

namespace OrderFlow.IntegrationTests.Features.Couriers;

public class CourierFlowIntegrationTests : BaseIntegrationTest
{
    public CourierFlowIntegrationTests(IntegrationTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task PickupOrder_ShouldUpdateDatabaseStatus_AndPublishOutboxMessage()
    {
        // Arrange - ۱. آماده‌سازی دیتای اولیه مطابق با قوانین دامین (DDD)
        var currentCourierId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // ساخت انتیتی پیک و فعال‌سازی شیفت بر اساس قوانین دامین مدل جدید
        var testCourier = new Courier(currentCourierId, "Kiana Driver", VehicleType.Motorcycle);
        testCourier.ToggleAvailability();

        var testOrder = new CustomerOrder(
            customerUserId: Guid.NewGuid(),
            restaurantId: Guid.NewGuid(),
            restaurantName: "Burger King",
            customerAddress: "Berlin Center",
            customerLatitude: 52.5300,
            customerLongitude: 13.4100
        );

        // اضافه کردن یک آیتم برای اینکه TotalAmount دیگر صفر نباشد
        testOrder.AddOrderItem(quantity: 2, unitPrice: 75m, menuItemId: Guid.NewGuid(), menuItemName: "Whopper Menu");

        // جلو بردن وضعیت سفارش با استفاده از متدهای اصیل دامین
        testOrder.MarkPaymentAsSucceeded();
        testOrder.StartPreparing();
        testOrder.TransitionToReadyForPickup();
        testOrder.AssignCourier(currentCourierId);

        var orderId = testOrder.Id;

        // ذخیره سفارش و پیک آماده‌ی پیکاپ در دیتابیس واقعی SQL Server کانتینر
        using (var setupScope = Factory.Services.CreateScope())
        {
            var dbContext = setupScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Couriers.Add(testCourier); // 👈 ذخیره لایه دامین پیک
            dbContext.CustomerOrders.Add(testOrder);
            await dbContext.SaveChangesAsync();
        }

        var harness = Factory.Services.GetRequiredService<ITestHarness>();

        // Act - ۲. تزریق مستقیم هدر به کلاینت فعال و شلیک درخواست

        // ابتدا هدر قبلی احتمالی را پاک می‌کنیم تا تداخل ایجاد نشود
        Client.DefaultRequestHeaders.Remove("X-Test-UserId");

        // تزریق آیدی پیکی که سفارش به او تخصیص داده شده به هدرهای کلاینت اصلی
        Client.DefaultRequestHeaders.Add("X-Test-UserId", currentCourierId.ToString());

        // حالا با همان کلاینتی که توکن پیش‌فرض دارد پست میکنیم
        var response = await Client.PostAsync($"/api/v1/couriers/orders/{orderId}/pickup", null);

        // Assert - ۳. بررسی صحت عملکرد کل سیستم (HTTP, DB, Outbox)

        // الف) بررسی پاسخ لایه HTTP
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // ب) بررسی تغییر وضعیت بیزینسی در پایگاه‌داده SQL Server کانتینر
        using (var assertScope = Factory.Services.CreateScope())
        {
            var dbContext = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updatedOrder = await dbContext.CustomerOrders.FindAsync(orderId);

            updatedOrder.Should().NotBeNull();
            updatedOrder!.Status.Should().Be(OrderStatus.OutForDelivery,
                "The domain logic should transition the order state to OutForDelivery after pickup.");
        }

        // ج) بررسی عملکرد پترن Outbox (آیا پیام در مموری‌بوروکر آماده ارسال شده است؟)
        var eventPublished = await harness.Published.Any<OrderPickedUpIntegrationEvent>(
            e => e.Context.Message.OrderId == orderId);

        eventPublished.Should().BeTrue("The Outbox pattern must intercept and publish the OrderPickedUpIntegrationEvent.");
    }

    [Fact]
    public async Task PickupOrder_ShouldReturnBadRequest_WhenCourierIsNotTheAssignedOne()
    {
        // Arrange - ۱. ساخت دو پیک مجزا و یک سفارش تخصیص‌داده‌شده به پیک اول
        var assignedCourierId = Guid.NewGuid();
        var strangerCourierId = Guid.NewGuid(); // 👈 پیکی که قصد دارد سفارش را به زور بردارد

        // ساخت هر دو انتیتی پیک با وضعیت شیفت فعال جهت بررسی منطقی در هندلر
        var assignedCourier = new Courier(assignedCourierId, "Assigned Driver", VehicleType.Bicycle);
        assignedCourier.ToggleAvailability();

        var strangerCourier = new Courier(strangerCourierId, "Stranger Driver", VehicleType.Motorcycle);
        strangerCourier.ToggleAvailability();

        var testOrder = new CustomerOrder(
            customerUserId: Guid.NewGuid(),
            restaurantId: Guid.NewGuid(),
            restaurantName: "Burger King",
            customerAddress: "Berlin Center",
            customerLatitude: 52.5300,
            customerLongitude: 13.4100
        );

        testOrder.AddOrderItem(quantity: 2, unitPrice: 75m, menuItemId: Guid.NewGuid(), menuItemName: "Whopper Menu");

        // طی کردن مراحل دامین و تخصیص سفارش به پیک اصلی (Assigned Courier)
        testOrder.MarkPaymentAsSucceeded();
        testOrder.StartPreparing();
        testOrder.TransitionToReadyForPickup();
        testOrder.AssignCourier(assignedCourierId);

        var orderId = testOrder.Id;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

        // ذخیره سفارش و پیک‌ها در دیتابیس کانتینر SQL Server
        using (var setupScope = Factory.Services.CreateScope())
        {
            var dbContext = setupScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Couriers.AddRange(assignedCourier, strangerCourier); // 👈 ذخیره هر دو پیک
            dbContext.CustomerOrders.Add(testOrder);
            await dbContext.SaveChangesAsync(cts.Token);
        }

        var harness = Factory.Services.GetRequiredService<ITestHarness>();

        // Act - ۲. ارسال درخواست با هدر پیک غریبه (Stranger Courier)
        Client.DefaultRequestHeaders.Remove("X-Test-UserId");
        Client.DefaultRequestHeaders.Add("X-Test-UserId", strangerCourierId.ToString());
        HttpResponseMessage? response = null;
        Exception? caughtException = null;
        try
        {
            // شلیک رکوئست به سمت کنترلر
            response = await Client.PostAsync($"/api/v1/couriers/orders/{orderId}/pickup", null, cancellationToken: cts.Token);
        }
        catch (Exception ex)
        {
            // اگر سرور تست خطا را مستقیم بالا فرستاد، آن را ذخیره می‌کنیم تا تست متوقف نشود
            caughtException = ex;
        }
        // Assert - ۳. راستی‌آزمایی سه‌لایه‌ای امنیتی سیستم

        // الف) تایید اینکه API خطا برگردانده است (بسته به مپینگ مدیا‌آرت یا میدلور شما، ۴۰۰ یا ۴۰۹)
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "The API must reject the request because the courier identities do not match.");

        // ب) تایید غایی اینکه دیتابیس کماکان دست‌نخورده باقی مانده و وضعیت تغییر نکرده است
        using (var assertScope = Factory.Services.CreateScope())
        {
            var dbContext = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var currentOrderInDb = await dbContext.CustomerOrders.FindAsync(orderId, cts.Token);

            currentOrderInDb.Should().NotBeNull();

            // وضعیت باید کماکان ReadyForPickup باشد و به OutForDelivery نرفته باشد
            currentOrderInDb!.Status.Should().Be(OrderStatus.ReadyForPickup,
                "The order status in SQL Server must remain unchanged after a failed stranger pickup attempt.");

            // مالکیت سفارش نباید تغییر کرده باشد
            currentOrderInDb.CourierUserId.Should().Be(assignedCourierId,
                "The courier assignment must not be overwritten by the stranger.");
        }

        // ج) تایید اینکه الگوی Outbox هیچ پیامی روی شبکه منتشر نکرده است
        var eventPublished = await harness.Published.Any<OrderPickedUpIntegrationEvent>(
            e => e.Context.Message.OrderId == orderId, cts.Token);

        eventPublished.Should().BeFalse("The system must NOT publish an integration event for an illegal pickup operation.");
    }

    [Fact]
    public async Task DeliverOrder_ShouldUpdateDatabaseStatus()
    {
        // Arrange - ۱. آماده‌سازی سفارش و رساندن آن به وضعیت OutForDelivery طبق قوانین دامین
        var currentCourierId = Guid.NewGuid();

        // ساخت انتیتی پیک جهت سازگاری با پایپ‌لاین کنترلر دلیوری
        var testCourier = new Courier(currentCourierId, "Kiana Driver", VehicleType.Scooter);
        testCourier.ToggleAvailability();

        var testOrder = new CustomerOrder(
            customerUserId: Guid.NewGuid(),
            restaurantId: Guid.NewGuid(),
            restaurantName: "Pizza Hut",
            customerAddress: "Berlin Alexanderplatz",
            customerLatitude: 52.5200,
            customerLongitude: 13.4050
        );

        // اضافه کردن آیتم برای معتبر بودن مبلغ سفارش
        testOrder.AddOrderItem(quantity: 1, unitPrice: 120m, menuItemId: Guid.NewGuid(), menuItemName: "Family Pizza");

        // طی کردن چرخه حیات دامین تا فاز ارسال
        testOrder.MarkPaymentAsSucceeded();
        testOrder.StartPreparing();
        testOrder.TransitionToReadyForPickup();
        testOrder.AssignCourier(currentCourierId);
        testOrder.TransitionToOutForDelivery(currentCourierId); // سفارش الان در وضعیت دلیوری است

        var orderId = testOrder.Id;
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

        // ذخیره سفارش و پیک آماده‌ی تحویل در دیتابیس کانتینر SQL Server
        using (var setupScope = Factory.Services.CreateScope())
        {
            var dbContext = setupScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Couriers.Add(testCourier); // 👈 ذخیره لایه دامین پیک
            dbContext.CustomerOrders.Add(testOrder);
            await dbContext.SaveChangesAsync(cts.Token);
        }

        // Act - ۲. ارسال درخواست اتمام دلیوری با هدر پیک تخصیص‌داده‌شده
        Client.DefaultRequestHeaders.Remove("X-Test-UserId");
        Client.DefaultRequestHeaders.Add("X-Test-UserId", currentCourierId.ToString());
        // شلیک به اندپوینت تحویل
        var response = await Client.PostAsync($"/api/v1/couriers/orders/{orderId}/complete", null, cts.Token);

        // Assert - ۳. راستی‌آزمایی پاسخ HTTP و پایداری وضعیت در دیتابیس

        // الف) تایید پاسخ لایه HTTP (بدون محتوا / موفق)
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // ب) تایید تغییر وضعیت نهایی به Delivered در دیتابیس SQL Server کانتینر
        using (var assertScope = Factory.Services.CreateScope())
        {
            var dbContext = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var updatedOrder = await dbContext.CustomerOrders.FindAsync(orderId, cts.Token);

            updatedOrder.Should().NotBeNull();
            updatedOrder!.Status.Should().Be(OrderStatus.Delivered,
                "The domain logic must transition the order state to Delivered after a successful drop-off.");

            updatedOrder.CourierUserId.Should().Be(currentCourierId,
                "The assigned courier must remain the owner of the finalized delivery.");
        }
    }
}

















//using FluentAssertions;
//using MassTransit.Testing;
//using Microsoft.Extensions.DependencyInjection;
//using OrderFlow.Contracts.IntegrationEvents;
//using OrderFlow.Domain.Entities;
//using OrderFlow.Domain.Enums;
//using OrderFlow.Infrastructure.Persistence;
//using OrderFlow.IntegrationTests.Fixtures;
//using System.Net;

//namespace OrderFlow.IntegrationTests.Features.Couriers;

//public class CourierFlowIntegrationTests : BaseIntegrationTest
//{
//    public CourierFlowIntegrationTests(IntegrationTestWebFactory factory) : base(factory)
//    {
//    }

//    [Fact]
//    public async Task PickupOrder_ShouldUpdateDatabaseStatus_AndPublishOutboxMessage()
//    {
//        // Arrange - ۱. آماده‌سازی دیتای اولیه مطابق با قوانین دامین (DDD)
//        var currentCourierId = Guid.Parse("00000000-0000-0000-0000-000000000001");

//        var testOrder = new CustomerOrder(
//            customerUserId: Guid.NewGuid(),
//            restaurantId: Guid.NewGuid(),
//            restaurantName: "Burger King",
//            customerAddress: "Berlin Center",
//            customerLatitude: 52.5300,
//            customerLongitude: 13.4100
//        );

//        // اضافه کردن یک آیتم برای اینکه TotalAmount دیگر صفر نباشد
//        testOrder.AddOrderItem(quantity: 2, unitPrice: 75m, menuItemId: Guid.NewGuid(), menuItemName: "Whopper Menu");

//        // جلو بردن وضعیت سفارش با استفاده از متدهای اصیل دامین
//        testOrder.MarkPaymentAsSucceeded();
//        testOrder.StartPreparing();
//        testOrder.TransitionToReadyForPickup();
//        testOrder.AssignCourier(currentCourierId);

//        var orderId = testOrder.Id;

//        // ذخیره سفارش آماده‌ی پیکاپ در دیتابیس واقعی SQL Server کانتینر
//        using (var setupScope = Factory.Services.CreateScope())
//        {
//            var dbContext = setupScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//            dbContext.CustomerOrders.Add(testOrder);
//            await dbContext.SaveChangesAsync();
//        }

//        var harness = Factory.Services.GetRequiredService<ITestHarness>();

//        // Act - ۲. تزریق مستقیم هدر به کلاینت فعال و شلیک درخواست

//        // ابتدا هدر قبلی احتمالی را پاک می‌کنیم تا تداخل ایجاد نشود
//        Client.DefaultRequestHeaders.Remove("X-Test-UserId");

//        // تزریق آیدی پیکی که سفارش به او تخصیص داده شده به هدرهای کلاینت اصلی
//        Client.DefaultRequestHeaders.Add("X-Test-UserId", currentCourierId.ToString());

//        // حالا با همان کلاینتی که توکن پیش‌فرض دارد پست میکنیم
//        var response = await Client.PostAsync($"/api/v1/couriers/orders/{orderId}/pickup", null);

//        // Assert - ۳. بررسی صحت عملکرد کل سیستم (HTTP, DB, Outbox)

//        // الف) بررسی پاسخ لایه HTTP
//        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

//        // ب) بررسی تغییر وضعیت بیزینسی در پایگاه‌داده SQL Server کانتینر
//        using (var assertScope = Factory.Services.CreateScope())
//        {
//            var dbContext = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//            var updatedOrder = await dbContext.CustomerOrders.FindAsync(orderId);

//            updatedOrder.Should().NotBeNull();
//            updatedOrder!.Status.Should().Be(OrderStatus.OutForDelivery,
//                "The domain logic should transition the order state to OutForDelivery after pickup.");
//        }

//        // ج) بررسی عملکرد پترن Outbox (آیا پیام در مموری‌بوروکر آماده ارسال شده است؟)
//        var eventPublished = await harness.Published.Any<OrderPickedUpIntegrationEvent>(
//            e => e.Context.Message.OrderId == orderId);

//        eventPublished.Should().BeTrue("The Outbox pattern must intercept and publish the OrderPickedUpIntegrationEvent.");
//    }

//    [Fact]
//    public async Task PickupOrder_ShouldReturnBadRequest_WhenCourierIsNotTheAssignedOne()
//    {
//        // Arrange - ۱. ساخت دو پیک مجزا و یک سفارش تخصیص‌داده‌شده به پیک اول
//        var assignedCourierId = Guid.NewGuid();
//        var strangerCourierId = Guid.NewGuid(); // 👈 پیکی که قصد دارد سفارش را به زور بردارد

//        var testOrder = new CustomerOrder(
//            customerUserId: Guid.NewGuid(),
//            restaurantId: Guid.NewGuid(),
//            restaurantName: "Burger King",
//            customerAddress: "Berlin Center",
//            customerLatitude: 52.5300,
//            customerLongitude: 13.4100
//        );

//        testOrder.AddOrderItem(quantity: 2, unitPrice: 75m, menuItemId: Guid.NewGuid(), menuItemName: "Whopper Menu");

//        // طی کردن مراحل دامین و تخصیص سفارش به پیک اصلی (Assigned Courier)
//        testOrder.MarkPaymentAsSucceeded();
//        testOrder.StartPreparing();
//        testOrder.TransitionToReadyForPickup();
//        testOrder.AssignCourier(assignedCourierId);

//        var orderId = testOrder.Id;
//        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

//        // ذخیره سفارش در دیتابیس کانتینر SQL Server
//        using (var setupScope = Factory.Services.CreateScope())
//        {
//            var dbContext = setupScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//            dbContext.CustomerOrders.Add(testOrder);
//            await dbContext.SaveChangesAsync(cts.Token);
//        }

//        var harness = Factory.Services.GetRequiredService<ITestHarness>();

//        // Act - ۲. ارسال درخواست با هدر پیک غریبه (Stranger Courier)
//        Client.DefaultRequestHeaders.Remove("X-Test-UserId");
//        Client.DefaultRequestHeaders.Add("X-Test-UserId", strangerCourierId.ToString());
//        HttpResponseMessage? response = null;
//        Exception? caughtException = null;
//        try
//        {
//            // شلیک رکوئست به سمت کنترلر
//            response = await Client.PostAsync($"/api/v1/couriers/orders/{orderId}/pickup", null, cancellationToken: cts.Token);
//        }
//        catch (Exception ex)
//        {
//            // اگر سرور تست خطا را مستقیم بالا فرستاد، آن را ذخیره می‌کنیم تا تست متوقف نشود
//            caughtException = ex;
//        }
//        // Assert - ۳. راستی‌آزمایی سه‌لایه‌ای امنیتی سیستم

//        // الف) تایید اینکه API خطا برگردانده است (بسته به مپینگ مدیا‌آرت یا میدلور شما، ۴۰۰ یا ۴۰۹)
//        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
//            "The API must reject the request because the courier identities do not match.");

//        // ب) تایید غایی اینکه دیتابیس کماکان دست‌نخورده باقی مانده و وضعیت تغییر نکرده است
//        using (var assertScope = Factory.Services.CreateScope())
//        {
//            var dbContext = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//            var currentOrderInDb = await dbContext.CustomerOrders.FindAsync(orderId, cts.Token);

//            currentOrderInDb.Should().NotBeNull();

//            // وضعیت باید کماکان ReadyForPickup باشد و به OutForDelivery نرفته باشد
//            currentOrderInDb!.Status.Should().Be(OrderStatus.ReadyForPickup,
//                "The order status in SQL Server must remain unchanged after a failed stranger pickup attempt.");

//            // مالکیت سفارش نباید تغییر کرده باشد
//            currentOrderInDb.CourierUserId.Should().Be(assignedCourierId,
//                "The courier assignment must not be overwritten by the stranger.");
//        }

//        // ج) تایید اینکه الگوی Outbox هیچ پیامی روی شبکه منتشر نکرده است
//        var eventPublished = await harness.Published.Any<OrderPickedUpIntegrationEvent>(
//            e => e.Context.Message.OrderId == orderId, cts.Token);

//        eventPublished.Should().BeFalse("The system must NOT publish an integration event for an illegal pickup operation.");
//    }

//    [Fact]
//    public async Task DeliverOrder_ShouldUpdateDatabaseStatus()
//    {
//        // Arrange - ۱. آماده‌سازی سفارش و رساندن آن به وضعیت OutForDelivery طبق قوانین دامین
//        var currentCourierId = Guid.NewGuid();

//        var testOrder = new CustomerOrder(
//            customerUserId: Guid.NewGuid(),
//            restaurantId: Guid.NewGuid(),
//            restaurantName: "Pizza Hut",
//            customerAddress: "Berlin Alexanderplatz",
//            customerLatitude: 52.5200,
//            customerLongitude: 13.4050
//        );

//        // اضافه کردن آیتم برای معتبر بودن مبلغ سفارش
//        testOrder.AddOrderItem(quantity: 1, unitPrice: 120m, menuItemId: Guid.NewGuid(), menuItemName: "Family Pizza");

//        // طی کردن چرخه حیات دامین تا فاز ارسال
//        testOrder.MarkPaymentAsSucceeded();
//        testOrder.StartPreparing();
//        testOrder.TransitionToReadyForPickup();
//        testOrder.AssignCourier(currentCourierId);
//        testOrder.TransitionToOutForDelivery(currentCourierId); // سفارش الان در وضعیت دلیوری است

//        var orderId = testOrder.Id;
//        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

//        // ذخیره سفارش آماده‌ی تحویل در دیتابیس کانتینر SQL Server
//        using (var setupScope = Factory.Services.CreateScope())
//        {
//            var dbContext = setupScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//            dbContext.CustomerOrders.Add(testOrder);
//            await dbContext.SaveChangesAsync(cts.Token);
//        }

//        // Act - ۲. ارسال درخواست اتمام دلیوری با هدر پیک تخصیص‌داده‌شده
//        Client.DefaultRequestHeaders.Remove("X-Test-UserId");
//        Client.DefaultRequestHeaders.Add("X-Test-UserId", currentCourierId.ToString());
//        // شلیک به اندپوینت تحویل
//        var response = await Client.PostAsync($"/api/v1/couriers/orders/{orderId}/complete", null, cts.Token);

//        // Assert - ۳. راستی‌آزمایی پاسخ HTTP و پایداری وضعیت در دیتابیس

//        // الف) تایید پاسخ لایه HTTP (بدون محتوا / موفق)
//        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

//        // ب) تایید تغییر وضعیت نهایی به Delivered در دیتابیس SQL Server کانتینر
//        using (var assertScope = Factory.Services.CreateScope())
//        {
//            var dbContext = assertScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//            var updatedOrder = await dbContext.CustomerOrders.FindAsync(orderId, cts.Token);

//            updatedOrder.Should().NotBeNull();
//            updatedOrder!.Status.Should().Be(OrderStatus.Delivered,
//                "The domain logic must transition the order state to Delivered after a successful drop-off.");

//            updatedOrder.CourierUserId.Should().Be(currentCourierId,
//                "The assigned courier must remain the owner of the finalized delivery.");
//        }
//    }
//}