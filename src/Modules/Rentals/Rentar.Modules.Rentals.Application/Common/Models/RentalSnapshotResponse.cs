using Rentar.Modules.Rentals.Domain;

namespace Rentar.Modules.Rentals.Application.Common.Models;

/// <summary>What a rental booked now keeps of its car, its customer and the car's daily price.</summary>
public sealed record RentalSnapshotResponse(RentalCarSnapshot Car, RentalCustomerSnapshot Customer, decimal DailyPrice);
