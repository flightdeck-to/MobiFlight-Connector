﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MobiFlight.InputConfig
{
    class RetriggerInputAction : InputAction
    {
        public new const String Label = "MobiFlight - Retrigger Switches";
        public const String TYPE = "RetriggerInputAction";
        DateTime lastExecution = DateTime.Now;

        public override object Clone()
        {
            RetriggerInputAction clone = new RetriggerInputAction();
         
            return clone;
        }

        public override void ReadXml(System.Xml.XmlReader reader)
        {
            // Nothing to do
        }

        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("type", TYPE);
        }

        public override void execute(
            FSUIPC.FSUIPCCacheInterface fsuipcCache,
            SimConnectMSFS.SimConnectCacheInterface simConnectCache,
            MobiFlightCacheInterface moduleCache,
            InputEventArgs args,
            List<ConfigRefValue> configRefs)
        {
            // only execute if not happened last 1 seconds
            if (DateTime.Now.Ticks  - lastExecution.Ticks < 50000000) return;
            // Log.Instance.log("RetriggerInputAction.execute: Seconds since lastExecution " + (DateTime.Now.Ticks - lastExecution.Ticks), LogSeverity.Debug);

            foreach (MobiFlightModule module in moduleCache.GetModules()) {
                module.Retrigger();
            }

            lastExecution = DateTime.Now;
        }
    }
}
