using System.ComponentModel.DataAnnotations;

namespace RAI_Lab2.Models
{
    public class ManageRooms
    {
        public IEnumerable<Room>? existingRooms { get; set; }

        [Required(ErrorMessage = "Nazwa salki jest wymagana")]
        [Display(Name = "Nazwa nowej salki")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Nazwa musi mieć od 3 do 100 znaków")]
        public string newRoomName { get; set; } = null!;

        [Required(ErrorMessage = "Pojemność jest wymagana")]
        [Range(1, 100, ErrorMessage = "Pojemność musi być liczbą od 1 do 100")]
        [Display(Name = "Pojemność")]
        public int newRoomSize { get; set; }
    }
}
