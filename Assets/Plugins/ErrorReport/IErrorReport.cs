using System;
using JetBrains.Annotations;

public interface IErrorReport {
	[StringFormatMethod("format")]
	void Log(string format, params object[] parameters);

	[StringFormatMethod("format")]
	void Log(UnityEngine.Object context, string format, params object[] parameters);

	[StringFormatMethod("format")]
	void Warning(string format, params object[] parameters);

	[StringFormatMethod("format")]
	void Warning(UnityEngine.Object context, string format, params object[] parameters);

	[StringFormatMethod("format")]
	void Warning(int warningId, string format, params object[] parameters);

	[StringFormatMethod("format")]
	void Warning(int warningId, UnityEngine.Object context, string format, params object[] parameters);


	[StringFormatMethod("format")]
	void Error(string format, params object[] parameters);

	[StringFormatMethod("format")]
	void Error(UnityEngine.Object context, string format, params object[] parameters);

	[StringFormatMethod("format")]
	void Error(int errorId, string format, params object[] parameters);

	[StringFormatMethod("format")]
	void Error(int errorId, UnityEngine.Object context, string format, params object[] parameters);

    void HandleException(Exception e);
    void HandleException(UnityEngine.Object context, Exception e);

    bool HasErrors { get; }
    bool HasWarnings { get; }
}
