using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Profisys_Programming_Task.Model
{
    internal class DocumentItems
    {
        [Key]
        public int Id { get; set; }
        //foreign key
        [Required]
        public int DocumentId { get; set; }
        [Required]
        public int Ordinal { get; set; }
        [Required]
        public string Product { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public double Price { get; set; }
        [Required]
        public int TaxRate { get; set; }

        public Documents Document { get; set; }
    }
}
