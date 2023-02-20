using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Data;

namespace Nop.Services.Customers
{
    public partial class CustomerService
    {
        //private readonly IRepository<Customer> _customerRepository;
        public IList<Customer> GetAllCustomersWithItemsInCart()
        {
            return _customerRepository.Table.Where(x => x.HasShoppingCartItems
            && !String.IsNullOrEmpty(x.Email)
            && x.Active
            && !x.Deleted).ToList();
            //return new List<Customer>();
        }
    }
}
