using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Profisys_Programming_Task.Model
{
    internal class Documents
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DocumentType Type { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string City { get; set; }
    }
}
