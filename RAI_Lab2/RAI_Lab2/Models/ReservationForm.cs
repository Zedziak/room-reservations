using System.ComponentModel.DataAnnotations;

namespace RAI_Lab2.Models
{
    public class ReservationForm
    {
        [Required]
        public int roomId { get; set; }

        [Required(ErrorMessage = "Data rozpoczęcia jest wymagana.")] 
        public DateTime startTime { get; set; }

        [Required(ErrorMessage = "Data zakończenia jest wymagana.")] 
        public DateTime endTime { get; set; }
    }
}
