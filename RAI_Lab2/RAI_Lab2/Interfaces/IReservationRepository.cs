using RAI_Lab2.Models;

namespace RAI_Lab2.Interfaces
{
    public interface IReservationRepository
    {
        void InitData();
        User GetUserByLogin(string login);
        IEnumerable<User> GetAllUsers();

        IEnumerable<Room> GetAllRooms();
        Room GetRoomById(int id);
        void AddRoom(Room room);
        void DeleteRoom(int id);

        IEnumerable<Reservation> GetReservationsForDay(DateTime day);
        IEnumerable<Reservation> GetUpcomingReservationsForUser(string login); 

        (bool Success, string ErrorMessage) CreateReservation(Reservation reservation);
        bool CancelReservation(int reservationId, string login);
    }
}
