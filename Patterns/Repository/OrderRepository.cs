using System;
using System.Collections.Generic;
using Npgsql;
using OOAD_Project.Domain;

namespace OOAD_Project.Patterns.Repository
{
    public class OrderRepository : IRepository<Order>
    {
        private const string BaseSelect = @"
            SELECT
                o.order_id,
                o.table_id,
                t.table_name,
                o.user_id,
                u.name  AS staff_name,
                o.order_type,
                o.order_date,
                o.total_amount,
                o.status,
                o.payment_method
            FROM orders o
            LEFT JOIN tables t ON o.table_id = t.table_id
            LEFT JOIN users  u ON o.user_id  = u.id";

        // ── IRepository<Order> ────────────────────────────────────────────
        public Order? GetById(int id)
        {
            string query = BaseSelect + " WHERE o.order_id = @id LIMIT 1;";
            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                using var reader = cmd.ExecuteReader();
                return reader.Read() ? MapReaderToOrder(reader) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderRepository] GetById error: {ex.Message}");
                return null;
            }
        }

        public IEnumerable<Order> GetAll()
        {
            var orders = new List<Order>();
            string query = BaseSelect + " ORDER BY o.order_date DESC;";
            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                using var cmd = new NpgsqlCommand(query, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read()) orders.Add(MapReaderToOrder(reader));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderRepository] GetAll error: {ex.Message}");
            }
            return orders;
        }

        public int Add(Order entity)
        {
            const string query = @"
                INSERT INTO orders
                    (table_id, user_id, order_type, order_date, total_amount, status, payment_method)
                VALUES
                    (@table_id, @user_id, @order_type, @order_date, @total_amount, @status, @payment_method)
                RETURNING order_id;";
            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@table_id", (object?)entity.TableId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@user_id", (object?)entity.UserId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@order_type", entity.OrderType ?? "Takeaway");
                cmd.Parameters.AddWithValue("@order_date", entity.OrderDate);
                cmd.Parameters.AddWithValue("@total_amount", entity.TotalAmount);
                cmd.Parameters.AddWithValue("@status", entity.Status ?? "Pending");
                cmd.Parameters.AddWithValue("@payment_method", (object?)entity.PaymentMethod ?? DBNull.Value);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderRepository] Add error: {ex.Message}");
                throw;
            }
        }

        public void Update(Order entity)
        {
            const string query = @"
                UPDATE orders
                SET table_id       = @table_id,
                    user_id        = @user_id,
                    order_type     = @order_type,
                    total_amount   = @total_amount,
                    status         = @status,
                    payment_method = @payment_method
                WHERE order_id = @order_id;";
            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@table_id", (object?)entity.TableId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@user_id", (object?)entity.UserId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@order_type", entity.OrderType ?? "Takeaway");
                cmd.Parameters.AddWithValue("@total_amount", entity.TotalAmount);
                cmd.Parameters.AddWithValue("@status", entity.Status ?? "Pending");
                cmd.Parameters.AddWithValue("@payment_method", (object?)entity.PaymentMethod ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@order_id", entity.OrderId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderRepository] Update error: {ex.Message}");
                throw;
            }
        }

        public void Delete(int id)
        {
            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                using var cmd = new NpgsqlCommand(
                    "DELETE FROM orders WHERE order_id = @id;", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderRepository] Delete error: {ex.Message}");
                throw;
            }
        }

        public bool Exists(int id)
        {
            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                using var cmd = new NpgsqlCommand(
                    "SELECT COUNT(*) FROM orders WHERE order_id = @id;", conn);
                cmd.Parameters.AddWithValue("@id", id);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderRepository] Exists error: {ex.Message}");
                return false;
            }
        }

        // ── Extended query methods ────────────────────────────────────────

        /// <summary>
        /// Returns the most-recent Unpaid order for a table, or null.
        /// Used by DiningForm instead of raw SQL.
        /// </summary>
        public Order? GetUnpaidOrderForTable(int tableId)
        {
            string query = BaseSelect +
                " WHERE o.table_id = @tid AND o.status = 'Unpaid'" +
                " ORDER BY o.order_date DESC LIMIT 1;";
            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@tid", tableId);
                using var reader = cmd.ExecuteReader();
                return reader.Read() ? MapReaderToOrder(reader) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderRepository] GetUnpaidOrderForTable error: {ex.Message}");
                return null;
            }
        }

        public IEnumerable<Order> GetByStatus(string status)
        {
            var orders = new List<Order>();
            string query = BaseSelect + " WHERE o.status = @status ORDER BY o.order_date DESC;";
            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@status", status);
                using var reader = cmd.ExecuteReader();
                while (reader.Read()) orders.Add(MapReaderToOrder(reader));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderRepository] GetByStatus error: {ex.Message}");
            }
            return orders;
        }

        public IEnumerable<Order> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            var orders = new List<Order>();
            string query = BaseSelect +
                " WHERE o.order_date BETWEEN @start AND @end ORDER BY o.order_date DESC;";
            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@start", startDate);
                cmd.Parameters.AddWithValue("@end", endDate);
                using var reader = cmd.ExecuteReader();
                while (reader.Read()) orders.Add(MapReaderToOrder(reader));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OrderRepository] GetByDateRange error: {ex.Message}");
            }
            return orders;
        }

        // ── Mapper ────────────────────────────────────────────────────────
        private static Order MapReaderToOrder(NpgsqlDataReader r) => new Order
        {
            OrderId = r.GetInt32(0),
            TableId = r.IsDBNull(1) ? (int?)null : r.GetInt32(1),
            TableName = r.IsDBNull(2) ? null : r.GetString(2),
            UserId = r.IsDBNull(3) ? (int?)null : r.GetInt32(3),
            StaffName = r.IsDBNull(4) ? null : r.GetString(4),
            OrderType = r.IsDBNull(5) ? "Takeaway" : r.GetString(5),
            OrderDate = r.GetDateTime(6),
            TotalAmount = r.GetDecimal(7),
            Status = r.IsDBNull(8) ? "Pending" : r.GetString(8),
            PaymentMethod = r.IsDBNull(9) ? null : r.GetString(9)
        };
    }
}