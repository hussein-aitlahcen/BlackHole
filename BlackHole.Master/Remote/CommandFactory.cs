using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlackHole.Master.Remote
{
    public enum CommandType
    {
        DOWNLOAD,
        UPLOAD,
    }

    public static class CommandFactory
    {
        private static long m_nextCommandId;
        public static long NextCommandId => Interlocked.Increment(ref m_nextCommandId);

        public static IRemoteCommand CreateCommand<TIn>(CommandType type,
            Slave slave,
            string targetText,
            Action<RemoteCommand<TIn>> onExecute,
            Action<TIn> onContinue,
            Action<TIn> onCompleted,
            Action onFaulted)
        {
            switch (type)
            {
                case CommandType.DOWNLOAD:
                    return new RemoteCommand<TIn>(
                        NextCommandId,
                        "Download", 
                        slave, 
                        "Downloading...",
                        targetText,
                        onExecute, 
                        onContinue, 
                        onCompleted, 
                        onFaulted);

                default:
                    return null;
            }
        }
    }
}
