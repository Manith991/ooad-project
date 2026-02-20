using System;
using System.Collections.Generic;
using Npgsql;
using OOAD_Project.Domain;

namespace OOAD_Project.Patterns.Repository
{
    /// <summary>
    /// REPOSITORY PATTERN – User data access.
    /// Extended with GetByEmail() to support the password-reset flow
    /// without raw SQL inside FormResetPass.
    /// </summary>
    public class UserRepository : IRepository<User>
    {
        // ── IRepository<User> ─────────────────────────────────────────────

        public User? GetById(int id)
        {
            const string query =
                "SELECT id, username, name, role, status, image " +
                "FROM users WHERE id = @id LIMIT 1;";
            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                using var reader = cmd.ExecuteReader();
                return reader.Read() ? MapReaderToUser(reader) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserRepository] GetById error: {ex.Message}");
                return null;
            }
        }

        public IEnumerable<User> GetAll()
        {
            var users = new List<User>();
            const string query =
                "SELECT id, username, name, role, status, image " +
                "FROM users ORDER BY id ASC;";
            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                using var cmd = new NpgsqlCommand(query, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read()) users.Add(MapReaderToUser(reader));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserRepository] GetAll error: {ex.Message}");
            }
            return users;
        }

        public int Add(User entity)
        {
            const string query = @"
                INSERT INTO users (username, name, role, status, image)
                VALUES (@username, @name, @role, @status, @image)
                RETURNING id;";
            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", (object?)entity.Username ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@name", entity.Name);
                cmd.Parameters.AddWithValue("@role", entity.Role);
                cmd.Parameters.AddWithValue("@status", entity.Status ?? "Active");
                cmd.Parameters.AddWithValue("@image", (object?)entity.Image ?? DBNull.Value);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserRepository] Add error: {ex.Message}");
                throw;
            }
        }

        public void Update(User entity)
        {
            const string query = @"
                UPDATE users
                SET name   = @name,
                    role   = @role,
                    status = @status,
                    image  = @image
                WHERE id = @id;";
            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@name", entity.Name);
                cmd.Parameters.AddWithValue("@role", entity.Role);
                cmd.Parameters.AddWithValue("@status", entity.Status ?? "Active");
                cmd.Parameters.AddWithValue("@image", (object?)entity.Image ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@id", entity.Id);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserRepository] Update error: {ex.Message}");
                throw;
            }
        }

        public void Delete(int id)
        {
            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                using var cmd = new NpgsqlCommand("DELETE FROM users WHERE id = @id;", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserRepository] Delete error: {ex.Message}");
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
                    "SELECT COUNT(*) FROM users WHERE id = @id;", conn);
                cmd.Parameters.AddWithValue("@id", id);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserRepository] Exists error: {ex.Message}");
                return false;
            }
        }

        // ── Extended query methods ────────────────────────────────────────

        /// <summary>
        /// Returns the user whose email matches, or null.
        /// Used by FormResetPass instead of raw SQL in the form.
        /// </summary>
        public User? GetByEmail(string email)
        {
            const string query =
                "SELECT id, username, name, role, status, image " +
                "FROM users WHERE email = @email LIMIT 1;";
            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@email", email);
                using var reader = cmd.ExecuteReader();
                return reader.Read() ? MapReaderToUser(reader) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserRepository] GetByEmail error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Returns the user matching username + password hash, or null.
        /// Used by LoginForm instead of raw SQL in the form.
        /// </summary>
        public User? GetByCredentials(string username, string passwordHash)
        {
            const string query =
                "SELECT id, username, name, role, status, image " +
                "FROM users WHERE username = @username AND password = @password LIMIT 1;";
            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", passwordHash);
                using var reader = cmd.ExecuteReader();
                return reader.Read() ? MapReaderToUser(reader) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserRepository] GetByCredentials error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Returns the user whose username matches, or null.
        /// Used by DiningForm instead of raw SQL for user-id lookup.
        /// </summary>
        public User? GetByUsername(string username)
        {
            const string query =
                "SELECT id, username, name, role, status, image " +
                "FROM users WHERE username = @username LIMIT 1;";
            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@username", username);
                using var reader = cmd.ExecuteReader();
                return reader.Read() ? MapReaderToUser(reader) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserRepository] GetByUsername error: {ex.Message}");
                return null;
            }
        }

        public IEnumerable<User> GetByRole(string role)
        {
            var users = new List<User>();
            const string query =
                "SELECT id, username, name, role, status, image " +
                "FROM users WHERE role = @role ORDER BY id ASC;";
            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                using var cmd = new NpgsqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@role", role);
                using var reader = cmd.ExecuteReader();
                while (reader.Read()) users.Add(MapReaderToUser(reader));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UserRepository] GetByRole error: {ex.Message}");
            }
            return users;
        }

        // ── Mapper ────────────────────────────────────────────────────────
        private static User MapReaderToUser(NpgsqlDataReader r) => new User
        {
            Id = r.GetInt32(0),
            Username = r.IsDBNull(1) ? null : r.GetString(1),
            Name = r.GetString(2),
            Role = r.GetString(3),
            Status = r.IsDBNull(4) ? "Active" : r.GetString(4),
            Image = r.IsDBNull(5) ? null : r.GetString(5)
        };
    }
}