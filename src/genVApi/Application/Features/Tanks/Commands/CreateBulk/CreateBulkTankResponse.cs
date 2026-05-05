namespace Application.Features.Tanks.Commands.CreateBulk;

public class CreateBulkTankResponse
{
    public Guid TankId { get; set; }
    public string TankName { get; set; }
    public int TotalRacks { get; set; }
    public int TotalSlots { get; set; }
    public int TotalBoxes { get; set; }
    public int TotalCells { get; set; }
}
