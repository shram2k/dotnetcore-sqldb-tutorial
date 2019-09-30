using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreSqlDb.Models
{
    public class Todo
    {
        public int ID { get; set; }
        public string Description { get; set; }

        [Display(Name = "Created Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime CreatedDate { get; set; }

    }


    public class TodoModel
    {
        public int ID { get; set; }
        public string Description { get; set; }

        [Display(Name = "Created Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime CreatedDate { get; set; }

        [Required]
        [Display(Name = "Upload Pic")]
        public IFormFile UploadPic { get; set; }
    }
}

