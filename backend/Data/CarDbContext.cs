using CARDB_EF.Models.DTOs;
using CARDB_EF.Models.EF;
using CARDB_EF.Models.EF.Logs;
using Microsoft.EntityFrameworkCore;

namespace CARDB_EF.Data
{
    public class CarDbContext : DbContext
    {
        public CarDbContext(DbContextOptions<CarDbContext> options) : base(options)
        {
            
        }

        public DbSet<RegisteredUser> Users { get; set; }
        public DbSet<ListedCar> Cars { get; set; }
        public DbSet<CarListView> CarListView { get; set; }
        public DbSet<CarImage> CarImages { get; set; }
        public DbSet<UserFavorite> Favorites { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public DbSet<UserLog> UserLogs { get; set; }
        public DbSet<CarLog> CarLogs { get; set; }
        public DbSet<OfferLog> OfferLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            #region Main Tables
            modelBuilder.Entity<RegisteredUser>(entity =>
            {
                entity.ToTable("CDB_REGISTEREDUSER");
                entity.HasKey(u => u.UserId);

                entity.Property(u => u.UserId).HasColumnName("USERID");
                entity.Property(u => u.Username).HasColumnName("USERNAME_CD");
                entity.Property(u => u.Password).HasColumnName("PASSWORD_CD");
                entity.Property(u => u.Mail).HasColumnName("MAIL_CD");
                entity.Property(u => u.Phone).HasColumnName("PHONE_CD");
                entity.Property(u => u.UserLocation).HasColumnName("USERLOCATION_CD");
                entity.Property(u => u.IsAdmin).HasColumnName("ADMIN_IND");
                entity.Property(u => u.RecordTime).HasColumnName("RECORD_TM");
                entity.Property(u => u.IsActive).HasColumnName("USERSTATUS_IND");
            });

            modelBuilder.Entity<ListedCar>(entity =>
            {
                entity.ToTable("CDB_LISTEDCAR");
                entity.HasKey(c => c.CarId);

                entity.Property(c => c.CarId).HasColumnName("CARID");
                entity.Property(c => c.OwnerId).HasColumnName("OWNERID");
                entity.Property(c => c.Brand).HasColumnName("BRAND_CD");
                entity.Property(c => c.Model).HasColumnName("MODEL_CD");
                entity.Property(c => c.Year).HasColumnName("YEAR_NUM");
                entity.Property(c => c.KM).HasColumnName("KM_NUM");
                entity.Property(c => c.FuelType).HasColumnName("FUELTYPE_CD");
                entity.Property(c => c.Transmission).HasColumnName("TRANSMISSION_CD");
                entity.Property(c => c.Price).HasColumnName("PRICE_NUM");
                entity.Property(c => c.Description).HasColumnName("DESCRIPTION_CD");
                entity.Property(c => c.RecordTime).HasColumnName("RECORD_TM");
                entity.Property(c => c.CarStatus).HasColumnName("CARSTATUS_IND");

                entity.HasOne(c => c.Owner)
                      .WithMany()
                      .HasForeignKey(c => c.OwnerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CarImage>(entity =>
            {
                entity.ToTable("CDB_CARIMAGE");

                entity.HasKey(i => i.ImageId);
                entity.Property(i => i.ImageId).HasColumnName("IMAGEID").ValueGeneratedOnAdd();
                entity.Property(i => i.ImageUrl).HasColumnName("IMAGEURL_CD");
                entity.Property(i => i.CarId).HasColumnName("CAR_ID");
                entity.Property(i => i.AltText).HasColumnName("ALTTEXT_CD");
                entity.Property(i => i.UploadTime).HasColumnName("UPLOAD_TM");
            });

            modelBuilder.Entity<CarListView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("CDB_CARLIST_VIEW");

                entity.Property(v => v.CarId).HasColumnName("CARID");
                entity.Property(v => v.Brand).HasColumnName("BRAND");
                entity.Property(v => v.Model).HasColumnName("MODEL");
                entity.Property(v => v.Year).HasColumnName("YEAR");
                entity.Property(v => v.KM).HasColumnName("KM");
                entity.Property(v => v.Price).HasColumnName("PRICE");
                entity.Property(v => v.Status).HasColumnName("STATUS");
                entity.Property(v => v.ListDate).HasColumnName("LISTDATE");
                entity.Property(v => v.OwnerId).HasColumnName("OWNERID");
                entity.Property(v => v.OwnerUsername).HasColumnName("OWNERUSERNAME");
                entity.Property(v => v.OwnerLocation).HasColumnName("OWNERLOCATION");
                entity.Property(v => v.ThumbnailUrl).HasColumnName("THUMBNAILURL");
            });

            modelBuilder.Entity<UserFavorite>(entity =>
            {
                entity.ToTable("CDB_USERFAVORITE");
                entity.HasKey(f => f.FavId);

                entity.Property(f => f.FavId).HasColumnName("FAVID").ValueGeneratedOnAdd();
                entity.Property(f => f.UserId).HasColumnName("USER_ID");
                entity.Property(f => f.CarId).HasColumnName("CAR_ID");
                entity.Property(f => f.AddTime).HasColumnName("ADD_TM");
            });

            modelBuilder.Entity<Offer>(entity =>
            {
                entity.ToTable("CDB_OFFER");
                entity.HasKey(o => o.OfferId);

                entity.Property(o => o.OfferId).HasColumnName("OFFERID");
                entity.Property(o => o.CarId).HasColumnName("CAR_ID");
                entity.Property(o => o.SenderId).HasColumnName("OFFER_SENDER_ID");
                entity.Property(o => o.ReceiverId).HasColumnName("OFFER_RECEIVER_ID");
                entity.Property(o => o.OfferPrice).HasColumnName("OFFER_PRICE");
                entity.Property(o => o.OfferTime).HasColumnName("OFFER_TM");
                entity.Property(o => o.OfferStatus).HasColumnName("OFFERSTATUS_IND");
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("CDB_NOTIFICATION");
                entity.HasKey(n => n.NotificationId);

                entity.Property(n => n.NotificationId).HasColumnName("NTFID");
                entity.Property(n => n.MessageText).HasColumnName("MESSAGETEXT_CD");
                entity.Property(n => n.ReceiverId).HasColumnName("RECEIVERID");
                entity.Property(n => n.SentTime).HasColumnName("SENTTIME_TM");
                entity.Property(n => n.IsRead).HasColumnName("READ_IND");
            });
            #endregion

            #region Log Tables
            modelBuilder.Entity<UserLog>(entity =>
            {
                entity.ToTable("CDB_REGISTEREDUSERLOG");
                entity.HasKey(l => l.LogId);

                entity.Property(l => l.LogId).HasColumnName("LOGID").ValueGeneratedOnAdd();
                entity.Property(l => l.UserId).HasColumnName("USERID");
                entity.Property(l => l.ActionType).HasColumnName("ACTION_TYPE");
                entity.Property(l => l.ActionTime).HasColumnName("ACTION_TM");
                entity.Property(l => l.ActionDetails).HasColumnName("ACTIONDETAILS_CD");
            });

            modelBuilder.Entity<CarLog>(entity =>
            {
                entity.ToTable("CDB_LISTEDCARLOG");
                entity.HasKey(l => l.LogId);

                entity.Property(l => l.LogId).HasColumnName("LOGID").ValueGeneratedOnAdd();
                entity.Property(l => l.CarId).HasColumnName("CARID");
                entity.Property(l => l.ActionType).HasColumnName("ACTION_TYPE");
                entity.Property(l => l.ActionTime).HasColumnName("ACTION_TM");
                entity.Property(l => l.ActionDetails).HasColumnName("ACTIONDETAILS_CD");
            });

            modelBuilder.Entity<OfferLog>(entity =>
            {
                entity.ToTable("CDB_OFFERLOG");
                entity.HasKey(l => l.LogId);

                entity.Property(l => l.LogId).HasColumnName("LOGID").ValueGeneratedOnAdd();
                entity.Property(l => l.OfferId).HasColumnName("OFFERID");
                entity.Property(l => l.ActionType).HasColumnName("ACTION_TYPE");
                entity.Property(l => l.ActionTime).HasColumnName("ACTION_TM");
                entity.Property(l => l.ActionDetails).HasColumnName("ACTIONDETAILS_CD");
            });
            #endregion
        }
    }
}