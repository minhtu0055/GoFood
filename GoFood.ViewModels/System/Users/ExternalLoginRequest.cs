using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoFood.ViewModels.System.Users
{
    public class ExternalLoginRequest
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Provider { get; set; }
        public string ExternalId { get; set; }
        public string PictureUrl { get; set; }
    }
}
