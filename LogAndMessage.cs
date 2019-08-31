using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using AmiBroker.Data;

namespace AmiBroker.DataSources.FT
{
    internal enum MessageType : int { Error = 0, Warning = 1, Info = 2, Trace = 3 }

    /// <summary>
    /// Queued UI meassage to be displayed in the plugin notification area (button, right)
    /// </summary>
    internal class QueuedMessage
    {
        internal MessageType MessageType;
        internal string Message;
        internal DateTime EntryTime;
        internal DateTime DisplayTime;

        internal QueuedMessage(MessageType messageType, string message)
        {
            MessageType = messageType;
            Message = message;
            EntryTime = DateTime.Now;
            DisplayTime = DateTime.MinValue;
        }
    }

    /// <summary>
    /// This static class writes messages to the log file of .NET for AmiBroker
    /// and sends the UI messages to a queue of the data plugin to be shown in the notification area
    /// (Queuing messages is needed to keep them long enough for users to read.)
    /// </summary>
    [DebuggerStepThrough]
    internal static class LogAndMessage
    {
        private static bool verboseLog;                                 // if false, trace messages are NOT written to the log file
        private static string logSource;                                // text to identify the source of log entries written by this plugin
        private static List<QueuedMessage> queuedMessages;              // queue to keep messages for the plugin notification area

        static LogAndMessage()
        {
            verboseLog = false;
            logSource = "DataSource";
            queuedMessages = new List<QueuedMessage>();
        }

        /// <summary>
        /// Log source (column in log file to identify what component wrote the log entry)
        /// </summary>
        internal static string LogSource
        {
            get
            {
                return logSource;
            }
            set
            {
                logSource = value;
            }
        }

        /// <summary>
        /// Property to control how to handle Trace massages
        /// </summary>
        internal static bool VerboseLog
        {
            get
            {
                return verboseLog;
            }
            set
            {
                verboseLog = value;
            }
        }

        /// <summary>
        /// Writes a message to the log file
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        internal static void Log(MessageType type, string message)
        {
            if (IsLoggable(type))
            {
                LogMessage(type, message);
            }
        }

        /// <summary>
        /// Writes a message to the log file
        /// </summary>
        /// <param name="tickerData"></param>
        /// <param name="type"></param>
        /// <param name="message"></param>
        internal static void Log(TickerData tickerData, MessageType type, string message)
        {
            if (IsLoggable(type))
            {
                LogMessage(type, AddTickerToMessage(tickerData, message));
            }
        }

        /// <summary>
        /// Writes a message sent by TWS to the log file
        /// </summary>
        /// <param name="tickerData"></param>
        /// <param name="context"></param>
        /// <param name="e"></param>
        internal static void Log(TickerData tickerData, string context, int id, int errorCode, string errorMsg)
        {
            MessageType messageType = IBClientHelper.GetMessageType(errorCode);

            if (IsLoggable(messageType))
            {
                string message = GetTwsMessageForLog(tickerData, context, id, errorCode, errorMsg);

                LogMessage(messageType, message);
            }
        }

        /// <summary>
        /// Writes a message to the log file and queues it for the plugin notification area
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        internal static void LogAndQueue(MessageType type, string message)
        {
            if (IsLoggable(type))
            {
                LogMessage(type, message);
                QueueMessage(type, message);
            }
        }

        /// <summary>
        /// Writes a message to the log file and queues it for the plugin notification area
        /// </summary>
        /// <param name="tickerData"></param>
        /// <param name="type"></param>
        /// <param name="message"></param>
        internal static void LogAndQueue(TickerData tickerData, MessageType type, string message)
        {
            if (IsLoggable(type))
            {
                string msg = AddTickerToMessage(tickerData, message);

                LogMessage(type, msg);
                QueueMessage(type, msg);
            }
        }

        /// <summary>
        /// Writes an error message sent by TWS to the log file and queues it for the plugin notification area
        /// </summary>
        /// <param name="tickerData"></param>
        /// <param name="context"></param>
        /// <param name="e"></param>
        internal static void LogAndQueue(TickerData tickerData, string context, int id, int errorCode, string errorMsg)
        {
            MessageType messageType = IBClientHelper.GetMessageType(errorCode);

            if (IsLoggable(messageType))
            {
                LogMessage(messageType, GetTwsMessageForLog(tickerData, context, id, errorCode, errorMsg));
                QueueMessage(messageType, GetTwsMessageForUser(tickerData, context, id, errorCode, errorMsg));
            }
        }

        /// <summary>
        /// Gets the queued messages for the plugin notification area
        /// </summary>
        /// <returns></returns>
        internal static string GetMessages()
        {
            StringBuilder msg = new StringBuilder(500);

            // if no messages in the queue
            if (queuedMessages.Count == 0)
                return string.Empty;

            lock (queuedMessages)
            {
                int waitUnit = 5000;
                if (queuedMessages.Count > 1)
                    waitUnit = 3000;
                if (queuedMessages.Count > 10)
                    waitUnit = 1000;

                // remove all timed out messages
                while (queuedMessages.Count > 0
                    && queuedMessages[0].DisplayTime != DateTime.MinValue                           // already displayed &&
                    && (queuedMessages[0].DisplayTime.AddMilliseconds(waitUnit) < DateTime.Now ||   // was displayed long enough ||
                        queuedMessages[0].EntryTime.AddMilliseconds(10000) < DateTime.Now))         // already outdated
                    queuedMessages.RemoveAt(0);

                //#if DEBUG
                //                if (queuedMessages.Count > 0)
                //                {
                //                    msg.Append("#:");
                //                    msg.AppendLine(queuedMessages.Count.ToString());
                //                }
                //#endif

                // build text to be shown in the bouble of the data plugin (AB can show only 255 chars...)
                foreach (QueuedMessage m in queuedMessages)
                {
                    msg.AppendLine(m.Message);
                    if (m.DisplayTime == DateTime.MinValue)
                        m.DisplayTime = DateTime.Now;

                    if (msg.Length > 255)               // fits max buffer of AB
                    {
                        msg.Length = 255;
                        msg[252] = '.';
                        msg[253] = '.';
                        msg[254] = '.';
                        msg[254] = '\0';
                        break;
                    }
                }
            }

            return msg.ToString();
        }

        #region helpers

        /// <summary>
        /// Decides if a message needs to be written to the log
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsLoggable(MessageType type)
        {
            return type == MessageType.Error
                || type == MessageType.Warning
                || type == MessageType.Info
                || type == MessageType.Trace && verboseLog;
        }

        /// <summary>
        /// Decides if a message needs to be displayed in the plugin notification area
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsDisplayable(MessageType type)
        {
            return type == MessageType.Error
                || type == MessageType.Warning
                || type == MessageType.Info && verboseLog;
            //|| type == MessageType.Trace && verboseLog;
        }

        /// <summary>
        /// Converts the message type to its string representation and writes a complete message to the log file
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        internal static void LogMessage(MessageType type, string message)
        {
            switch (type)
            {
                case MessageType.Error:
                    DataSourceBase.DotNetLog(logSource, "Error", message);
                    break;

                case MessageType.Warning:
                    DataSourceBase.DotNetLog(logSource, "Warning", message);
                    break;

                case MessageType.Info:
                    DataSourceBase.DotNetLog(logSource, "Info", message);
                    break;

                case MessageType.Trace:

                    DataSourceBase.DotNetLog(logSource, "Trace", message);
                    break;
            }
        }

        /// <summary>
        /// Converts the message type to its string representation and queues the message for the plugin notification area
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        private static void QueueMessage(MessageType type, string message)
        {
            lock (queuedMessages)
            {
                switch (type)
                {
                    case MessageType.Error:
                        queuedMessages.Add(new QueuedMessage(type, "ERROR! " + message));
                        break;

                    case MessageType.Warning:
                        queuedMessages.Add(new QueuedMessage(type, "Warning! " + message));
                        break;

                    case MessageType.Info:
                        queuedMessages.Add(new QueuedMessage(type, message));
                        break;

                    case MessageType.Trace:
                        queuedMessages.Add(new QueuedMessage(type, message));
                        break;
                }
            }
        }

        /// <summary>
        /// Adds ticker info to the message
        /// </summary>
        /// <param name="tickerData"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private static string AddTickerToMessage(TickerData tickerData, string message)
        {
            if (tickerData == null)
                return message;
            else
                return tickerData.ToString() + ": " + message;
        }

        /// <summary>
        /// Converts ErrorEventArgs to a message to be logged
        /// </summary>
        /// <param name="tickerData"></param>
        /// <param name="context"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static string GetTwsMessageForLog(TickerData tickerData, string context, int id, int errorCode, string errorMsg)
        {
            StringBuilder stringBuilder = new StringBuilder(500);

            if (tickerData != null)
            {
                stringBuilder.Append(tickerData.Ticker);

                stringBuilder.Append(": ");
            }

            if (context != null)
            {
                stringBuilder.Append("Context: ");
                stringBuilder.Append(context);
                stringBuilder.Append(", ");
            }

            stringBuilder.Append("TWS Message: ");

            if (id >= 0)
            {
                stringBuilder.Append("ReqId: ");
                stringBuilder.Append(id);
                stringBuilder.Append(",");
            }

            stringBuilder.Append(" Code: ");
            stringBuilder.Append(errorCode);
            stringBuilder.Append(", Message: ");
            stringBuilder.Append(errorMsg);

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Converts/shortens ErrorEventArgs to a message to be presented to user in plugin notification area
        /// </summary>
        /// <param name="tickerData"></param>
        /// <param name="context"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private static string GetTwsMessageForUser(TickerData tickerData, string context, int id, int errorCode, string errorMsg)
        {
            StringBuilder stringBuilder = new StringBuilder(500);

            if (tickerData != null)
            {
                stringBuilder.Append(tickerData.Ticker);
                stringBuilder.Append(": ");
            }

            if (context != null)
            {
                stringBuilder.Append(context);
                stringBuilder.Append(": (");
            }

            stringBuilder.Append(errorCode);
            stringBuilder.Append(") ");
            if (errorCode == 162)
            {
                // remove the too long string part : Historical Market Data Service error message:
                string msg = "HMDS: " + errorMsg.Substring(errorMsg.IndexOf(':') + 1);
                stringBuilder.Append(msg);
            }
            else
                stringBuilder.Append(errorMsg);

            return stringBuilder.ToString();
        }

        #endregion
    }
}
