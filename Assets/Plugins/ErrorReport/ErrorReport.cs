using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Object = UnityEngine.Object;


public class ErrorReport : IErrorReport {
	private int m_TotalWarningCount;
	private int m_TotalErrorCount;
	private HashSet<Object> m_Contexts = new HashSet<Object>();
	private Dictionary<int, int> m_WarningCounts = new Dictionary<int, int>();
	private Dictionary<int, int> m_ErrorCounts = new Dictionary<int, int>();

	public IEnumerable<Object> Contexts {
		get { return m_Contexts; }
	}

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

	public bool HasErrors {
		get { return m_TotalErrorCount > 0; }
	}

	public bool HasWarnings {
		get { return m_TotalWarningCount > 0; }
	}

	public bool HasErrorsOrWarnings {
		get { return m_TotalErrorCount > 0 || m_TotalWarningCount > 0; }
	}

#line hidden
	[StringFormatMethod("format")]
	public void Log(string format, params object[] parameters) {
		Debug.Log(String.Format(format, parameters));
	}

	[StringFormatMethod("format")]
	public void Log(Object context, string format, params object[] parameters) {
		Debug.Log(String.Format(format, parameters), NormaliseContext(context));
		m_Contexts.Add(context);
	}

	[StringFormatMethod("format")]
	public void Warning(string format, params object[] parameters) {
		Debug.LogWarning(String.Format(format, parameters));
		AddWarningCount();
	}

	[StringFormatMethod("format")]
	public void Warning(Object context, string format, params object[] parameters) {
		Debug.LogWarning(String.Format(format, parameters), NormaliseContext(context));
		AddWarningCount();
		m_Contexts.Add(context);
	}

	[StringFormatMethod("format")]
	public void Warning(int warningId, string format, params object[] parameters) {
		Debug.LogWarning(String.Format(format, parameters));
		AddWarningCount(warningId);
	}

	[StringFormatMethod("format")]
	public void Warning(int warningId, Object context, string format, params object[] parameters) {
		Debug.LogWarning(String.Format(format, parameters), NormaliseContext(context));
		AddWarningCount(warningId);
		m_Contexts.Add(context);
	}


	[StringFormatMethod("format")]
	public void Error(string format, params object[] parameters) {
		Debug.LogError(String.Format(format, parameters));
		AddErrorCount();
	}

	[StringFormatMethod("format")]
	public void Error(Object context, string format, params object[] parameters) {
		Debug.LogError(String.Format(format, parameters), NormaliseContext(context));
		AddErrorCount();
		m_Contexts.Add(context);
	}

	[StringFormatMethod("format")]
	public void Error(int errorId, string format, params object[] parameters) {
		Debug.LogError(String.Format(format, parameters));
		AddErrorCount(errorId);
	}

	[StringFormatMethod("format")]
	public void Error(int errorId, Object context, string format, params object[] parameters) {
		Debug.LogError(String.Format(format, parameters), NormaliseContext(context));
		AddErrorCount(errorId);
		m_Contexts.Add(context);
	}

	public void HandleException(Exception e) {
		Debug.LogException(e);
		AddErrorCount();
	}

	public void HandleException(Object context, Exception e) {
		Debug.LogException(e, NormaliseContext(context));
		AddErrorCount();
	}
#line default

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


	//public void Done(string message) {
	//    Done(message, message, message);
	//}

	//public void Done(string successMessage, string errorOrWarningMessage) {
	//    Done(successMessage, errorOrWarningMessage, errorOrWarningMessage);
	//}

	public void Done(string successMessage = "Everything is Awesome", string warningMessage = "Done", string errorMessage = "Done") {
		string warnings = "";
		string errors = "";
		string message = successMessage;
		bool bLogWarning = false;
		bool bLogError = false;

		string suffix = "";

		warnings = FormatPlural(m_TotalWarningCount, "{0} warnings", "{0} warning", "");
		bLogWarning = m_TotalWarningCount > 0;

		if (bLogWarning) {
			message = warningMessage;
			suffix = " ¯\\_(ツ)_/¯";
		}

		errors = FormatPlural(m_TotalErrorCount, "{0} errors", "{0} error", "");
		bLogError = m_TotalErrorCount > 0;

		if (bLogError) {
			message = errorMessage;
			suffix = " (╯°□°）╯︵ ┻━┻";
		}

		string fullMessage;

		// deal with a null message being passed in
		if (!string.IsNullOrEmpty(message)) {
			string warningSeperator = bLogWarning ? ", " : "";
			string errorSeperator = bLogError ? ", " : "";

			fullMessage = string.Format("{0}{1}{2}{3}{4}{5}", message, warningSeperator, warnings, errorSeperator, errors, suffix);
		} else {
			string errorSeperator = (bLogWarning && bLogError) ? ", " : "";
			fullMessage = string.Format("{0}{1}{2}{3}", warnings, errorSeperator, errors, suffix);
		}

#line hidden
		if (!string.IsNullOrEmpty(fullMessage)) {
			if (bLogError) {
				Debug.LogError(fullMessage);
			} else if (bLogWarning) { 
				Debug.LogWarning(fullMessage);
			} else {
				Debug.Log(fullMessage);
			}
		}
#line default
	}

	private Object NormaliseContext(Object o) {
#if UNITY_EDITOR	
		//Deal with the fact that Unity won't display Prefabs more than two deep
		// select the deepest object closest to the actual context object.
		string path = AssetDatabase.GetAssetPath(o);
		if (!string.IsNullOrEmpty(path)) {
			//we're an asset

			Transform t = null;
			GameObject go = o as GameObject;
			if (go != null) {
				t = go.transform;
			} else {
				Component component = o as Component;
				if (component != null) {
					t = component.transform;
					o = component.gameObject;
				}
			}
			if (t != null) {
				Object context = o;
				Object parentContext = o;		
				while (t != null) {
					//shuffle everything up so we keep a constant 2 deep buffer
					context = parentContext;
					parentContext = t.gameObject;
					t = t.parent;
				}
				return context;
			}
		}
#endif
		return o;
	}

	public static string FormatPlural(int count, string pluralString, string singularString, string noneString) {
		string format = null;
		if (count == 0) {
			format = noneString;
		} else if (Mathf.Abs(count) == 1) {
			format = singularString;
		} else {
			format = pluralString;
		}
		if (format != null) {
			return string.Format(format, count);
		} else {
			return null;
		}
	}
}