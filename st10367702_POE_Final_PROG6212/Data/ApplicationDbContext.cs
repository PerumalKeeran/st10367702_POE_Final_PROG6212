using Microsoft.EntityFrameworkCore;
using st10367702_POE_Final_PROG6212.Models;

namespace st10367702_POE_Final_PROG6212.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<SupportingDocument> SupportingDocuments { get; set; }
    }
}
