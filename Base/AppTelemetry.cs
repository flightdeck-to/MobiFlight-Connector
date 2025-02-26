﻿using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MobiFlight.Base
{
    public sealed class AppTelemetry
    {
        private static readonly AppTelemetry client = new AppTelemetry();
        private Microsoft.ApplicationInsights.TelemetryClient telemetryClient;
        private bool enabled;

        public bool Enabled
        {
            get { return enabled; }
            set { if (enabled != value)
                {
                    enabled = value;
                    InitTelemetryClient();
                }
            }
        }

        AppTelemetry() {           
        }

        private void InitTelemetryClient()
        {
            TelemetryConfiguration configuration = TelemetryConfiguration.Active;
            configuration.InstrumentationKey = "712d6bd9-733d-4735-b173-ba30ade778fb";
            configuration.DisableTelemetry = !enabled;
            telemetryClient = new Microsoft.ApplicationInsights.TelemetryClient(configuration);
            telemetryClient.Context.Component.Version = Assembly.GetEntryAssembly().GetName().Version.ToString();
            telemetryClient.Context.Session.Id = Guid.NewGuid().ToString();
            telemetryClient.Context.User.Id = (Environment.UserName + Environment.MachineName).GetHashCode().ToString();
            telemetryClient.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
        }

        public static AppTelemetry Instance
        {
            get
            {
                return client;
            }
        }

        public Microsoft.ApplicationInsights.TelemetryClient GetClient ()
        {
            if (telemetryClient == null) InitTelemetryClient();
            return telemetryClient;
        }

        public void TrackStart()
        {
            EventTelemetry trackingEvent = new EventTelemetry("Started");
            GetClient().TrackEvent(trackingEvent);
        }

        public void ConfigLoaded(ConfigFile configFile)
        {
            // Track config loaded event
            EventTelemetry trackingEvent = new EventTelemetry("ConfigLoaded");
            List<OutputConfigItem> outputConfigs = configFile.GetOutputConfigItems();
            List<InputConfigItem> inputConfigs = configFile.GetInputConfigItems();

            foreach (OutputConfigItem item in outputConfigs)
            {
                String key = "output." + item.DisplayType;
                if (!trackingEvent.Metrics.ContainsKey(key)) trackingEvent.Metrics[key] = 0;
                trackingEvent.Metrics[key] += 1;

                key = "output." + item.SourceType;
                if (!trackingEvent.Metrics.ContainsKey(key)) trackingEvent.Metrics[key] = 0;
                trackingEvent.Metrics[key] += 1;
            }

            foreach (InputConfigItem item in inputConfigs)
            {
                String key = "input." + item.Type;
                if (!trackingEvent.Metrics.ContainsKey(key)) trackingEvent.Metrics[key] = 0;
                trackingEvent.Metrics[key] += 1;

                foreach(String itemKey in item.GetStatistics().Keys)
                {
                    if (!trackingEvent.Metrics.ContainsKey(itemKey)) trackingEvent.Metrics[itemKey] = 0;
                    trackingEvent.Metrics[itemKey] += item.GetStatistics()[itemKey];
                }
            }

            trackingEvent.Metrics["outputs"] = outputConfigs.Count();
            trackingEvent.Metrics["inputs"] = inputConfigs.Count();
            GetClient().TrackEvent(trackingEvent);
        }

        public void TrackBoardStatistics(ExecutionManager execManager)
        {
            EventTelemetry trackingEvent = new EventTelemetry("BoardStatistics");
            foreach (String key in execManager.GetStatistics().Keys)
            {
                trackingEvent.Metrics[key] = execManager.GetStatistics()[key];
            }
            GetClient().TrackEvent(trackingEvent);
        }

        internal void TrackSettings()
        {
            EventTelemetry trackingEvent = new EventTelemetry("Settings");
            trackingEvent.Metrics["Settings.BetaUpdates"] = Properties.Settings.Default.BetaUpdates ? 1 : 0;
            foreach (String key in Log.Instance.GetStatistics().Keys)
            {
                trackingEvent.Metrics["Settings." + key] = Log.Instance.GetStatistics()[key];
            }
            GetClient().TrackEvent(trackingEvent);
        }

        public void TrackShutdown()
        {
            EventTelemetry trackingEvent = new EventTelemetry("Shutdown");
            GetClient().TrackEvent(trackingEvent);
            GetClient().Flush();
        }

        public void TrackFlightSimConnected(String SimName, String ConnectionType)
        {
            EventTelemetry trackingEvent = new EventTelemetry("FlightSimConnected");
            trackingEvent.Metrics["FlightSim." + SimName] = 1;
            trackingEvent.Properties["FlightSim"] = SimName;
            trackingEvent.Properties["FlightSimConnection"] = ConnectionType;
            GetClient().TrackEvent(trackingEvent);
        }
    }

    public class LogAppenderInsights : ILogAppender
    {        
        public LogAppenderInsights()
        {   }

        public void log(string message, LogSeverity severity)
        {
            String msg = DateTime.Now + "(" + DateTime.Now.Millisecond + ")" + ": " + message;
            
            EventTelemetry myevent = new EventTelemetry();
            myevent.Name = "log";
            myevent.Properties.Add("message", msg);
            myevent.Properties.Add("severity", severity.ToString());
            AppTelemetry.Instance.GetClient().TrackEvent(myevent);                        
        }
    }
}
