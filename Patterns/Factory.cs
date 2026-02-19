namespace OOAD_Project.Patterns
{
    #region Factory Interface
    public interface IFormFactory
    {
        Form CreateForm();
    }
    #endregion

    #region Concrete Factory Implementations
    public class HomeFormFactory : IFormFactory
    {
        public Form CreateForm()
        {
            return new HomeForm();
        }
    }

    public class StaffFormFactory : IFormFactory
    {
        private readonly string _userRole;
        public StaffFormFactory(string userRole)
        {
            _userRole = userRole;
        }
        public Form CreateForm()
        {
            return new StaffForm(_userRole);
        }
    }

    public class ProductFormFactory : IFormFactory
    {
        private readonly string _userRole;
        public ProductFormFactory(string userRole)
        {
            _userRole = userRole;
        }
        public Form CreateForm()
        {
            return new ProductForm(_userRole);
        }
    }

    public class CategoriesFormFactory : IFormFactory
    {
        private readonly string _userRole;
        public CategoriesFormFactory(string userRole)
        {
            _userRole = userRole;
        }
        public Form CreateForm()
        {
            return new CategoriesForm(_userRole);
        }
    }

    public class TableFormFactory : IFormFactory
    {
        private readonly string _userRole;
        public TableFormFactory(string userRole)
        {
            _userRole = userRole;
        }
        public Form CreateForm()
        {
            return new TableForm(_userRole);
        }
    }

    public class RecordFormFactory : IFormFactory
    {
        private readonly string _userRole;
        public RecordFormFactory(string userRole)
        {
            _userRole = userRole;
        }
        public Form CreateForm()
        {
            return new RecordForm(_userRole);
        }
    }

    public class DiningFormFactory : IFormFactory
    {
        private readonly string _username;
        public DiningFormFactory(string username)
        {
            _username = username;
        }
        public Form CreateForm()
        {
            return new DiningForm(_username);
        }
    }

    public class POSFormFactory : IFormFactory
    {
        private readonly Form _parent;
        private readonly string _username;
        private readonly string _orderType;
        private readonly string? _tableName;
        private readonly int? _tableId;
        private readonly int? _orderId;
        public POSFormFactory(Form parent, string username, string orderType,
            string? tableName = null, int? tableId = null, int? orderId = null)
        {
            _parent = parent;
            _username = username;
            _orderType = orderType;
            _tableName = tableName;
            _tableId = tableId;
            _orderId = orderId;
        }
        public Form CreateForm()
        {
            return new POSForm(_parent, _username, _orderType, _tableName, _tableId, _orderId);
        }
    }
    #endregion

    #region Factory Manager (Optional - Advanced)
    public static class FormFactoryManager
    {
        public static Form CreateHomeForm()
            => new HomeFormFactory().CreateForm();
        public static Form CreateStaffForm(string userRole)
            => new StaffFormFactory(userRole).CreateForm();
        public static Form CreateProductForm(string userRole)
            => new ProductFormFactory(userRole).CreateForm();
        public static Form CreateCategoriesForm(string userRole)
            => new CategoriesFormFactory(userRole).CreateForm();
        public static Form CreateTableForm(string userRole)
            => new TableFormFactory(userRole).CreateForm();
        public static Form CreateRecordForm(string userRole)
            => new RecordFormFactory(userRole).CreateForm();
        public static Form CreateDiningForm(string username)
            => new DiningFormFactory(username).CreateForm();
        public static Form CreatePOSForm(Form parent, string username, string orderType,
            string? tableName = null, int? tableId = null, int? orderId = null)
            => new POSFormFactory(parent, username, orderType, tableName, tableId, orderId).CreateForm();
    }
    #endregion
}