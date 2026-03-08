using System;
using System.Collections.Generic;
using System.Linq;

namespace OOAD_Project.Patterns.Command
{

    // =========================================================================
    //  CART ITEM  (one row in the POS grid)
    // =========================================================================
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal => Price * Quantity;

        public CartItem Clone() => new CartItem
        {
            ProductId = ProductId,
            ProductName = ProductName,
            Price = Price,
            Quantity = Quantity
        };
    }

    // =========================================================================
    //  COMMAND MANAGER  (undo / redo stacks)
    // =========================================================================
    public class CartCommandManager
    {
        private readonly Stack<ICommand> _undoStack = new();
        private readonly Stack<ICommand> _redoStack = new();

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public void Execute(ICommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();
            Console.WriteLine($"[CartCommandManager] Execute: {command.GetDescription()} " +
                              $"(undo={_undoStack.Count}, redo={_redoStack.Count})");
        }

        public void Undo()
        {
            if (!CanUndo) return;
            var cmd = _undoStack.Pop();
            cmd.Undo();
            _redoStack.Push(cmd);
            Console.WriteLine($"[CartCommandManager] Undo: {cmd.GetDescription()} " +
                              $"(undo={_undoStack.Count}, redo={_redoStack.Count})");
        }

        public void Redo()
        {
            if (!CanRedo) return;
            var cmd = _redoStack.Pop();
            cmd.Execute();
            _undoStack.Push(cmd);
            Console.WriteLine($"[CartCommandManager] Redo: {cmd.GetDescription()} " +
                              $"(undo={_undoStack.Count}, redo={_redoStack.Count})");
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }

    // =========================================================================
    //  COMMANDS
    // =========================================================================

    /// <summary>Add or increment a cart item. Undo removes/decrements it.</summary>
    public class AddCartItemCommand : ICommand
    {
        private readonly List<CartItem> _cart;
        private readonly CartItem _item;
        private bool _wasNewRow;

        public AddCartItemCommand(List<CartItem> cart, CartItem item)
        {
            _cart = cart ?? throw new ArgumentNullException(nameof(cart));
            _item = item ?? throw new ArgumentNullException(nameof(item));
        }

        public void Execute()
        {
            var existing = _cart.FirstOrDefault(c => c.ProductId == _item.ProductId);
            if (existing != null)
            {
                existing.Quantity += _item.Quantity;
                _wasNewRow = false;
            }
            else
            {
                _cart.Add(_item.Clone());
                _wasNewRow = true;
            }
        }

        public void Undo()
        {
            var existing = _cart.FirstOrDefault(c => c.ProductId == _item.ProductId);
            if (existing == null) return;
            if (_wasNewRow)
                _cart.Remove(existing);
            else
            {
                existing.Quantity -= _item.Quantity;
                if (existing.Quantity <= 0) _cart.Remove(existing);
            }
        }

        public string GetDescription() =>
            $"Add {_item.Quantity}× {_item.ProductName}";
    }

    /// <summary>Remove a cart row entirely. Undo re-inserts it at its original index.</summary>
    public class RemoveCartItemCommand : ICommand
    {
        private readonly List<CartItem> _cart;
        private readonly int _productId;
        private CartItem? _snapshot;
        private int _originalIndex;

        public RemoveCartItemCommand(List<CartItem> cart, int productId)
        {
            _cart = cart ?? throw new ArgumentNullException(nameof(cart));
            _productId = productId;
        }

        public void Execute()
        {
            var item = _cart.FirstOrDefault(c => c.ProductId == _productId)
                ?? throw new InvalidOperationException($"Product {_productId} not in cart.");
            _originalIndex = _cart.IndexOf(item);
            _snapshot = item.Clone();
            _cart.Remove(item);
        }

        public void Undo()
        {
            if (_snapshot == null) return;
            int insertAt = Math.Min(_originalIndex, _cart.Count);
            _cart.Insert(insertAt, _snapshot.Clone());
        }

        public string GetDescription() =>
            $"Remove {_snapshot?.ProductName ?? $"product {_productId}"}";
    }

    /// <summary>Update quantity of one item. Undo reverts to previous quantity.</summary>
    public class UpdateCartQuantityCommand : ICommand
    {
        private readonly List<CartItem> _cart;
        private readonly int _productId;
        private readonly int _newQuantity;
        private int _oldQuantity;

        public UpdateCartQuantityCommand(List<CartItem> cart, int productId, int newQuantity)
        {
            _cart = cart ?? throw new ArgumentNullException(nameof(cart));
            _productId = productId;
            _newQuantity = newQuantity;
        }

        public void Execute()
        {
            var item = _cart.FirstOrDefault(c => c.ProductId == _productId)
                ?? throw new InvalidOperationException($"Product {_productId} not in cart.");
            _oldQuantity = item.Quantity;
            item.Quantity = _newQuantity;
            if (item.Quantity <= 0) _cart.Remove(item);
        }

        public void Undo()
        {
            var item = _cart.FirstOrDefault(c => c.ProductId == _productId);
            if (item != null)
                item.Quantity = _oldQuantity;
        }

        public string GetDescription() =>
            $"Change qty product {_productId}: {_oldQuantity} → {_newQuantity}";
    }

    /// <summary>Clear the whole cart. Undo restores every item.</summary>
    public class ClearCartCommand : ICommand
    {
        private readonly List<CartItem> _cart;
        private List<CartItem>? _snapshot;

        public ClearCartCommand(List<CartItem> cart)
        {
            _cart = cart ?? throw new ArgumentNullException(nameof(cart));
        }

        public void Execute()
        {
            _snapshot = _cart.Select(c => c.Clone()).ToList();
            _cart.Clear();
        }

        public void Undo()
        {
            if (_snapshot == null) return;
            _cart.Clear();
            foreach (var item in _snapshot) _cart.Add(item.Clone());
        }

        public string GetDescription() =>
            $"Clear cart ({_snapshot?.Count ?? 0} items)";
    }
}