namespace SampleInventory.Dtos
{
    public class WarehouseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int? ParentWarehouseId { get; set; }
        public string ParentWarehouseName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public List<WarehouseDto> Children { get; set; } = new List<WarehouseDto>();
    }
}
