using System;

namespace Nop.Core.Domain.Orders
{
    public partial class AccountingDetailModel
    {
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the total amount
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the total quantity
        /// </summary>
        public int TotalQuantity { get; set; }
        /// <summary>
        /// Gets or sets the Product price
        /// </summary>
        public int UnitPric { get; set; }

        
    }
}