using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class Permission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Book))]
        public int BookIdFK { get; set; }

        public virtual Book Book { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public string UserIdFK { get; set; }

        public virtual CustomUser User { get; set; }

        [Required]
        [DefaultValue(false)]
        public bool Read { get; set; }

        [Required]
        [DefaultValue(false)]
        public bool Write { get; set; }

        public string IpAddress { get; set; }
    }
}
