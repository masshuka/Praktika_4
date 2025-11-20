using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Program
    {
        public static List<User> Users = new List<User>();
        public static IPAddress IpAddress;
        public static int Port;

        public static bool AutorizationUser(string login, string password, out int userId)
        {
            userId = -1;
            User user = Users.Find(x => x.login == login && x.password == password);
            if (user != null)
            {
                userId = user.id;
                return true;
            }
            return false;
        }
    }
}
