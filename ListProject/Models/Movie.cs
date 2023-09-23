namespace ListProject.Models
{
    public class Movie : EntityBase
    {
        public byte[] ImageData { get; set; }
        public string Name { get; set; }
        public int Year { get; set; }
        public int Length { get; set; }
        public double Score { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
}
