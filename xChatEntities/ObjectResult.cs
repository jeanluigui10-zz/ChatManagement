﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xChatEntities
{
    /// <summary>
    /// Objeto que devuelve el resultado de una operación.
    /// </summary>
    public class ObjectResult
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}
