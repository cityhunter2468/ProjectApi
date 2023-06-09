﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectApi.Models
{
    public class LoginModel
    {
        [Required]
        [MaxLength(30)]
        public string UserName { get; set; }
        [Required]
        [MaxLength(30)]
        public string Password { get; set; }
    }
}