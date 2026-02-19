using OOAD_Project.Domain;

namespace OOAD_Project.Patterns
{
    /// <summary>
    /// OBSERVER PATTERN IMPLEMENTATION
    /// Defines a one-to-many dependency between objects.
    /// When Order state changes, all observers are notified automatically.
    /// </summary>

    #region Observer Interface

    /// <summary>
    /// Observer interface - all observers must implement this
    /// </summary>
    public interface IOrderObserver
    {
        void Update(Order order);
        string GetObserverName(); // For debugging/logging
    }

    #endregion

    #region Subject (Observable Order)

    /// <summary>
    /// Observable Order class - notifies observers when state changes
    /// </summary>
    public class ObservableOrder
    {
        private readonly List<IOrderObserver> _observers = new List<IOrderObserver>();
        private Order _order;

        public ObservableOrder(Order order)
        {
            _order = order;
        }

        /// <summary>
        /// Attach an observer to this order
        /// </summary>
        public void Attach(IOrderObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
                Console.WriteLine($"[Observer] Attached: {observer.GetObserverName()}");
            }
        }

        /// <summary>
        /// Detach an observer from this order
        /// </summary>
        public void Detach(IOrderObserver observer)
        {
            if (_observers.Remove(observer))
            {
                Console.WriteLine($"[Observer] Detached: {observer.GetObserverName()}");
            }
        }

        /// <summary>
        /// Notify all observers of state change
        /// </summary>
        public void NotifyObservers()
        {
            Console.WriteLine($"[Observer] Notifying {_observers.Count} observers...");
            foreach (var observer in _observers)
            {
                try
                {
                    observer.Update(_order);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Observer] Error notifying {observer.GetObserverName()}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Update order status and notify observers
        /// </summary>
        public void UpdateStatus(string newStatus)
        {
            string oldStatus = _order.Status;
            _order.Status = newStatus;
            Console.WriteLine($"[Observer] Order {_order.OrderId} status changed: {oldStatus} → {newStatus}");
            NotifyObservers();
        }

        /// <summary>
        /// Update order total and notify observers
        /// </summary>
        public void UpdateTotal(decimal newTotal)
        {
            decimal oldTotal = _order.TotalAmount;
            _order.TotalAmount = newTotal;
            Console.WriteLine($"[Observer] Order {_order.OrderId} total changed: ${oldTotal:F2} → ${newTotal:F2}");
            NotifyObservers();
        }

        /// <summary>
        /// Get the underlying order
        /// </summary>
        public Order GetOrder() => _order;
    }

    #endregion

    #region Concrete Observer Implementations

    /// <summary>
    /// Observer that updates table status when order status changes
    /// </summary>
    public class TableStatusObserver : IOrderObserver
    {
        private readonly DiningForm? _diningForm;

        public TableStatusObserver(DiningForm? diningForm = null)
        {
            _diningForm = diningForm;
        }

        public void Update(Order order)
        {
            if (order.TableId == null) return;

            // Update table status based on order status
            string newTableStatus = order.Status switch
            {
                "Paid" => "Available",
                "Pending" => "Taken",
                "Unpaid" => "Taken",
                _ => "Available"
            };

            Console.WriteLine($"[TableStatusObserver] Setting table {order.TableId} to {newTableStatus}");

            // Update in database
            try
            {
                string query = "UPDATE tables SET status = @status WHERE table_id = @tid";
                Database.Execute(query,
                    new Npgsql.NpgsqlParameter("@status", newTableStatus),
                    new Npgsql.NpgsqlParameter("@tid", order.TableId.Value));

                // Refresh UI if DiningForm is available
                _diningForm?.Invoke((Action)(() => {
                    // Refresh the dining form UI
                    // _diningForm.LoadTableCards(); // You'd need to make this public
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TableStatusObserver] Error updating table: {ex.Message}");
            }
        }

        public string GetObserverName() => "TableStatusObserver";
    }

    /// <summary>
    /// Observer that logs order changes for reporting
    /// </summary>
    public class OrderLoggingObserver : IOrderObserver
    {
        public void Update(Order order)
        {
            string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] " +
                              $"Order #{order.OrderId} | " +
                              $"Status: {order.Status} | " +
                              $"Total: ${order.TotalAmount:F2} | " +
                              $"Table: {order.TableId?.ToString() ?? "N/A"}";

            Console.WriteLine($"[OrderLoggingObserver] {logMessage}");

            // In a real app, write to log file or database
            // File.AppendAllText("order_log.txt", logMessage + Environment.NewLine);
        }

        public string GetObserverName() => "OrderLoggingObserver";
    }

    /// <summary>
    /// Observer that updates sales statistics
    /// </summary>
    public class SalesStatisticsObserver : IOrderObserver
    {
        public void Update(Order order)
        {
            // Only track completed/paid orders
            if (order.Status == "Paid")
            {
                Console.WriteLine($"[SalesStatisticsObserver] Recording sale: ${order.TotalAmount:F2}");

                // In a real app, update sales dashboard, daily totals, etc.
                // UpdateDailySales(order.TotalAmount);
                // UpdateMonthlyRevenue(order.TotalAmount);
            }
        }

        public string GetObserverName() => "SalesStatisticsObserver";
    }

    /// <summary>
    /// Observer that sends notifications (could be email, SMS, etc.)
    /// </summary>
    public class NotificationObserver : IOrderObserver
    {
        public void Update(Order order)
        {
            if (order.Status == "Paid")
            {
                Console.WriteLine($"[NotificationObserver] Sending receipt for Order #{order.OrderId}");
                // SendReceiptEmail(order);
                // SendSMSConfirmation(order);
            }
            else if (order.Status == "Cancelled")
            {
                Console.WriteLine($"[NotificationObserver] Order #{order.OrderId} was cancelled");
            }
        }

        public string GetObserverName() => "NotificationObserver";
    }

    /// <summary>
    /// Observer that updates the record/history display
    /// </summary>
    public class RecordDisplayObserver : IOrderObserver
    {
        private readonly RecordForm? _recordForm;

        public RecordDisplayObserver(RecordForm? recordForm = null)
        {
            _recordForm = recordForm;
        }

        public void Update(Order order)
        {
            Console.WriteLine($"[RecordDisplayObserver] Refreshing order history after change to Order #{order.OrderId}");

            // Refresh the record form if it's open
            _recordForm?.Invoke((Action)(() => {
                // _recordForm.LoadRecords(); // You'd need to make this public
            }));
        }

        public string GetObserverName() => "RecordDisplayObserver";
    }

    #endregion
}