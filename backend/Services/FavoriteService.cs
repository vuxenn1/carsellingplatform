using CARDB_EF.Data;
using CARDB_EF.Models.EF;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CARDB_EF.Services
{
    /// <summary>
    /// Manages user favorites for cars.
    /// </summary>
    public class FavoriteService
    {
        private readonly CarDbContext cdb;

        /// <summary>
        /// Initializes a new instance of the <see cref="FavoriteService"/>.
        /// </summary>
        /// <param name="cdb">The database context.</param>
        public FavoriteService(CarDbContext cdb) => this.cdb = cdb;

        /// <summary>
        /// Adds a car to a user's favorites asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="carId">The ID of the car to add to favorites.</param>
        /// <returns>A tuple indicating the success status and a descriptive message.</returns>
        public async Task<(bool Success, string Message)> AddFavoriteAsync(int userId, int carId)
        {
            if (userId <= 0 || carId <= 0)
                return (false, "Invalid User ID or Car ID.");

            bool alreadyExists = await cdb.Favorites
                .AnyAsync(f => f.UserId == userId && f.CarId == carId);

            if (alreadyExists)
                return (false, "This car is already in your favorites.");

            var newFavorite = new UserFavorite
            {
                UserId = userId,
                CarId = carId,
                AddTime = DateTime.UtcNow
            };

            await cdb.Favorites.AddAsync(newFavorite);
            await cdb.SaveChangesAsync();

            return (true, "Favorite added successfully.");
        }

        /// <summary>
        /// Removes a car from a user's favorites asynchronously.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="carId">The ID of the car to remove from favorites.</param>
        /// <returns>A tuple indicating the success status and a descriptive message.</returns>
        public async Task<(bool Success, string Message)> RemoveFavoriteAsync(int userId, int carId)
        {
            var favorite = await cdb.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.CarId == carId);

            if (favorite == null)
                return (false, "Favorite not found.");

            cdb.Favorites.Remove(favorite);
            await cdb.SaveChangesAsync();

            return (true, "Favorite removed successfully.");
        }

        /// <summary>
        /// Asynchronously checks if a car is favorited by a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="carId">The ID of the car.</param>
        /// <returns>
        /// True if the car is favorited by the user; otherwise, false.
        /// Throws an exception on a database error.
        /// </returns>
        public async Task<bool> IsCarFavoritedAsync(int userId, int carId)
        {
            if (userId <= 0 || carId <= 0)
                return false;

            return await cdb.Favorites
                .AnyAsync(f => f.UserId == userId && f.CarId == carId);
        }

        /// <summary>
        /// Asynchronously gets the total count of a user's favorited cars.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>
        /// The integer count of the user's favorites. Returns 0 for an invalid user ID.
        /// Throws an exception on a database error.
        /// </returns>
        public async Task<int> GetFavoriteCountAsync(int userId)
        {
            if (userId <= 0)
                return 0;

            return await cdb.Favorites.CountAsync(f => f.UserId == userId);
        }
    }
}