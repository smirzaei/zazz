﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Web.Filters;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    public class LoginController : ApiController
    {
        private readonly IUserService _userService;
        private readonly ICryptoService _cryptoService;
        private readonly IApiAppRepository _apiAppRepository;

        public LoginController(IUserService userService, ICryptoService cryptoService,
            IApiAppRepository apiAppRepository)
        {
            _userService = userService;
            _cryptoService = cryptoService;
            _apiAppRepository = apiAppRepository;
        }

        [HMACAuthorize(IgnoreUserIdAndPassword = true)]
        public LoginApiResponse Post(LoginApiRequest request)
        {
            var user = _userService.GetUser(request.Username, true, true);
            if (user == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var clearPassword = _cryptoService.DecryptPassword(user.Password, user.PasswordIV);
            var app = _apiAppRepository.GetById(request.AppId);

            var hashCheck = _cryptoService.GenerateHMACSHA512Hash(Encoding.UTF8.GetBytes(clearPassword),
                                                                  app.PasswordSigningKey);

            if (request.Password != hashCheck)
                throw new HttpResponseException(HttpStatusCode.Unauthorized);

            return null;
        }
    }
}
