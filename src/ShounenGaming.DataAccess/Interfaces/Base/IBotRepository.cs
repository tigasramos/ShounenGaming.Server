﻿using ShounenGaming.Core.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.DataAccess.Interfaces.Base
{
    public interface IBotRepository : IBaseRepository<Bot>
    {
        Task<Bot?> GetBotByDiscordId(string discordId);
        Task<Bot?> GetBotByRefreshToken(string refreshToken);
    }
}