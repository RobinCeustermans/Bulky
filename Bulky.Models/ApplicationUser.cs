﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Bulky.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }

        //fk
        public int? CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        [ValidateNever]
        public Company? Company { get; set; } //also make this nullable if CompanyId is nullable

        //no property in db
        [NotMapped]
        public string Role { get; set; }

    }
}
