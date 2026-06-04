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
            // Arrange
            var order = new CustomerOrder(
                customerUserId: Guid.NewGuid(),
                restaurantId: Guid.NewGuid(),
                restaurantName: "Pizza Hut",
                customerAddress: "Berlin Alexanderplatz",
                customerLatitude: 52.5200,
                customerLongitude: 13.4050
            );

            order.MarkPaymentAsSucceeded();

            // Act 
            Action act = () => order.Cancel();

            // Assert 
            act.Should().Throw<OrderStateException>()
                .WithMessage("Paid orders cannot be canceled due to no-refund policy constraints.");

            order.Status.Should().Be(OrderStatus.Created, "The order status must remain Created after a failed cancellation.");
        }

        [Fact]
        public void TransitionToOutForDelivery_ShouldThrowOrderStateException_WhenCourierIsNotTheAssignedOne()
        {
            // Arrange 
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

            order.MarkPaymentAsSucceeded();
            order.StartPreparing();
            order.TransitionToReadyForPickup();
            order.AssignCourier(assignedCourierId);
            // Act 
            Action act = () => order.TransitionToOutForDelivery(strangerCourierId);

            // Assert
            act.Should().Throw<OrderStateException>()
                .WithMessage("Only the assigned courier can pick up this order.");

            order.Status.Should().Be(OrderStatus.ReadyForPickup, "The order must remain in ReadyForPickup status.");
        }

        [Fact]
        public void StartPreparing_ShouldThrowOrderStateException_WhenOrderIsUnpaid()
        {
            // Arrange 
            var order = new CustomerOrder(
                customerUserId: Guid.NewGuid(),
                restaurantId: Guid.NewGuid(),
                restaurantName: "McDonalds",
                customerAddress: "Berlin Spandau",
                customerLatitude: 52.5200,
                customerLongitude: 13.2000
            );

            // Act 
            Action act = () => order.StartPreparing();

            // Assert
            act.Should().Throw<OrderStateException>()
                .WithMessage("Cannot start preparation on an unpaid order.");
        }

        [Fact]
        public void AssignCourier_ShouldThrowOrderStateException_WhenOrderIsNotReadyForPickup()
        {
            // Arrange 
            var order = new CustomerOrder(
                customerUserId: Guid.NewGuid(),
                restaurantId: Guid.NewGuid(),
                restaurantName: "Subway",
                customerAddress: "Berlin Hauptbahnhof",
                customerLatitude: 52.5250,
                customerLongitude: 13.3690
            );

            // Act
            Action act = () => order.AssignCourier(Guid.NewGuid());

            // Assert
            act.Should().Throw<OrderStateException>()
                .WithMessage("An order must be ready for pickup before a courier can claim it.");
        }
    }
}