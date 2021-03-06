﻿using System;
using System.Collections.Generic;

namespace HA4IoT.Contracts.Services.Resources
{
    public interface IResourceService : IService
    {
        void RegisterType(Enum @enum);

        void RegisterText(Enum id, string value);

        string GetText(Enum id);

        string GetText(Enum id, params object[] formatParameterObjects);

        string GetText(Enum id, IDictionary<string, object> formatParameters);
    }
}
