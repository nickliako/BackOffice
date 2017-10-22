using System;
using System.Collections.Generic;

namespace Models
{
    public partial class Bill
    {
        public int Id { get; set; }
        public float Amount { get; set; }
        public int UserId { get; set; }
        public DateTime DateDue { get; set; }
        public string BillId { get; set; }
        public string BillDescription { get; set; }

        public User User { get; set; }
    }
}
