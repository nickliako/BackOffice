using System;
using System.Collections.Generic;

namespace Models
{
    public partial class User
    {
        public User()
        {
            Bill = new HashSet<Bill>();
        }

        public int UserId { get; set; }
        public long Vat { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public long Phone { get; set; }
        public string EMail { get; set; }
        public DateTime? LastLogOnDate { get; set; }
        public bool EmailSent { get; set; }
        public int? AddressId { get; set; }

        public Address Address { get; set; }
        public ICollection<Bill> Bill { get; set; }
    }
}
