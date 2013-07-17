﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Zazz.Core.Interfaces;
using Zazz.Core.Models.Data;

namespace Zazz.Web.Controllers.Api
{
    public class TagsController : ApiController
    {
        private readonly IStaticDataRepository _staticDataRepository;

        public TagsController(IStaticDataRepository staticDataRepository)
        {
            _staticDataRepository = staticDataRepository;
        }

        // GET api/v1/tags
        public IEnumerable<Category> Get()
        {
            return _staticDataRepository.GetCategories();
        }

        // GET api/v1/tags/5
        public Category Get(int id)
        {
            return _staticDataRepository.GetCategories()
                                        .SingleOrDefault(t => t.Id == id);
        }
    }
}