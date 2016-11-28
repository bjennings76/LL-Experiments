using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

public abstract class ErrorReportFilter : IErrorReport{
	private IErrorReport m_Parent;
	private bool m_HasWarnings;
	private bool m_HasErrors;

	protected enum Level {
		Log,
		Warning,
		Error,
	}


	public ErrorReportFilter(IErrorReport parent) {
		m_Parent = parent;
	}

	[StringFormatMethod("format")]
	public void Log(string format, params object[] parameters) {
		FilterLog(Level.Log, 0, null, format, parameters);
	}

	[StringFormatMethod("format")]
	public void Log(Object context, string format, params object[] parameters) {
		FilterLog(Level.Log, 0, context, format, parameters);
	}

	[StringFormatMethod("format")]
	public void Warning(string format, params object[] parameters) {
		FilterLog(Level.Warning, 0, null, format, parameters);
	}

	[StringFormatMethod("format")]
	public void Warning(Object context, string format, params object[] parameters) {
		FilterLog(Level.Warning, 0, context, format, parameters);
	}

	[StringFormatMethod("format")]
	public void Warning(int warningId, string format, params object[] parameters) {
		FilterLog(Level.Warning, warningId, null, format, parameters);
	}

	[StringFormatMethod("format")]
	public void Warning(int warningId, Object context, string format, params object[] parameters) {
		FilterLog(Level.Warning, warningId, context, format, parameters);
	}

	[StringFormatMethod("format")]
	public void Error(string format, params object[] parameters) {
		FilterLog(Level.Error, 0, null, format, parameters);
	}

	[StringFormatMethod("format")]
	public void Error(Object context, string format, params object[] parameters) {
		FilterLog(Level.Error, 0, context, format, parameters);
	}

	[StringFormatMethod("format")]
	public void Error(int errorId, string format, params object[] parameters) {
		FilterLog(Level.Error, errorId, null, format, parameters);
	}

	[StringFormatMethod("format")]
	public void Error(int errorId, Object context, string format, params object[] parameters) {
		FilterLog(Level.Error, errorId, context, format, parameters);
	}

	public void HandleException(Exception e) {
		FilterLog(Level.Error, 0, null, "Exception {0}\n{1}\n{2}", e.GetType().FullName, e.Message, e.StackTrace);
	}

	public void HandleException(Object context, Exception e) {
		FilterLog(Level.Error, 0, context, "Exception {0}\n{1}\n{2}", e.GetType().FullName, e.Message, e.StackTrace);
	}

	protected abstract void FilterLog(Level level, int id, Object context, string format, params object[] parameters);
	protected void ParentLog(Level level, int id, Object context, string format, params object[] parameters) {


		switch (level) {
			case Level.Log:
				m_Parent.Log(context, format, parameters);
				break;
			case Level.Warning:
				m_Parent.Warning(id, context, format, parameters);
				m_HasWarnings = true;
				break;
			case Level.Error:
				m_Parent.Error(id, context, format, parameters);
				m_HasErrors = true;
				break;
		}
	}

	public bool HasErrors {
		get { return m_HasErrors; }
	}

	public bool HasWarnings {
		get { return m_HasWarnings; }
	}
}
