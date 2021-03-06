﻿using Serenity.Data;
using Newtonsoft.Json;
using System;

namespace Serenity.Services
{
    public class UpdateLocalizationRequest<TEntity> : SaveRequest<TEntity>
    {
        public Int32? CultureId { get; set; }
    }
}