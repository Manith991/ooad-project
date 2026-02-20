using System;
using System.Collections.Generic;
using Npgsql;
using OOAD_Project.Domain;

namespace OOAD_Project.Patterns.Repository
{
    /// <summary>
    /// REPOSITORY PATTERN – Order-item (order_details) data access.
    /// Centralises all order-detail row queries so forms never contain raw SQL.
    /// </summary>
    public class OrderItemRepository
    {
        // ── Domain model returned by this repository ──────────────────────
        public class OrderItem
        {
            public int OrderId { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal Subtotal { get; set; }
        }

        // ── Fetch all line items for a given order ────────────────────────
        public IEnumerable<OrderItem> GetByOrderId(int orderId)
        {
            var items = new List<OrderItem>();

            const string query = @"
                SELECT
                    od.order_id,
                    od.product_id,
                    p.productname,
                    od.quantity,
                    od.price,
                    od.subtotal
                FROM  order_details od
                JOIN  products p ON od.product_id = p.productid
                WHERE od.order_id = @order_id
                ORDER BY p.productname;";

            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@order_id", orderId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    items.Add(new OrderItem
                    {
                        OrderId = reader.GetInt32(0),
                        ProductId = reader.GetInt32(1),
                        ProductName = reader.IsDBNull(2) ? "—" : reader.GetString(2),
                        Quantity = reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                        Price = reader.IsDBNull(4) ? 0m : reader.GetDecimal(4),
                        Subtotal = reader.IsDBNull(5)
                                        ? (reader.IsDBNull(3) ? 0m : reader.GetInt32(3)) *
                                          (reader.IsDBNull(4) ? 0m : reader.GetDecimal(4))
                                        : reader.GetDecimal(5)
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderItemRepository] GetByOrderId error: {ex.Message}");
            }

            return items;
        }

        // ── Upsert a single line item ─────────────────────────────────────
        public void Upsert(int orderId, int productId, int quantity, decimal price)
        {
            const string query = @"
                INSERT INTO order_details (order_id, product_id, quantity, price)
                VALUES (@oid, @pid, @qty, @price)
                ON CONFLICT (order_id, product_id) DO UPDATE
                    SET quantity = order_details.quantity + EXCLUDED.quantity,
                        price    = EXCLUDED.price;";

            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@oid", orderId);
                cmd.Parameters.AddWithValue("@pid", productId);
                cmd.Parameters.AddWithValue("@qty", quantity);
                cmd.Parameters.AddWithValue("@price", price);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderItemRepository] Upsert error: {ex.Message}");
                throw;
            }
        }

        // ── Update quantity / price for one line item ─────────────────────
        public void Update(int orderId, int productId, int quantity, decimal price)
        {
            const string query = @"
                UPDATE order_details
                SET quantity = @qty, price = @price
                WHERE order_id = @oid AND product_id = @pid;";

            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@qty", quantity);
                cmd.Parameters.AddWithValue("@price", price);
                cmd.Parameters.AddWithValue("@oid", orderId);
                cmd.Parameters.AddWithValue("@pid", productId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderItemRepository] Update error: {ex.Message}");
                throw;
            }
        }

        // ── Recalculate orders.total_amount from line items ───────────────
        public void RefreshOrderTotal(int orderId)
        {
            const string query = @"
                UPDATE orders
                SET total_amount = (
                    SELECT COALESCE(SUM(subtotal), 0)
                    FROM   order_details
                    WHERE  order_id = @oid
                )
                WHERE order_id = @oid;";

            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@oid", orderId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderItemRepository] RefreshOrderTotal error: {ex.Message}");
                throw;
            }
        }
    }
}