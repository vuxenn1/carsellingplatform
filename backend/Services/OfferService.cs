using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

using CARDB_EF.Data;
using CARDB_EF.Models.EF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CARDB_EF.Services
{
    /// <summary>
    /// Manages business logic for creating, processing, and retrieving offers for cars.
    /// </summary>
    public class OfferService
    {
        private readonly CarDbContext cdb;
        private readonly LogService logger;
        private readonly CarService carService;
        private readonly NotificationService notificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="OfferService"/>.
        /// </summary>
        /// <param name="cdb">The database context.</param>
        /// <param name="logger">The service for creating log messages.</param>
        /// <param name="carService">The service for managing car statuses.</param>
        /// <param name="notificationService">The service for sending user notifications.</param>
        public OfferService(CarDbContext cdb, LogService logger, CarService carService, NotificationService notificationService)
        {
            this.cdb = cdb;
            this.logger = logger;
            this.carService = carService;
            this.notificationService = notificationService;
        }

        /// <summary>
        /// Creates a new offer for a car asynchronously.
        /// </summary>
        /// <param name="offer">The offer object to be created. Must include CarId, SenderId, ReceiverId, and OfferPrice.</param>
        /// <returns>A tuple indicating the success status and a descriptive message.</returns>
        public async Task<(bool Success, string Message)> CreateOfferAsync(Offer offer)
        {
            if (offer?.CarId == null || offer.SenderId == null || offer.ReceiverId == null || offer.OfferPrice == null)
                return (false, "Offer information is incomplete. Please provide all required fields.");

            offer.OfferTime = DateTime.UtcNow;
            offer.OfferStatus = "pending";

            await cdb.Offers.AddAsync(offer);
            await cdb.SaveChangesAsync();

            var cultureInfo = new System.Globalization.CultureInfo("tr-TR");
            string logMessage = $"User #{offer.SenderId} offered {offer.OfferPrice?.ToString("N0", cultureInfo)} for Car #{offer.CarId}.";

            var logToUpdate = await cdb.OfferLogs
                                       .Where(l => l.OfferId == offer.OfferId && l.ActionType == "INSERT")
                                       .OrderByDescending(l => l.LogId)
                                       .FirstOrDefaultAsync();

            if (logToUpdate != null)
                logToUpdate.ActionDetails = logMessage;
                await cdb.SaveChangesAsync();

            return (true, $"Offer created successfully with ID: {offer.OfferId}");
        }

        /// <summary>
        /// Accepts a pending offer, marks the car as sold, and rejects all other pending offers for that car.
        /// </summary>
        /// <param name="offerId">The ID of the offer to accept.</param>
        /// <returns>A tuple indicating the success status and a descriptive message.</returns>
        public async Task<(bool Success, string Message)> AcceptOfferAsync(int offerId)
        {
            var offerToAccept = await cdb.Offers.FirstOrDefaultAsync(o => o.OfferId == offerId && o.OfferStatus == "pending");

            if (offerToAccept == null)
                return (false, "Pending offer not found or it has already been processed.");

            await carService.MarkCarSoldAsync((int)offerToAccept.CarId);
            offerToAccept.OfferStatus = "accepted";

            var otherOffers = await cdb.Offers
                .Where(o => o.CarId == offerToAccept.CarId && o.OfferId != offerId && o.OfferStatus == "pending")
                .ToListAsync();

            foreach (var otherOffer in otherOffers)
            {
                otherOffer.OfferStatus = "rejected";
                var rejectionMessage = "Your offer for a car was automatically rejected because the owner accepted another offer.";
                await notificationService.SendNotificationAsync(new Notification { ReceiverId = otherOffer.SenderId, MessageText = rejectionMessage });
            }

            await cdb.SaveChangesAsync();

            string logMessage = logger.OfferUpdateLogger(offerId, "accepted");
            var logToUpdate = await cdb.OfferLogs
                                        .Where(l => l.OfferId == offerId && l.ActionType == "UPDATE")
                                        .OrderByDescending(l => l.LogId)
                                        .FirstOrDefaultAsync();
            if (logToUpdate != null)
            {
                logToUpdate.ActionDetails = logMessage;
                await cdb.SaveChangesAsync();
            }

            return (true, $"Offer {offerId} accepted successfully.");
        }

        /// <summary>
        /// Rejects a pending offer asynchronously.
        /// </summary>
        /// <param name="offerId">The ID of the offer to reject.</param>
        /// <returns>A tuple indicating the success status and a descriptive message.</returns>
        public async Task<(bool Success, string Message)> RejectOfferAsync(int offerId)
        {
            var offer = await cdb.Offers.FirstOrDefaultAsync(o => o.OfferId == offerId && o.OfferStatus == "pending");

            if (offer == null)
                return (false, "Pending offer not found or it has already been processed.");

            offer.OfferStatus = "rejected";
            await cdb.SaveChangesAsync();

            string logMessage = logger.OfferUpdateLogger(offerId, "rejected");
            var logToUpdate = await cdb.OfferLogs
                                       .Where(l => l.OfferId == offerId && l.ActionType == "UPDATE")
                                       .OrderByDescending(l => l.LogId)
                                       .FirstOrDefaultAsync();

            if (logToUpdate != null)
            {
                logToUpdate.ActionDetails = logMessage;
                await cdb.SaveChangesAsync();
            }

            return (true, $"Offer {offerId} rejected successfully.");
        }

        /// <summary>
        /// Retrieves all offers sent by a specific user, ordered by the most recent.
        /// </summary>
        /// <param name="userId">The ID of the user who sent the offers.</param>
        /// <returns>A list of <see cref="Offer"/> objects. Returns an empty list if no offers are found. Throws an exception on database error.</returns>
        public async Task<List<Offer>> GetSentOffersAsync(int userId)
        {
            return await cdb.Offers
                .Where(o => o.SenderId == userId)
                .OrderByDescending(o => o.OfferTime)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves all offers received by a specific user, ordered by the most recent.
        /// </summary>
        /// <param name="userId">The ID of the user who received the offers.</param>
        /// <returns>A list of <see cref="Offer"/> objects. Returns an empty list if no offers are found. Throws an exception on database error.</returns>
        public async Task<List<Offer>> GetReceivedOffersAsync(int userId)
        {
            return await cdb.Offers
                .Where(o => o.ReceiverId == userId)
                .OrderByDescending(o => o.OfferTime)
                .ToListAsync();
        }

        /// <summary>
        /// Gets the count of pending offers received by a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user who received the offers.</param>
        /// <returns>The total number of pending offers. Throws an exception on database error.</returns>
        public async Task<int> GetPendingReceivedOfferCountAsync(int userId)
        {
            return await cdb.Offers.CountAsync(o => o.ReceiverId == userId && o.OfferStatus == "pending");
        }
    }
}