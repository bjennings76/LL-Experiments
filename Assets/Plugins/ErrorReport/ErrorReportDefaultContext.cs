using System;
using JetBrains.Annotations;

public class ErrorReportDefaultContext : IErrorReport {
	private UnityEngine.Object m_DefaultContext;
	private IErrorReport m_Report;

	public UnityEngine.Object DefaultContext {
		get { return m_DefaultContext; }
		set { m_DefaultContext = value; }
	}

	public ErrorReportDefaultContext(UnityEngine.Object defaultContext, IErrorReport report) {
		m_DefaultContext = defaultContext;
		m_Report = report;
	}

#line hidden
	[StringFormatMethod("format")]
	public void Log(string format, params object[] parameters) {
		m_Report.Log(m_DefaultContext, format, parameters);
	}

	[StringFormatMethod("format")]
	public void Log(UnityEngine.Object context, string format, params object[] parameters) {
		m_Report.Log(context ?? m_DefaultContext, format, parameters);
	}

	[StringFormatMethod("format")]
	public void Warning(string format, params object[] parameters) {
		m_Report.Warning(m_DefaultContext, format, parameters);
	}

	[StringFormatMethod("format")]
	public void Warning(UnityEngine.Object context, string format, params object[] parameters) {
		m_Report.Warning(context ?? m_DefaultContext, format, parameters);
	}

	[StringFormatMethod("format")]
	public void Warning(int warningId, string format, params object[] parameters) {
		m_Report.Warning(warningId, m_DefaultContext, format, parameters);
	}

	[StringFormatMethod("format")]
	public void Warning(int warningId, UnityEngine.Object context, string format, params object[] parameters) {
		m_Report.Warning(warningId, context ?? m_DefaultContext, format, parameters);
	}

	[StringFormatMethod("format")]
	public void Error(string format, params object[] parameters) {
		m_Report.Error(m_DefaultContext, format, parameters);
	}

	[StringFormatMethod("format")]
	public void Error(UnityEngine.Object context, string format, params object[] parameters) {
		m_Report.Error(context ?? m_DefaultContext, format, parameters);
	}

	[StringFormatMethod("format")]
	public void Error(int errorId, string format, params object[] parameters) {
		m_Report.Error(errorId, m_DefaultContext, format, parameters);
	}

	[StringFormatMethod("format")]
	public void Error(int errorId, UnityEngine.Object context, string format, params object[] parameters) {
		m_Report.Error(errorId, context ?? m_DefaultContext, format, parameters);
	}

	public void HandleException(Exception e) {
		m_Report.HandleException(m_DefaultContext, e);
	}

	public void HandleException(UnityEngine.Object context, Exception e) {
		m_Report.HandleException(context ?? m_DefaultContext, e);
	}
#line default

	public bool HasErrors {
		get { return m_Report.HasErrors; }
	}

	public bool HasWarnings {
		get { return m_Report.HasWarnings; }
	}


}