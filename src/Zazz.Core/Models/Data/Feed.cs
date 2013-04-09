﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Zazz.Core.Models.Data
{
    public class Feed : BaseEntity
    {
        public Feed()
        {
            FeedPhotoIds = new HashSet<FeedPhotoId>();
        }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public int UserId { get; set; }

        [ForeignKey("EventId")]
        public virtual ZazzEvent Event { get; set; }

        public int? EventId { get; set; }

        [ForeignKey("PhotoId")]
        public virtual Photo Photo { get; set; }

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }

        public int? PostId { get; set; }

        public int? PhotoId { get; set; }

        public DateTime Time { get; set; }

        public FeedType FeedType { get; set; }

        public virtual ICollection<FeedPhotoId> FeedPhotoIds { get; set; }
    }
}