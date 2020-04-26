using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AuthenticationSampleWebApp.DTOs
{
    public class EnrollStudentReq
    {
        [RegularExpression("^s[0-9]+$")]
        public string IndexNumber { get; set; }
        [Required(ErrorMessage = "Musisz podać imię!")]
        [MaxLength(100)]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Musisz podać nazwisko!")]
        [MaxLength(255)]
        public string LastName { get; set; }

        [Required]
        public string Studies { get; set; }
    }
}
