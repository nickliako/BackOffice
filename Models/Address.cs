using System;
using System.Collections.Generic;

namespace Models
{
    public partial class Address
    {
        public Address()
        {
            User = new HashSet<User>();
        }

        public int AddressId { get; set; }
        public string County { get; set; }
        public string AddressName { get; set; }

        public ICollection<User> User { get; set; }
    }
}
