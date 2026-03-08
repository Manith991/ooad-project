using System;
using Npgsql;
using OOAD_Project.Domain;
using OOAD_Project.Patterns.Repository;

namespace OOAD_Project.Patterns.Command
{
    /// <summary>
    /// Encapsulates a staff-update with safe undo.
    /// Password is intentionally excluded from the snapshot because
    /// UserRepository.Update() never touches the password column.
    /// </summary>
    public class UpdateStaffCommand : ICommand
    {
        private readonly User _newState;
        private readonly IRepository<User> _repo;

        // Snapshot of the mutable columns only (no password needed)
        private string? _oldName;
        private string? _oldRole;
        private string? _oldStatus;
        private string? _oldImage;

        public UpdateStaffCommand(User newState, IRepository<User> repo)
        {
            _newState = newState ?? throw new ArgumentNullException(nameof(newState));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public void Execute()
        {
            // Capture only the columns our Update() statement writes
            var current = _repo.GetById(_newState.Id)
                ?? throw new InvalidOperationException(
                    $"Staff user ID {_newState.Id} not found.");

            _oldName = current.Name;
            _oldRole = current.Role;
            _oldStatus = current.Status;
            _oldImage = current.ImagePath;   // may be null – that is fine

            _repo.Update(_newState);
            Console.WriteLine($"[UpdateStaffCommand] Updated staff ID {_newState.Id}");
        }

        public void Undo()
        {
            if (_oldName == null) return;  

            var restored = new User
            {
                Id = _newState.Id,
                Username = _newState.Username,
                Name = _oldName,
                Role = _oldRole!,
                Status = _oldStatus,
                ImagePath = _oldImage
            };

            _repo.Update(restored);
            Console.WriteLine($"[UpdateStaffCommand] Restored staff ID {_newState.Id}");
        }

        public string GetDescription() =>
            $"Update Staff: {_newState.Name} (ID {_newState.Id})";
    }

    /// <summary>
    /// Encapsulates a staff-delete with full-row restore (including password hash).
    /// </summary>
    public class DeleteStaffCommand : ICommand
    {
        private readonly int _userId;
        private readonly IRepository<User> _repo;

        // Full snapshot – password included so we can truly restore
        private int _snapId;
        private string? _snapUsername;
        private string? _snapName;
        private string? _snapEmail;
        private string? _snapPassword;   // hashed value straight from DB
        private string? _snapRole;
        private string? _snapStatus;
        private string? _snapImage;

        public DeleteStaffCommand(int userId, IRepository<User> repo)
        {
            _userId = userId;
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public void Execute()
        {
            // Read the full row including the password hash before we delete
            SnapshotFullRow(_userId);

            _repo.Delete(_userId);
            Console.WriteLine($"[DeleteStaffCommand] Deleted staff ID {_userId}");
        }

        public void Undo()
        {
            if (_snapName == null) return;   // Execute() was never called

            // Re-insert the exact original row.  We use a direct INSERT so we
            // can supply the original id and the password hash.
            const string sql = @"
                INSERT INTO users (id, username, name, email, password, role, status, image)
                VALUES (@id, @username, @name, @email, @password, @role, @status, @image)
                ON CONFLICT (id) DO NOTHING;";

            try
            {
                using var conn = Database.GetConnection();
                conn.Open();
                using var cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", _snapId);
                cmd.Parameters.AddWithValue("@username", (object?)_snapUsername ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@name", _snapName);
                cmd.Parameters.AddWithValue("@email", (object?)_snapEmail ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@password", (object?)_snapPassword ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@role", _snapRole!);
                cmd.Parameters.AddWithValue("@status", (object?)_snapStatus ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@image", (object?)_snapImage ?? DBNull.Value);
                cmd.ExecuteNonQuery();
                Console.WriteLine($"[DeleteStaffCommand] Restored staff ID {_snapId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeleteStaffCommand] Undo error: {ex.Message}");
                throw;
            }
        }

        public string GetDescription() =>
            $"Delete Staff: {_snapName ?? $"ID {_userId}"}";

        // ── private helper ────────────────────────────────────────────────
        private void SnapshotFullRow(int id)
        {
            const string sql =
                "SELECT id, username, name, email, password, role, status, image " +
                "FROM users WHERE id = @id LIMIT 1;";

            using var conn = Database.GetConnection();
            conn.Open();
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var r = cmd.ExecuteReader();

            if (!r.Read())
                throw new InvalidOperationException($"Staff user ID {id} not found.");

            _snapId = r.GetInt32(0);
            _snapUsername = r.IsDBNull(1) ? null : r.GetString(1);
            _snapName = r.GetString(2);
            _snapEmail = r.IsDBNull(3) ? null : r.GetString(3);
            _snapPassword = r.IsDBNull(4) ? null : r.GetString(4);
            _snapRole = r.GetString(5);
            _snapStatus = r.IsDBNull(6) ? null : r.GetString(6);
            _snapImage = r.IsDBNull(7) ? null : r.GetString(7);
        }
    }
}