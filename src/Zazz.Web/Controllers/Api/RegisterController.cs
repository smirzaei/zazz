﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Core.Models.Data.Enums;
using Zazz.Web.Filters;
using Zazz.Web.Models.Api;
using Zazz.Web.OAuthAuthorizationServer;

namespace Zazz.Web.Controllers.Api
{
    public class RegisterController : ApiController
    {
        private readonly IAuthService _authService;
        private readonly IOAuthClientRepository _oauthClientRepository;

        public RegisterController(IAuthService authService, IOAuthClientRepository oauthClientRepository)
        {
            _authService = authService;
            _oauthClientRepository = oauthClientRepository;
        }

        public OAuthAccessTokenResponse Post(ApiRegister request)
        {
            if (request == null ||
                String.IsNullOrWhiteSpace(request.Username) ||
                String.IsNullOrWhiteSpace(request.Password) ||
                String.IsNullOrWhiteSpace(request.Email)
                )
            {
                throw new OAuthException(OAuthError.InvalidRequest);
            }

            if (request.AccountType == AccountType.Club && String.IsNullOrWhiteSpace(request.ClubName))
            {
                throw new OAuthException(OAuthError.InvalidRequest);
            }

            //authorizing client
            if (Request.Headers.Authorization == null ||
                String.IsNullOrWhiteSpace(Request.Headers.Authorization.Parameter))
                throw new OAuthException(OAuthError.InvalidClient);

            var clientId = Request.Headers.Authorization.Parameter;

            var client = _oauthClientRepository.GetById(clientId);
            if (client == null)
                throw new OAuthException(OAuthError.InvalidClient);

            User user;
            if (request.AccountType == AccountType.User)
            {
                user = new User
                       {
                           AccountType = AccountType.User,
                           Email = request.Email,
                           IsConfirmed = false,
                           JoinedDate = DateTime.UtcNow,
                           LastActivity = DateTime.UtcNow,
                           Username = request.Username,
                           UserDetail = new UserDetail
                                        {
                                            FullName = request.FullName,
                                            Gender = request.Gender
                                        }
                       };
            }
            else
            {
                user = new User
                       {
                           AccountType = AccountType.Club,
                           Email = request.Email,
                           IsConfirmed = false,
                           JoinedDate = DateTime.UtcNow,
                           LastActivity = DateTime.UtcNow,
                           Username = request.Username,
                           ClubDetail = new ClubDetail
                                        {
                                            Address = request.ClubAddress,
                                            ClubName = request.ClubName,
                                            ClubType = request.ClubType
                                        }
                       };
            }

            try
            {
                var u = _authService.Register(user, request.Password, true);
                return null;
            }
            catch (InvalidEmailException)
            {
                throw new OAuthException(OAuthError.InvalidRequest);
            }
            catch (PasswordTooLongException)
            {
                throw new OAuthException(OAuthError.InvalidRequest);
            }
            catch (UsernameExistsException)
            {
                throw new OAuthException(OAuthError.InvalidRequest);
            }
        }
    }
}
