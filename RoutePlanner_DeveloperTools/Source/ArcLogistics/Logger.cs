/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Logging.Filters;
using Microsoft.Practices.EnterpriseLibrary.Logging.Formatters;
using Microsoft.Practices.EnterpriseLibrary.Logging.TraceListeners;
using ESRI.ArcLogistics.Properties;
using ESRI.ArcLogistics;
using System.Reflection;

namespace ESRI.ArcLogistics
{
    // Summary:
    // Identifies the type of event that has caused the trace.
    public enum ALTraceEventType
    {
        Critical = 1,
        Error = 2,
        Warning = 4,
        Information = 8,
     }

    /// <summary>
    /// Class-logger. For saving log messages.
    /// </summary>
    public sealed class Logger
    {
        #region constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private const string MESSAGE_FORMAT = "Timestamp: {timestamp(local:yyyy-MM-dd HH:mm:ss.fff)}{newline} Category: {category}{newline} Message: {message}{newline}";

        private const bool DEFAULT_IS_LOG_ENABLED = true;
        private const string DEFAULT_LOG_FILENAME = "Log.txt";
        private const int DEFAULT_LOG_FILESIZE_IN_KB = 5000;
        private const TraceEventType DEFAULT_LOG_MINIMAL_SEVERITY = TraceEventType.Information;

        #endregion

        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private Logger()
        {
        }

        #endregion

        #region public members

        public static TraceEventType MinimalSeverity
        {
            get { return _minimalSeverity; }
        }

        #endregion

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        
        // APIREV: make own type like TraceEventType so user won't have a need to add a reference to Logger dll.
        // Create log instance
        static public void Initialize(string logFilePath, int logFileSize,
            TraceEventType minimalSeverity, bool isLogEnabled)
        {
            if (logFileSize < 1)
                throw new SettingsException(Properties.Resources.LogFileCantBeSmall);
            _minimalSeverity = minimalSeverity;
            _isLogEnabled = isLogEnabled;

            // Create message formatter
            TextFormatter formatter = new TextFormatter(MESSAGE_FORMAT);

            // Create Log sources
            LogSource emptyTraceSource = new LogSource("EmptySource");
            LogSource errorsTraceSource = new LogSource("Logger", SourceLevels.All);

            // Create listener for rolling log file
            RollingFlatFileTraceListener rollingTrace = new RollingFlatFileTraceListener(logFilePath, "", "", formatter, logFileSize, "yyyy - MM - dd", RollFileExistsBehavior.Overwrite, RollInterval.Year);
            errorsTraceSource.Listeners.Add(rollingTrace);

            // Create and fill sources array
            IDictionary<string, LogSource> traceSources = new Dictionary<string, LogSource>();
            traceSources.Add(TraceEventType.Critical.ToString(), errorsTraceSource);
            traceSources.Add(TraceEventType.Error.ToString(), errorsTraceSource);
            traceSources.Add(TraceEventType.Warning.ToString(), errorsTraceSource);
            traceSources.Add(TraceEventType.Information.ToString(), errorsTraceSource);

            // create default category string
            string defaultCategory = _minimalSeverity.ToString();

            ICollection<ILogFilter> filters = new ILogFilter[0];
            _logWriter = new LogWriter(filters,   // filters collection
                                      traceSources,        // sources array
                                      emptyTraceSource,    // all events trace source
                                      emptyTraceSource,    // not processed trace source
                                      errorsTraceSource,   // errors trace source
                                      defaultCategory,     // string defaultCategory
                                      false,               // enable tracing
                                      true);               // save message as warning, when no categories match
        }

        /// <summary>
        /// Write an Info message to logfile
        /// </summary>
        /// <param name="message">Info message to log</param>
        public static void Info(string message)
        {
            _CreateLogRecord(message, TraceEventType.Information);
        }

        /// <summary>
        /// Write an Info message to logfile
        /// </summary>
        /// <param name="ex">Info message to log</param>
        public static void Info(Exception ex)
        {
            _CreateLogRecord(ex, TraceEventType.Information);
        }

        /// <summary>
        /// Write an Warning message to logfile
        /// </summary>
        /// <param name="message">Warning message to log</param>
        public static void Warning(string message)
        {
            _CreateLogRecord(message, TraceEventType.Warning);
        }

        /// <summary>
        /// Write an Warning message to logfile
        /// </summary>
        /// <param name="ex">Warning Exception to log</param>
        public static void Warning(Exception ex)
        {
            _CreateLogRecord(ex, TraceEventType.Warning);
        }

        /// <summary>
        /// Write an Error message to logfile
        /// </summary>
        /// <param name="message">Error message to log</param>
        public static void Error(string message)
        {
            _CreateLogRecord(message, TraceEventType.Error);
        }

        /// <summary>
        /// Write an Error message to logfile
        /// </summary>
        /// <param name="ex">Error Exception to log</param>
        public static void Error(Exception ex)
        {
            _CreateLogRecord(ex, TraceEventType.Error);
        }

        /// <summary>
        /// Write an CriticalError message to logfile
        /// </summary>
        /// <param name="message">CriticalError message to log</param>
        public static void Critical(string message)
        {
            _CreateLogRecord(message, TraceEventType.Critical);
        }

        /// <summary>
        /// Write an CriticalError message to logfile
        /// </summary>
        /// <param name="ex">CriticalError Exception to log</param>
        public static void Critical(Exception ex)
        {
            _CreateLogRecord(ex, TraceEventType.Critical);
        }

        public static bool IsSeverityEnabled(TraceEventType severity)
        {
            bool enabled = severity <= _minimalSeverity ? true : false;

            return enabled;
        }

        #endregion

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // Create log instance
        static private void _InitializeDefaults()
        {
            // calculate logs file path
            string assemblyFilePath = Assembly.GetExecutingAssembly().Location;
            string assemblyPath = System.IO.Path.GetDirectoryName(assemblyFilePath);
            string logFilePath = Path.Combine(assemblyPath, DEFAULT_LOG_FILENAME);

            Initialize(logFilePath, DEFAULT_LOG_FILESIZE_IN_KB, DEFAULT_LOG_MINIMAL_SEVERITY, DEFAULT_IS_LOG_ENABLED);
        }

        // Write record to file
        static private void _CreateLogRecord(string message, TraceEventType currentSeverity)
        {
            if (_logWriter == null)
            {
                _InitializeDefaults();
            }

            if (_isLogEnabled)
            {
                if (currentSeverity <= _minimalSeverity)
                {
                    LogEntry logEntry = new LogEntry();
                    logEntry.Categories.Add(currentSeverity.ToString());
                    logEntry.Message = message;
                    logEntry.Severity = currentSeverity;
                    _logWriter.Write(logEntry);
                }
            }
        }

        // Write record to file
        static private void _CreateLogRecord(Exception ex, TraceEventType currentSeverity)
        {
            string logMessage = _GetLogMessage(ex);

            _CreateLogRecord(logMessage, currentSeverity);
        }

        static private string _GetLogMessage(Exception ex)
        {
            string logMessage = ex.Message + "\r\n" + ex.StackTrace;

            if (ex.InnerException != null)
            {
                logMessage += "\r\n" + "Inner exception:\r\n";
                logMessage += _GetLogMessage(ex.InnerException);
            }

            return logMessage;
        }

        #endregion

        #region private members
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Minimal severity to logging
        /// </summary>
        private static TraceEventType _minimalSeverity;

        /// <summary>
        /// Indicates log on\off
        /// </summary>
        private static bool _isLogEnabled;

        private static LogWriter _logWriter;

        #endregion
    }
}
