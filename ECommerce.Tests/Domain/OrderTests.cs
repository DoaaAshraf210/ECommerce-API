using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Tests.Domain
{
    public class OrderTests
    {

        [Theory]
        [InlineData(OrderStatus.Pending, OrderStatus.Paid)]
        [InlineData(OrderStatus.Pending, OrderStatus.Cancelled)]
        [InlineData(OrderStatus.Paid, OrderStatus.Shipped)]
        [InlineData(OrderStatus.Paid, OrderStatus.Cancelled)]
        [InlineData(OrderStatus.Shipped, OrderStatus.Delivered)]
        public void CanTransitionTo_ValidTransition_ReturnsTrue(OrderStatus from, OrderStatus to)
        {
            var order = new Order { Status = from };

            var result = order.CanTransitionTo(to);

            Assert.True(result);
        }

        [Theory]
        [InlineData(OrderStatus.Pending, OrderStatus.Pending)]
        [InlineData(OrderStatus.Pending, OrderStatus.Shipped)]
        [InlineData(OrderStatus.Pending, OrderStatus.Delivered)]
        [InlineData(OrderStatus.Paid, OrderStatus.Pending)]
        [InlineData(OrderStatus.Paid, OrderStatus.Delivered)]
        [InlineData(OrderStatus.Shipped, OrderStatus.Cancelled)]
        [InlineData(OrderStatus.Shipped, OrderStatus.Paid)]
        [InlineData(OrderStatus.Delivered, OrderStatus.Cancelled)]
        [InlineData(OrderStatus.Delivered, OrderStatus.Shipped)]
        [InlineData(OrderStatus.Cancelled, OrderStatus.Pending)]
        [InlineData(OrderStatus.Cancelled, OrderStatus.Paid)]
        public void CanTransitionTo_InvalidTransition_ReturnsFalse(OrderStatus from, OrderStatus to)
        {
            var order = new Order { Status = from };

            var result = order.CanTransitionTo(to);

            Assert.False(result);
        }





    }
}
