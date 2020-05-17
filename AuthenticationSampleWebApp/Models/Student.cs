using System;

namespace AuthenticationSampleWebApp.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        // public String Haslo { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
