namespace RAI_Lab2.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int roomId { get; set; }
        public string login { get; set; }
        public DateTime startTime { get; set; } 
        public DateTime endTime { get; set; } 

        public Room room { get; set; } = null!;
    }
}
