﻿using System;
using Windows.Data.Json;
using HA4IoT.Contracts;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Core.Timer;
using HA4IoT.Networking;
using HA4IoT.Notifications;

namespace HA4IoT.Actuators
{
    public class MotionDetector : ActuatorBase, IMotionDetector
    {
        private TimedAction _autoEnableAction;
        private MotionDetectorState _state = MotionDetectorState.Idle;

        public MotionDetector(string id, IBinaryInput input, IHomeAutomationTimer timer, IHttpRequestController httpApiController, INotificationHandler notificationHandler)
            : base(id, httpApiController, notificationHandler)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            
            input.StateChanged += (s, e) => HandleInputStateChanged(e);

            IsEnabledChanged += (s, e) =>
            {
                HandleIsEnabledStateChanged(timer, notificationHandler);
            };
        }

        public event EventHandler MotionDetected;
        public event EventHandler DetectionCompleted;
        public event EventHandler<MotionDetectorStateChangedEventArgs> StateChanged;

        public MotionDetectorState GetState()
        {
            return _state;
        }

        public override void ApiGet(ApiRequestContext context)
        {
            base.ApiGet(context);

            context.Response.SetNamedValue("state", JsonValue.CreateStringValue(_state.ToString()));
        }

        public override void ApiPost(ApiRequestContext context)
        {
            base.ApiPost(context);
            
            if (context.Request.ContainsKey("action"))
            {
                string action = context.Request.GetNamedString("action");
                if (action.Equals("detected", StringComparison.OrdinalIgnoreCase))
                {
                    UpdateState(MotionDetectorState.MotionDetected);
                }
                else if (action.Equals("detectionCompleted", StringComparison.OrdinalIgnoreCase))
                {
                    UpdateState(MotionDetectorState.Idle);
                }
            }
        }

        private void HandleInputStateChanged(BinaryStateChangedEventArgs eventArgs)
        {
            // The relay at the motion detector is awlays held to high.
            // The signal is set to false if motion is detected.
            if (eventArgs.NewState == BinaryState.Low)
            {
                UpdateState(MotionDetectorState.MotionDetected);
            }
            else
            {
                UpdateState(MotionDetectorState.Idle);
            }
        }

        private void UpdateState(MotionDetectorState newState)
        {
            MotionDetectorState oldState = _state;
            _state = newState;

            if (!IsEnabled)
            {
                return;
            }

            if (newState == MotionDetectorState.MotionDetected)
            {
                NotificationHandler.PublishFrom(this, NotificationType.Info, "Motion detected at '{0}'.", Id);
                MotionDetected?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                NotificationHandler.PublishFrom(this, NotificationType.Info, "Detection completed at '{0}'.", Id);
                DetectionCompleted?.Invoke(this, EventArgs.Empty);
            }

            StateChanged?.Invoke(this, new MotionDetectorStateChangedEventArgs(oldState, newState));
        }

        private void HandleIsEnabledStateChanged(IHomeAutomationTimer timer, INotificationHandler notificationHandler)
        {
            if (!IsEnabled)
            {
                notificationHandler.PublishFrom(this, NotificationType.Info, "'{0}' disabled for 1 hour.", Id);
                _autoEnableAction = timer.In(TimeSpan.FromHours(1)).Do(() => IsEnabled = true);
            }
            else
            {
                _autoEnableAction?.Cancel();
            }
        }
    }
}