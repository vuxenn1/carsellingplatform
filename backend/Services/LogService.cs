using CARDB_EF.Data;
using CARDB_EF.Models.DTOs;
using CARDB_EF.Models.EF;
using System.Globalization;
using System.Text;

namespace CARDB_EF.Services
{
    /// <summary>
    /// A service responsible for generating formatted log message strings for various application events.
    /// </summary>
    public class LogService
    {
        private readonly CarDbContext cdb;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogService"/>.
        /// </summary>
        /// <param name="cdb">The database context.</param>
        public LogService(CarDbContext cdb)
        {
            this.cdb = cdb;
        }

        /// <summary>
        /// Generates a log message for a new user creation event.
        /// </summary>
        /// <param name="user">The newly created user object.</param>
        /// <returns>A formatted string detailing the new user's properties, or null if the user object is null.</returns>
        public string UserCreateLogger(RegisteredUser user)
        {
            if (user == null)
                return null;

            var detailsBuilder = new StringBuilder();
            detailsBuilder.AppendLine("User created with:");
            detailsBuilder.AppendLine($"ID: {user.UserId}");
            detailsBuilder.AppendLine($"Username: {user.Username}");
            detailsBuilder.AppendLine($"Email: {user.Mail}");
            detailsBuilder.AppendLine($"Phone: {user.Phone}");
            detailsBuilder.AppendLine($"Location: {user.UserLocation}");

            return detailsBuilder.ToString();
        }

        /// <summary>
        /// Generates a log message comparing the old and new states of an updated user.
        /// </summary>
        /// <param name="oldUser">The original user entity before changes.</param>
        /// <param name="newUser">The DTO containing the updated user information.</param>
        /// <returns>A formatted string detailing the changes, or an empty string if no changes were made.</returns>
        public string UserEditLogger(RegisteredUser oldUser, UserUpdateDto newUser)
        {
            if (oldUser == null || newUser == null)
                return null;

            var detailsBuilder = new StringBuilder();

            if (oldUser.Mail != newUser.Mail)
                detailsBuilder.AppendLine($"Mail changed from {oldUser.Mail} to {newUser.Mail}");

            if (oldUser.Phone != newUser.Phone)
                detailsBuilder.AppendLine($"Phone changed from {oldUser.Phone} to {newUser.Phone}");

            if (oldUser.UserLocation != newUser.UserLocation)
                detailsBuilder.AppendLine($"User location changed from {oldUser.UserLocation} to {newUser.UserLocation}");

            if (!string.IsNullOrWhiteSpace(newUser.Password))
                detailsBuilder.AppendLine("Password changed");

            return detailsBuilder.ToString();
        }

        /// <summary>
        /// Generates a log message for a user activation or deactivation event.
        /// </summary>
        /// <param name="userId">The ID of the user whose status changed.</param>
        /// <param name="isActivated">A boolean indicating the new status (true for activated, false for deactivated).</param>
        /// <returns>A formatted string describing the status change.</returns>
        public string UserStatusLogger(int userId, bool isActivated)
        {
            return $"User #{userId} has been {(isActivated ? "activated" : "deactivated")}\n";
        }

        /// <summary>
        /// Generates a log message for a new car creation event.
        /// </summary>
        /// <param name="car">The newly created car object.</param>
        /// <returns>A formatted string detailing the new car's properties, or null if the car object is null.</returns>
        public string CarCreateLogger(ListedCar car)
        {
            if (car == null)
                return null;

            var cultureInfo = new CultureInfo("tr-TR");
            var detailsBuilder = new StringBuilder();

            detailsBuilder.AppendLine($"Car created with ID: {car.CarId}");
            detailsBuilder.AppendLine($"Brand: {car.Brand}");
            detailsBuilder.AppendLine($"Model: {car.Model}");
            detailsBuilder.AppendLine($"Year: {car.Year}");
            detailsBuilder.AppendLine($"KM: {car.KM?.ToString("N0", cultureInfo)}");
            detailsBuilder.AppendLine($"Fuel Type: {car.FuelType}");
            detailsBuilder.AppendLine($"Transmission: {car.Transmission}");
            detailsBuilder.AppendLine($"Price: {car.Price?.ToString("N0", cultureInfo)}");
            detailsBuilder.AppendLine($"Description: {car.Description}");

            return detailsBuilder.ToString();
        }

        /// <summary>
        /// Generates a log message comparing the old and new states of an updated car.
        /// </summary>
        /// <param name="oldCar">The original car entity before changes.</param>
        /// <param name="newCar">The DTO containing the updated car information.</param>
        /// <returns>A formatted string detailing the changes, or an empty string if no changes were made.</returns>
        public string CarEditLogger(ListedCar oldCar, CarUpdateDto newCar)
        {
            if (oldCar == null || newCar == null)
                return null;

            var cultureInfo = new CultureInfo("tr-TR");
            var detailsBuilder = new StringBuilder();

            if (oldCar.Brand != newCar.Brand)
                detailsBuilder.AppendLine($"Brand changed from {oldCar.Brand} to {newCar.Brand}");
            if (oldCar.Model != newCar.Model)
                detailsBuilder.AppendLine($"Model changed from {oldCar.Model} to {newCar.Model}");
            if (oldCar.KM != newCar.KM)
                detailsBuilder.AppendLine($"KM changed from {oldCar.KM?.ToString("N0", cultureInfo)} to {newCar.KM.ToString("N0", cultureInfo)}");
            if (oldCar.FuelType != newCar.FuelType)
                detailsBuilder.AppendLine($"Fuel type changed from {oldCar.FuelType} to {newCar.FuelType}");
            if (oldCar.Transmission != newCar.Transmission)
                detailsBuilder.AppendLine($"Transmission changed from {oldCar.Transmission} to {newCar.Transmission}");
            if (oldCar.Price != newCar.Price)
                detailsBuilder.AppendLine($"Price changed from {oldCar.Price?.ToString("N0", cultureInfo)} to {newCar.Price.ToString("N0", cultureInfo)}");
            if (oldCar.Description != newCar.Description)
            {
                if (string.IsNullOrWhiteSpace(oldCar.Description) && !string.IsNullOrEmpty(newCar.Description))
                    detailsBuilder.AppendLine($"Description added: {newCar.Description}");
                else if (string.IsNullOrWhiteSpace(newCar.Description))
                    detailsBuilder.AppendLine($"Description removed: {oldCar.Description}");
                else
                    detailsBuilder.AppendLine($"Description changed from {oldCar.Description} to {newCar.Description}");
            }

            return detailsBuilder.ToString();
        }

        /// <summary>
        /// Generates a log message for a car status change event (e.g., sold, removed).
        /// </summary>
        /// <param name="carId">The ID of the car whose status changed.</param>
        /// <param name="status">The new status of the car.</param>
        /// <returns>A formatted string describing the status change.</returns>
        public string CarStatusLogger(int carId, string status)
        {
            return $"Car #{carId} has been updated to {status}\n";
        }

        /// <summary>
        /// Generates a log message for an offer status change event (e.g., accepted, rejected).
        /// </summary>
        /// <param name="offerId">The ID of the offer whose status changed.</param>
        /// <param name="action">The action taken on the offer (e.g., "accepted").</param>
        /// <returns>A formatted string describing the status change.</returns>
        public string OfferUpdateLogger(int offerId, string action)
        {
            return $"Offer #{offerId} has been updated to {action}\n";
        }
    }
}