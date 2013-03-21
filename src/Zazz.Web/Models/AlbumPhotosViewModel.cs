﻿using System.ComponentModel.DataAnnotations;
using PagedList;

namespace Zazz.Web.Models
{
    public class AlbumPhotosViewModel
    {
        public bool IsOwner { get; set; }

        [StringLength(250)]
        public string Description { get; set; }

        public int AlbumId { get; set; }

        public int UserId { get; set; }

        public string AlbumName { get; set; }

        public IPagedList<PhotoViewModel> Photos { get; set; }
    }
}