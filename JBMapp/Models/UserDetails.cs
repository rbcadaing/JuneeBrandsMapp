using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JBMapp.Models
{
    public class UserDetails
    {
        public int EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Position { get; set; }
        public string Email { get; set; }
        public bool IsAcknowledger { get; set; }
        public bool IsAdmin { get; set; }
    }
}