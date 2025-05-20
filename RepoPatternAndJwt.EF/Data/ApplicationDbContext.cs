using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RepoPatternAndJwt.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoPatternAndJwt.EF.Data
{
    // Inhert form IdentityDbContext because i use it for identity
    // i use ApplicationUser because i add more porperty
    // if i didn't want add i will use identity user
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> 
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options) 
        {
            
        }
    }
}
