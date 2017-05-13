﻿using SS14.Client.Interfaces.Configuration;
using SS14.Client.Interfaces.MessageLogging;
using SS14.Shared;
using SS14.Shared.GO;
using SS14.Shared.IoC;
using System;
using System.ServiceModel;
using System.Timers;

namespace SS14.Client.Services.MessageLogging
{
    [IoCTarget]
    public class MessageLogger : IMessageLogger
    {
        private static Timer _pingTimer;
        private readonly MessageLoggerServiceClient _loggerServiceClient;
        private bool _logging;

        public MessageLogger(IPlayerConfigurationManager _configurationManager)
        {
            _logging = _configurationManager.GetMessageLogging();
            _loggerServiceClient = new MessageLoggerServiceClient("NetNamedPipeBinding_IMessageLoggerService");
            if (_logging)
            {
                Ping();
                _pingTimer = new Timer(5000);
                _pingTimer.Elapsed += CheckServer;
                _pingTimer.Enabled = true;
            }
        }

        #region IMessageLogger Members

        /// <summary>
        /// Check to see if the server is still running
        /// </summary>
        public void Ping()
        {
            bool failed = false;
            try
            {
                bool up = _loggerServiceClient.ServiceStatus();
            }
            catch (CommunicationException e)
            {
                failed = true;
            }
            finally
            {
                if (failed)
                    _logging = false;
            }
        }

        public void LogOutgoingComponentNetMessage(int uid, ComponentFamily family, object[] parameters)
        {
            if (!_logging)
                return;

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] is Enum)
                    parameters[i] = (int) parameters[i];
            }
            try
            {
                _loggerServiceClient.LogClientOutgoingNetMessage(uid, (int) family, parameters);
            }
            catch (CommunicationException e)
            {
            }
        }

        public void LogIncomingComponentNetMessage(int uid, EntityMessage entityMessage, ComponentFamily componentFamily,
                                                   object[] parameters)
        {
            if (!_logging)
                return;

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] is Enum)
                    parameters[i] = (int) parameters[i];
            }
            try
            {
                _loggerServiceClient.LogClientIncomingNetMessage(uid, (int) entityMessage, (int) componentFamily,
                                                                 parameters);
            }
            catch (CommunicationException e)
            {
            }
        }

        public void LogComponentMessage(int uid, ComponentFamily senderfamily, string sendertype,
                                        ComponentMessageType type)
        {
            if (!_logging)
                return;

            try
            {
                _loggerServiceClient.LogClientComponentMessage(uid, (int) senderfamily, sendertype, (int) type);
            }
            catch (CommunicationException e)
            {
            }
        }

        #endregion

        public static void CheckServer(object source, ElapsedEventArgs e)
        {
            IoCManager.Resolve<IMessageLogger>().Ping();
        }
    }
}
