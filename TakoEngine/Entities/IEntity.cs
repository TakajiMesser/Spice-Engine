﻿using OpenTK;
using System.Collections.Generic;
using TakoEngine.Scripting.StimResponse;

namespace TakoEngine.Entities
{
    public interface IEntity
    {
        int ID { get; set; }
        Vector3 Position { get; set; }
    }
}
