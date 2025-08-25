using CARDB_EF.Data;
using CARDB_EF.Models.DTOs;
using CARDB_EF.Models.EF;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CARDB_EF.Services
{
    /// <summary>
    /// Manages business logic related to car listings, including creation, updates, and retrieval.
    /// </summary>
    public class CarService
    {
        private readonly CarDbContext cdb;
        private readonly LogService logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CarService"/>.
        /// </summary>
        /// <param name="cdb">The database context.</param>
        /// <param name="logger">The service for creating log messages.</param>
        public CarService(CarDbContext cdb, LogService logger)
        {
            this.cdb = cdb;
            this.logger = logger;
        }

        /// <summary>
        /// Asynchronously retrieves detailed information for a single car.
        /// </summary>
        /// <param name="carId">The ID of the car to retrieve.</param>
        /// <returns>A <see cref="CarDetailsView"/> object if the car is found; otherwise, null.</returns>
        public async Task<CarDetailsView> GetCarDetailsAsync(int carId)
        {
            var car = await cdb.Cars
                .AsNoTracking()
                .Include(c => c.Owner)
                .FirstOrDefaultAsync(c => c.CarId == carId);

            if (car == null)
                return null;

            return new CarDetailsView
            {
                CarId = car.CarId,
                Brand = car.Brand,
                Model = car.Model,
                Year = car.Year,
                KM = car.KM,
                FuelType = car.FuelType,
                Transmission = car.Transmission,
                Price = car.Price,
                Description = car.Description,
                CarStatus = car.CarStatus,
                ListDate = car.RecordTime,
                OwnerId = car.Owner?.UserId,
                OwnerUsername = car.Owner?.Username,
                OwnerLocation = car.Owner?.UserLocation
            };
        }

        /// <summary>
        /// Asynchronously retrieves a list of all cars from the database view.
        /// </summary>
        /// <returns>A list of <see cref="CarListView"/> objects. Throws an exception on database error.</returns>
        public async Task<List<CarListView>> GetAllCarListAsync()
        {
            return await cdb.CarListView.OrderBy(c => c.CarId).ToListAsync();
        }

        /// <summary>
        /// Asynchronously retrieves a paginated, filtered, and sorted list of available cars.
        /// </summary>
        /// <param name="pageNumber">The page number to retrieve.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="brand">The brand to filter by. "all" or empty for no filter.</param>
        /// <param name="sortBy">The field to sort by (price, km, year). Defaults to list date.</param>
        /// <param name="sortDirection">The sort direction ("asc" or "desc").</param>
        /// <returns>A <see cref="PagedCarList{CarListView}"/> object containing the items for the page and pagination details.</returns>
        public async Task<PagedCarList<CarListView>> GetAllAvailableCarListAsync(int pageNumber, int pageSize, string brand, string sortBy, string sortDirection)
        {
            var query = cdb.CarListView.AsNoTracking().Where(c => c.Status == "available");

            if (!string.IsNullOrEmpty(brand) && brand.ToLower() != "all")
                query = query.Where(c => c.Brand == brand);

            // Ensure stable ordering by adding a secondary sort key
            IOrderedQueryable<CarListView> orderedQuery;
            bool isDescending = sortDirection?.ToLower() == "desc";

            switch (sortBy?.ToLower())
            {
                case "price":
                    orderedQuery = isDescending ? query.OrderByDescending(c => c.Price).ThenByDescending(c => c.ListDate) : query.OrderBy(c => c.Price).ThenByDescending(c => c.ListDate);
                    break;
                case "km":
                    orderedQuery = isDescending ? query.OrderByDescending(c => c.KM).ThenByDescending(c => c.ListDate) : query.OrderBy(c => c.KM).ThenByDescending(c => c.ListDate);
                    break;
                case "year":
                    orderedQuery = isDescending ? query.OrderByDescending(c => c.Year).ThenByDescending(c => c.ListDate) : query.OrderBy(c => c.Year).ThenByDescending(c => c.ListDate);
                    break;
                default:
                    orderedQuery = query.OrderByDescending(c => c.ListDate);
                    break;
            }

            var totalItems = await orderedQuery.CountAsync();
            var items = await orderedQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedCarList<CarListView>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        /// <summary>
        /// Asynchronously retrieves all cars listed by a specific user.
        /// </summary>
        /// <param name="userId">The ID of the car owner.</param>
        /// <returns>A list of <see cref="CarListView"/> objects. Throws an exception on database error.</returns>
        public async Task<List<CarListView>> GetUserCarListAsync(int userId)
        {
            return await cdb.CarListView
                .AsNoTracking()
                .Where(c => c.OwnerId == userId)
                .OrderBy(c => c.CarId)
                .ToListAsync();
        }

        /// <summary>
        /// Asynchronously retrieves the list of available cars favorited by a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of <see cref="CarListView"/> objects representing the user's favorite available cars.</returns>
        public async Task<List<CarListView>> GetFavoriteCarListAsync(int userId)
        {
            var favoriteCarIds = await cdb.Favorites
                .Where(f => f.UserId == userId)
                .Select(f => f.CarId)
                .ToListAsync();

            if (!favoriteCarIds.Any())
            {
                return new List<CarListView>();
            }

            return await cdb.CarListView
                .AsNoTracking()
                .Where(c => favoriteCarIds.Contains(c.CarId) && c.Status == "available")
                .OrderBy(c => c.CarId)
                .ToListAsync();
        }

        /// <summary>
        /// Asynchronously creates a new car listing.
        /// </summary>
        /// <param name="model">The model containing the data for the new car.</param>
        /// <returns>A tuple indicating success, a message, and the ID of the newly created car.</returns>
        public async Task<(bool Success, string Message, int CarId)> UploadCarAsync(UploadCarModel model)
        {
            if (model == null)
                return (false, "Car data is missing.", -1);

            var car = new ListedCar
            {
                OwnerId = model.ownerId,
                Brand = model.brand,
                Model = model.model,
                Year = model.year,
                KM = model.km,
                FuelType = model.fuelType,
                Transmission = model.transmission,
                Price = model.price,
                Description = model.description,
                CarStatus = "available",
                RecordTime = DateTime.UtcNow
            };

            await cdb.Cars.AddAsync(car);
            await cdb.SaveChangesAsync();

            string logMessage = logger.CarCreateLogger(car);
            var logToUpdate = await cdb.CarLogs
                                .Where(l => l.CarId == car.CarId && l.ActionType == "INSERT")
                                .OrderByDescending(l => l.LogId)
                                .FirstOrDefaultAsync();

            if (logToUpdate != null)
            {
                logToUpdate.ActionDetails = logMessage;
                await cdb.SaveChangesAsync();
            }

            return (true, "Car uploaded successfully.", car.CarId ?? -1);
        }

        /// <summary>
        /// Asynchronously updates an existing car's details.
        /// </summary>
        /// <param name="carId">The ID of the car to update.</param>
        /// <param name="carUpdateDto">A DTO containing the new car details.</param>
        /// <returns>A tuple indicating the success status and a descriptive message.</returns>
        public async Task<(bool Success, string Message)> UpdateCarAsync(int carId, CarUpdateDto carUpdateDto)
        {
            var carAsNoTracking = await cdb.Cars.AsNoTracking().FirstOrDefaultAsync(c => c.CarId == carId);
            if (carAsNoTracking == null)
                return (false, "Car not found.");

            string logMessage = logger.CarEditLogger(carAsNoTracking, carUpdateDto);

            var carToUpdate = await cdb.Cars.FindAsync(carId);
            carToUpdate.Brand = carUpdateDto.Brand;
            carToUpdate.Model = carUpdateDto.Model;
            carToUpdate.KM = carUpdateDto.KM;
            carToUpdate.FuelType = carUpdateDto.FuelType;
            carToUpdate.Transmission = carUpdateDto.Transmission;
            carToUpdate.Price = carUpdateDto.Price;
            carToUpdate.Description = carUpdateDto.Description;

            await cdb.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(logMessage))
            {
                var logToUpdate = await cdb.CarLogs
                                      .Where(l => l.CarId == carId && l.ActionType == "UPDATE")
                                      .OrderByDescending(l => l.LogId)
                                      .FirstOrDefaultAsync();
                if (logToUpdate != null)
                {
                    logToUpdate.ActionDetails = logMessage;
                    await cdb.SaveChangesAsync();
                }
            }

            return (true, "Car updated successfully.");
        }

        /// <summary>
        /// Asynchronously marks a car's status as 'sold'.
        /// </summary>
        /// <param name="carId">The ID of the car to mark as sold.</param>
        /// <returns>A tuple indicating the success status and a descriptive message.</returns>
        public async Task<(bool Success, string Message)> MarkCarSoldAsync(int carId)
        {
            var car = await cdb.Cars.FirstOrDefaultAsync(c => c.CarId == carId);
            if (car == null)
                return (false, "Car not found.");

            car.CarStatus = "sold";
            await cdb.SaveChangesAsync();

            string logMessage = logger.CarStatusLogger(carId, "sold");
            var logToUpdate = await cdb.CarLogs
                                .Where(l => l.CarId == carId && l.ActionType == "UPDATE")
                                .OrderByDescending(l => l.LogId)
                                .FirstOrDefaultAsync();

            if (logToUpdate != null)
            {
                logToUpdate.ActionDetails = logMessage;
                await cdb.SaveChangesAsync();
            }

            return (true, "Car status updated to 'sold'.");
        }

        /// <summary>
        /// Asynchronously marks a car's status as 'available'.
        /// </summary>
        /// <param name="carId">The ID of the car to mark as available.</param>
        /// <returns>A tuple indicating the success status and a descriptive message.</returns>
        public async Task<(bool Success, string Message)> MarkCarAvailableAsync(int carId)
        {
            var car = await cdb.Cars.FirstOrDefaultAsync(c => c.CarId == carId);
            if (car == null)
                return (false, "Car not found.");

            car.CarStatus = "available";
            await cdb.SaveChangesAsync();

            string logMessage = logger.CarStatusLogger(carId, "available");
            var logToUpdate = await cdb.CarLogs
                                .Where(l => l.CarId == carId && l.ActionType == "UPDATE")
                                .OrderByDescending(l => l.LogId)
                                .FirstOrDefaultAsync();
            if (logToUpdate != null)
            {
                logToUpdate.ActionDetails = logMessage;
                await cdb.SaveChangesAsync();
            }

            return (true, "Car status updated to 'available'.");
        }
    }
}