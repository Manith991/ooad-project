using OOAD_Project.Domain;
using OOAD_Project.Patterns.Command;
using OOAD_Project.Patterns.Repository;
using System;
using System.Collections.Generic;

namespace OOAD_Project.Patterns.Command
{
    public class DeleteOrderCommand : ICommand
    {
        private readonly int _orderId;
        private readonly OrderRepository _orderRepo;
        private readonly OrderItemRepository _itemRepo;

        private Order? _deletedOrder;
        private List<OrderItemRepository.OrderItem> _deletedItems = new();

        public DeleteOrderCommand(int orderId, OrderRepository orderRepo)
        {
            _orderId = orderId;
            _orderRepo = orderRepo ?? throw new ArgumentNullException(nameof(orderRepo));
            _itemRepo = new OrderItemRepository();
        }

        public void Execute()
        {
            _deletedOrder = _orderRepo.GetById(_orderId)
                ?? throw new InvalidOperationException($"Order #{_orderId} not found.");

            _deletedItems = new List<OrderItemRepository.OrderItem>(
                _itemRepo.GetByOrderId(_orderId));

            _orderRepo.Delete(_orderId);
            Console.WriteLine($"[DeleteOrderCommand] Deleted order #{_orderId}");
        }

        public void Undo()
        {
            if (_deletedOrder == null) return;

            int newOrderId = _orderRepo.Add(_deletedOrder);

            foreach (var item in _deletedItems)
                _itemRepo.Upsert(newOrderId, item.ProductId, item.Quantity, item.Price);

            _itemRepo.RefreshOrderTotal(newOrderId);
            Console.WriteLine($"[DeleteOrderCommand] Restored order #{_orderId} as #{newOrderId}");
        }

        public string GetDescription() => $"Delete Order #{_orderId}";
    }
}
