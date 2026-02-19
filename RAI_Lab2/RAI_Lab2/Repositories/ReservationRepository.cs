using System.Collections.Concurrent;
using RAI_Lab2.Interfaces;
using RAI_Lab2.Models;

namespace RAI_Lab2.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly ConcurrentDictionary<int, Room> _rooms = new();
        private readonly ConcurrentDictionary<int, Reservation> _reservations = new();
        private readonly ConcurrentDictionary<string, User> _users = new();

        private int _nextRoomId = 1;
        private int _nextReservationId = 1;

        private readonly object _reservationLock = new object();

        public ReservationRepository()
        {
            InitData();
        }

        public void InitData()
        {
            _rooms.Clear();
            _reservations.Clear();
            _users.Clear();
            _nextRoomId = 1;
            _nextReservationId = 1;

            _users.TryAdd("admin", new User { login = "admin", isAdmin = true }); 
            _users.TryAdd("user1", new User { login = "user1", isAdmin = false });
            _users.TryAdd("user2", new User { login = "user2", isAdmin = false });

            AddRoomInternal(new Room { name = "Kreatywna", size = 10 }); 
            AddRoomInternal(new Room { name = "Dynamiczna", size = 6 });
            AddRoomInternal(new Room { name = "Focus", size = 4 });
            AddRoomInternal(new Room { name = "Agora", size = 20 });
        }

        public User GetUserByLogin(string login)
        {
            _users.TryGetValue(login, out var user);
            return user;
        }

        public IEnumerable<User> GetAllUsers() => _users.Values.ToList();

        public IEnumerable<Room> GetAllRooms() => _rooms.Values.OrderBy(s => s.name).ToList();

        public Room GetRoomById(int id)
        {
            _rooms.TryGetValue(id, out var room);
            return room;
        }

        private void AddRoomInternal(Room room)
        {
            room.id = Interlocked.Increment(ref _nextRoomId);
            _rooms.TryAdd(room.id, room);
        }

        public void AddRoom(Room room)
        {
            AddRoomInternal(room);
        }

        public void DeleteRoom(int id)
        {
            lock (_reservationLock)
            {
                _rooms.TryRemove(id, out _);

                var reservationsToDelete = _reservations.Values
                .Where(r => r.roomId == id)
                .ToList();

                foreach (var reservation in reservationsToDelete)
                {
                    _reservations.TryRemove(reservation.Id, out _);
                }
            }
        }

        public IEnumerable<Reservation> GetReservationsForDay(DateTime day)
        {
            lock (_reservationLock)
            {
                var startOfDay = day.Date;
                var endOfDay = startOfDay.AddDays(1);

                return _reservations.Values
                    .Where(r => (r.startTime < endOfDay && r.endTime > startOfDay))
                    .Select(r =>
                    {
                        r.room = GetRoomById(r.roomId);
                        return r;
                    })
                    .ToList();
            }
        }

        public IEnumerable<Reservation> GetUpcomingReservationsForUser(string login)
        {
            return _reservations.Values
                .Where(r => r.login == login && r.endTime > DateTime.Now)
                .OrderBy(r => r.startTime)
                .Select(r => {
                    r.room = GetRoomById(r.roomId);
                    return r;
                })
                .ToList();
        }

        public bool CancelReservation(int rezerwacjaId, string login)
        {
            if (_reservations.TryGetValue(rezerwacjaId, out var rezerwacja))
            {
                if (rezerwacja.login == login)
                {
                    return _reservations.TryRemove(rezerwacjaId, out _);
                }
            }
            return false;
        }

        public (bool Success, string ErrorMessage) CreateReservation(Reservation reservation)
        {
            lock (_reservationLock)
            {
                if (GetRoomById(reservation.roomId) == null)
                {
                    return (false, "Wybrana sala nie istnieje.");
                }

                bool collides = _reservations.Values
                    .Any(r =>
                        r.roomId == reservation.roomId &&
                        r.startTime < reservation.endTime &&
                        r.endTime > reservation.startTime
                    );

                if (collides)
                {
                    return (false, "Termin jest już zajęty.");
                }

                reservation.Id = Interlocked.Increment(ref _nextReservationId);
                _reservations.TryAdd(reservation.Id, reservation);
                return (true, null);
            }
        }
    }
}
