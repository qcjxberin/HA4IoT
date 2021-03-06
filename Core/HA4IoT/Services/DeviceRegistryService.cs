﻿using System;
using System.Collections.Generic;
using System.Linq;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Contracts.Services;
using HA4IoT.Contracts.Services.System;

namespace HA4IoT.Services
{
    public class DeviceRegistryService : ServiceBase, IDeviceRegistryService
    {
        private readonly Dictionary<string, IDevice> _devices = new Dictionary<string, IDevice>();

        public DeviceRegistryService(
            ISystemEventsService systemEventsService,
            ISystemInformationService systemInformationService)
        {
            if (systemEventsService == null) throw new ArgumentNullException(nameof(systemEventsService));
            if (systemInformationService == null) throw new ArgumentNullException(nameof(systemInformationService));

            systemEventsService.StartupCompleted += (s, e) =>
            {
                systemInformationService.Set("Devices/Count", _devices.Count);
            };
        }

        public void AddDevice(IDevice device)
        {
            _devices.Add(device.Id, device);
        }

        public TDevice GetDevice<TDevice>(string id) where TDevice : IDevice
        {
            return (TDevice)_devices[id];
        }

        public TDevice GetDevice<TDevice>() where TDevice : IDevice
        {
            return _devices.Values.OfType<TDevice>().SingleOrDefault();
        }

        public IList<TDevice> GetDevices<TDevice>() where TDevice : IDevice
        {
            return _devices.Values.OfType<TDevice>().ToList();
        }

        public IList<IDevice> GetDevices()
        {
            return _devices.Values.ToList();
        }
    }
}
