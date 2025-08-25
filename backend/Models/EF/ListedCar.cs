using CARDB_EF.Models.EF;
using System.ComponentModel.DataAnnotations;
using System;

public class ListedCar
{
    [Key] public int? CarId { get; set; }
    public int? OwnerId { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public int? Year { get; set; }
    public int? KM { get; set; }
    public string FuelType { get; set; }
    public string Transmission { get; set; }
    public decimal? Price { get; set; }
    public string Description { get; set; }
    public DateTime? RecordTime { get; set; }
    public string CarStatus { get; set; }

    public virtual RegisteredUser Owner { get; set; }
}