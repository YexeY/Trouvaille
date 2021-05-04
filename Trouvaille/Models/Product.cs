﻿#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using AuthoDemoMVC.Models;

namespace Trouvaille_WebAPI.Models
{
    public class Product
    {
        [Key]
        public Guid ProductId { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        [Column(TypeName = "decimal(16,2)")]
        public decimal Price { get; set; }

        public Guid? ManufacturerId { get; set; }

        public Manufacturer? Manufacturer { get; set; }

        public int InStock { get; set; }

        public int MinStock { get; set; }

        [Column(TypeName = "decimal(16,2)")]
        public decimal Tax { get; set; }

        public Guid? PictureId { get; set; }

        public Picture? picture { get; set; }

        public ICollection<Category>? ProductCategories { get; set; }
    }
}
