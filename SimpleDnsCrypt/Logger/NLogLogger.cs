using Caliburn.Micro;
using System;

namespace SimpleDnsCrypt.Logger
{
	public class NLogLogger : ILog
	{
		private readonly NLog.Logger _nLogLogger;

		public NLogLogger(Type type)
		{
			_nLogLogger = NLog.LogManager.GetLogger(type.Name);
		}

		public void Error(Exception exception)
		{
			if (LogMode.Error)
			{
				_nLogLogger.Error(exception);
			}
		}
		public void Info(string format, params object[] args)
		{
			if (LogMode.Debug)
			{
				_nLogLogger.Debug(format, args);
			}
		}
		public void Warn(string format, params object[] args)
		{
			if (LogMode.Warn)
			{
				_nLogLogger.Warn(format, args);
			}
		}
	}
}
