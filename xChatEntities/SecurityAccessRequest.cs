﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xChatEntities
{
    public class SecurityAccessRequestEntity
    {
        public string Nombre { get; set; }
        public string EMail { get; set; }
        public string Token { get; set; }
        public int UserId { get; set; }
    }
}
