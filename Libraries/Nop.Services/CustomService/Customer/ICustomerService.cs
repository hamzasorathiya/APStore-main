using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Customers
{
    public partial interface ICustomerService
    {
        #region Customer
        IList<Nop.Core.Domain.Customers.Customer> GetAllCustomersWithItemsInCart();
        #endregion
    }
}
