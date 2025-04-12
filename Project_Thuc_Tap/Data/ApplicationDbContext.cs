using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project_Thuc_Tap.Models;

namespace Project_Thuc_Tap.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options){}

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Đặt tên bảng tùy chỉnh nếu muốn
            builder.Entity<User>().ToTable("Users");
            builder.Entity<IdentityUserClaim<string>>(e => { e.ToTable("UserClaims"); });
            builder.Entity<IdentityUserLogin<string>>(e => { e.ToTable("UserLogins"); });
            builder.Entity<IdentityUserToken<string>>(e => { e.ToTable("UserTokens"); });
            builder.Entity<IdentityRole>(e => { e.ToTable("Roles"); });
            builder.Entity<IdentityRoleClaim<string>>(e => { e.ToTable("RoleClaims"); });
            builder.Entity<IdentityUserRole<string>>(e => { e.ToTable("UserRoles"); });
        }
        public DbSet<User> users { get; set; }
        public DbSet<Room> Room { get; set; }
        public DbSet<Department> Department { get; set; }
        public DbSet<DutySchedule> DutySchedule { get; set; }
        public DbSet<ShiftChangeRequest> ShiftChangeRequests { get; set; }
        public DbSet<ShiftChangeReceiver> ShiftChangeReceivers { get; set; }
        public DbSet<CompensatoryLeave> CompensatoryLeaves { get; set; }
        public DbSet<TimeKeeping> TimeKeeping { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<DetailReports> DetailReports { get; set; }


    }
}
