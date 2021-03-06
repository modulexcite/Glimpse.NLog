﻿using System;
using System.Linq;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Extensions;
using Glimpse.Core.Tab.Assist;
using NLog;

namespace Glimpse.NLog
{
    public class NLogTab : ITab, ITabLayout, ITabSetup, IKey
    {
        private static readonly object Layout = TabLayout.Create()
                                                         .Row(r => {
                                                             r.Cell(0).WidthInPixels(60);
                                                             r.Cell(1).WidthInPixels(100);
                                                             r.Cell(2);
                                                             r.Cell(3).WidthInPixels(120).Suffix(" ms").AlignRight().Prefix("T+ ").Class("mono");
                                                             r.Cell(4).WidthInPixels(80).Suffix(" ms").AlignRight().Class("mono");
                                                         }).Build();

        public string Key {
            get { return "glimpse_nlog"; }
        }

        public string Name {
            get { return "NLog"; }
        }

        public RuntimeEvent ExecuteOn {
            get { return RuntimeEvent.EndRequest; }
        }

        public Type RequestContextType {
            get { return null; }
        }

        public object GetData(ITabContext context) {
            var section = Plugin.Create("Level", "Logger", "Message", "From Request Start", "From Last");
            foreach (var item in context.GetMessages<NLogEventInfoMessage>()) {
                section.AddRow()
                       .Column(string.Format("<span data-levelNum='{0}'>{1}</span>", item.LevelNumber, item.Level)).Raw()
                       .Column(item.Logger)
                       .Column(GetMessage(item.LogEvent))
                       .Column(item.FromFirst.TotalMilliseconds.ToString("0.00"))
                       .Column(item.FromLast.TotalMilliseconds.ToString("0.00"))
                       .Column(item)
                       .ApplyRowStyle(StyleFromLevel(item.Level));
            }

            return section.Build();
        }

        public object GetLayout() {
            return Layout;
        }

        private object GetMessage(LogEventInfo logEventInfo)
        {
            // Nicer exception view
            if (logEventInfo.Exception != null)
            {
                return new
                {
                    Message = logEventInfo.FormattedMessage,
                    Exception = logEventInfo.Exception
                };
            }

            // Only one object, present it
            if (logEventInfo.Message == "{0}")
                return logEventInfo.Parameters.FirstOrDefault();

            // Fall back on formatted message
            return logEventInfo.FormattedMessage;
        }

        public void Setup(ITabSetupContext context) {
            context.PersistMessages<NLogEventInfoMessage>();
        }

        private string StyleFromLevel(LogLevel level) {
            switch (level.Name) {
                case "Trace":
                    return "trace";
                case "Debug":
                    return "debug";
                case "Info":
                    return "info";
                case "Warn":
                    return "warn";
                case "Error":
                    return "error";
                case "Fatal":
                    return "fail";
                default:
                    return "";
            }
        }
    }
}