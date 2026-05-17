namespace Warehouse.Models
{
    public class WarehouseItem
    {
        public required string Name { get; set; }
        public required string SKU { get; set; }
        public required string Category { get; set; }
        public int Quantity { get; set; }
    }
}