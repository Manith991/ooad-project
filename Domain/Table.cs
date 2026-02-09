namespace OOAD_Project.Domain
{
    public class Table
    {
        public int TableId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string Status { get; set; } = "Available";
    }
}
