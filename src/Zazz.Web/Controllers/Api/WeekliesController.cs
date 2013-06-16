﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Web.Http;
using Zazz.Core.Exceptions;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;
using Zazz.Web.Filters;
using Zazz.Web.Interfaces;
using Zazz.Web.Models.Api;

namespace Zazz.Web.Controllers.Api
{
    [HMACAuthorize]
    public class WeekliesController : BaseApiController
    {
        private readonly IWeeklyService _weeklyService;
        private readonly IObjectMapper _objectMapper;

        public WeekliesController(IWeeklyService weeklyService, IObjectMapper objectMapper)
        {
            _weeklyService = weeklyService;
            _objectMapper = objectMapper;
        }

        // GET api/v1/weeklies/5
        public ApiWeekly Get(int id)
        {
            if (id == 0)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            try
            {
                var weekly = _weeklyService.GetWeekly(id);
                return _objectMapper.WeeklyToApiWeekly(weekly);
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }

        // POST api/v1/weeklies
        public ApiWeekly Post(ApiWeekly weekly)
        {
            var userId = ExtractUserIdFromHeader();
            var w = new Weekly
                    {
                        DayOfTheWeek = weekly.DayOfTheWeek,
                        Description = weekly.Description,
                        Name = weekly.Name,
                        UserId = userId,
                        PhotoId = weekly.PhotoId
                    };

            try
            {
                _weeklyService.CreateWeekly(w);
                return _objectMapper.WeeklyToApiWeekly(w);
            }
            catch (InvalidOperationException)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
            catch (WeekliesLimitReachedException)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }
        }

        // Put api/v1/weeklies/5
        public void Put(int id, ApiWeekly weekly)
        {
            if (id == 0 || weekly == null)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            try
            {
                var userId = ExtractUserIdFromHeader();

                var w = new Weekly
                        {
                            Id = id,
                            DayOfTheWeek = weekly.DayOfTheWeek,
                            Description = weekly.Description,
                            Name = weekly.Name,
                            PhotoId = weekly.PhotoId
                        };

                _weeklyService.EditWeekly(w, userId);
            }
            catch (NotFoundException)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            catch (SecurityException)
            {
                throw new HttpResponseException(HttpStatusCode.Forbidden);
            }
        }

        // Delete api/v1/weeklies/5
        public void Delete(int id)
        {
            if (id == 0)
                throw new HttpResponseException(HttpStatusCode.BadRequest);

            throw new NotImplementedException();
        }
    }
}
