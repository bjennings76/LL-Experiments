using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

public class ErrorReportSilent : IErrorReport {
    private int m_TotalWarningCount;
    private int m_TotalErrorCount;
    private Dictionary<int, int> m_WarningCounts = new Dictionary<int, int>();
    private Dictionary<int, int> m_ErrorCounts = new Dictionary<int, int>();

    public int GetErrorCount() {
        return m_TotalErrorCount;
    }

    public int GetWarningCount() {
        return m_TotalWarningCount;
    }

    public int GetErrorCount(int errorId) {
        int count = 0;
        m_ErrorCounts.TryGetValue(errorId, out count);
        return count;
    }

    public int GetWarningCount(int warningId) {
        int count = 0;
        m_WarningCounts.TryGetValue(warningId, out count);
        return count;
    }



	[StringFormatMethod("format")]
	public void Log(string format, params object[] parameters) {}

	[StringFormatMethod("format")]
	public void Log(UnityEngine.Object context, string format, params object[] parameters) {}

	[StringFormatMethod("format")]
	public void Warning(string format, params object[] parameters) {
        AddWarningCount();
    }

	[StringFormatMethod("format")]
	public void Warning(UnityEngine.Object context, string format, params object[] parameters) {
        AddWarningCount();
    }

	[StringFormatMethod("format")]
	public void Warning(int warningId, string format, params object[] parameters) {
        AddWarningCount(warningId);
    }

	[StringFormatMethod("format")]
	public void Warning(int warningId, UnityEngine.Object context, string format, params object[] parameters) {
        AddWarningCount(warningId);
    }


	[StringFormatMethod("format")]
	public void Error(string format, params object[] parameters) {
        AddErrorCount();
    }

	[StringFormatMethod("format")]
	public void Error(UnityEngine.Object context, string format, params object[] parameters) {
        AddErrorCount();
    }

	[StringFormatMethod("format")]
	public void Error(int errorId, string format, params object[] parameters) {
        AddErrorCount(errorId);
    }

	[StringFormatMethod("format")]
	public void Error(int errorId, UnityEngine.Object context, string format, params object[] parameters) {
        AddErrorCount(errorId);
    }

    public void HandleException(Exception e) {
        AddErrorCount();
    }

    public void HandleException(UnityEngine.Object context, Exception e) {
        AddErrorCount();
    }

    private void AddErrorCount() {
        m_TotalErrorCount++;
    }

    private void AddErrorCount(int id) {
        int count = 0;
        m_ErrorCounts.TryGetValue(id, out count);
        count++;
        m_ErrorCounts[id] = count;

        m_TotalErrorCount++;
    }

    private void AddWarningCount() {
        m_TotalWarningCount++;
    }

    private void AddWarningCount(int id) {
        int count = 0;
        m_WarningCounts.TryGetValue(id, out count);
        count++;
        m_WarningCounts[id] = count;

        m_TotalWarningCount++;
    }



    public bool HasErrors {
        get { return m_TotalErrorCount > 0; }
    }

    public bool HasWarnings {
        get { return m_TotalWarningCount > 0; }
    }

    public bool HasErrorsOrWarnings {
        get { return m_TotalErrorCount > 0 || m_TotalWarningCount > 0; }
    }
}

