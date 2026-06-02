using FluentAssertions;
using OrderFlow.Domain.Entities;
using OrderFlow.Domain.Enums;
using OrderFlow.Domain.Exceptions.CustomerOrder;

namespace OrderFlow.UnitTests.Domain
{
    public class CustomerOrderTests
    {
        [Fact]
        public void Cancel_ShouldThrowOrderStateException_WhenOrderIsAlreadyPaid()
        {
            // Arrange - ۱. ساخت یک سفارش با کانستراکتور ۶ پارامتری دقیق شما
            var order = new CustomerOrder(
                customerUserId: Guid.NewGuid(),
                restaurantId: Guid.NewGuid(),
                restaurantName: "Pizza Hut",
                customerAddress: "Berlin Alexanderplatz",
                customerLatitude: 52.5200,
                customerLongitude: 13.4050
            );

            // تغییر وضعیت پرداخت به Succeeded طبق منطق دامین شما
            order.MarkPaymentAsSucceeded();

            // Act - ۲. تلاش برای کنسل کردن سفارش پرداخت شده
            Action act = () => order.Cancel();

            // Assert - ۳. بررسی دقیق پرتاب اکسپشن با پیام متناظر در کد شما
            act.Should().Throw<OrderStateException>()
                .WithMessage("Paid orders cannot be canceled due to no-refund policy constraints.");

            order.Status.Should().Be(OrderStatus.Created, "The order status must remain Created after a failed cancellation.");
        }

        [Fact]
        public void TransitionToOutForDelivery_ShouldThrowOrderStateException_WhenCourierIsNotTheAssignedOne()
        {
            // Arrange - ۱. آماده‌سازی شناسه‌ها
            var assignedCourierId = Guid.NewGuid();
            var strangerCourierId = Guid.NewGuid();

            var order = new CustomerOrder(
                customerUserId: Guid.NewGuid(),
                restaurantId: Guid.NewGuid(),
                restaurantName: "Burger King",
                customerAddress: "Berlin Center",
                customerLatitude: 52.5300,
                customerLongitude: 13.4100
            );

            // طی کردن چرخه حیات وضعیت دامین تا فاز آماده برای پیکاپ
            order.MarkPaymentAsSucceeded();
            order.StartPreparing();
            order.TransitionToReadyForPickup();
            order.AssignCourier(assignedCourierId); // تخصیص به پیک اصلی

            // Act - ۲. تلاش یک پیک دیگر (غریبه) برای تغییر وضعیت به ارسال
            Action act = () => order.TransitionToOutForDelivery(strangerCourierId);

            // Assert - ۳. تایید بلاک شدن توسط گارد دامین
            act.Should().Throw<OrderStateException>()
                .WithMessage("Only the assigned courier can pick up this order.");

            order.Status.Should().Be(OrderStatus.ReadyForPickup, "The order must remain in ReadyForPickup status.");
        }

        [Fact]
        public void StartPreparing_ShouldThrowOrderStateException_WhenOrderIsUnpaid()
        {
            // Arrange - سفارش در حالت پیش‌فرض Pending (پرداخت نشده) است
            var order = new CustomerOrder(
                customerUserId: Guid.NewGuid(),
                restaurantId: Guid.NewGuid(),
                restaurantName: "McDonalds",
                customerAddress: "Berlin Spandau",
                customerLatitude: 52.5200,
                customerLongitude: 13.2000
            );

            // Act - تلاش آشپزخانه برای شروع پخت بدون تایید پرداخت
            Action act = () => order.StartPreparing();

            // Assert
            act.Should().Throw<OrderStateException>()
                .WithMessage("Cannot start preparation on an unpaid order.");
        }

        [Fact]
        public void AssignCourier_ShouldThrowOrderStateException_WhenOrderIsNotReadyForPickup()
        {
            // Arrange - سفارش تازه ساخته شده و هنوز پخته نشده است
            var order = new CustomerOrder(
                customerUserId: Guid.NewGuid(),
                restaurantId: Guid.NewGuid(),
                restaurantName: "Subway",
                customerAddress: "Berlin Hauptbahnhof",
                customerLatitude: 52.5250,
                customerLongitude: 13.3690
            );

            // Act - تلاش پیک برای برداشتن سفارشی که هنوز آماده نیست
            Action act = () => order.AssignCourier(Guid.NewGuid());

            // Assert
            act.Should().Throw<OrderStateException>()
                .WithMessage("An order must be ready for pickup before a courier can claim it.");
        }
    }
}