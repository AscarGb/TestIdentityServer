using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Types
{
    public class ApplicationUser : IdentityUser
    {
        public int Year { get; set; }   // год рождения
    }
}
