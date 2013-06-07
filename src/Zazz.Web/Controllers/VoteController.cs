﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Zazz.Core.Interfaces;

namespace Zazz.Web.Controllers
{
    [Authorize]
    public class VoteController : Controller
    {
        private readonly IVoteService _voteService;
        private readonly IUserService _userService;

        public VoteController(IVoteService voteService, IUserService userService)
        {
            _voteService = voteService;
            _userService = userService;
        }

        public bool Exists(int id)
        {
            var userId = _userService.GetUserId(User.Identity.Name);
            return _voteService.HasUserVotedOnPhoto(id, userId);
        }
    }
}
