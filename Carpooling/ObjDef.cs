using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carpooling
{
    public class Profile
    {
        [SQLite.AutoIncrement, SQLite.PrimaryKey]
        public int ID_P { get; set; }
        public string Jmbg { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Email { get; set; }
        public string Tel { get; set; }
    }

    public class Route
    {
        [SQLite.AutoIncrement, SQLite.PrimaryKey]
        public int ID_R { get; set; }
        public double Latitude_S { get; set; }
        public double Longitude_S { get; set; }
        public double Latitude_F { get; set; }
        public double Longitude_F { get; set; }
        public DateTime Termin { get; set; }
        public int Br_mesta { get; set; }
        public string Caption { get; set; }
        public int ID_DRIVER { get; set; }
    }

    public class Hitch
    {
        [SQLite.AutoIncrement, SQLite.PrimaryKey]
        public int ID_H { get; set; }
        public int ID_Route { get; set; }
        public int ID_Profil { get; set; }
    }

    public class Result
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class ID
    {
 
    }
}
