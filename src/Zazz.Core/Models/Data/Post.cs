﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class Post : BaseEntity
    {
        [ForeignKey("UserId")]
        public User User { get; set; }

        public int UserId { get; set; }

        public string Message { get; set; }

        [MaxLength(100), DataType("varchar")]
        public string FacebookId { get; set; }

        public DateTime CreatedTime { get; set; }
    }
}