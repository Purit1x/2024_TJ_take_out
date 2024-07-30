using Microsoft.EntityFrameworkCore;
using Oracle.EntityFrameworkCore;

namespace takeout_tj.Data
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
		{
		}


	}
}
