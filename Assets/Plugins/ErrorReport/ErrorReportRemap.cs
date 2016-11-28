using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Object = UnityEngine.Object;

public class ErrorReportRemap : ErrorReportFilter {

	public enum RemapTo {
		Null,
		Log,
		Warning,
		Error
	}

	private RemapTo[] m_Remap;

	public RemapTo RemapLog {
		get { return m_Remap[(int)Level.Log]; }
		set { m_Remap[(int)Level.Log] = value; }
	}

	public RemapTo RemapWarning {
		get { return m_Remap[(int)Level.Warning]; }
		set { m_Remap[(int)Level.Warning] = value; }
	}

	public RemapTo RemapError {
		get { return m_Remap[(int)Level.Error]; }
		set { m_Remap[(int)Level.Error] = value; }
	}

	public ErrorReportRemap(IErrorReport parent)
		: base(parent) {
		m_Remap = new RemapTo[] {
			RemapTo.Log,
			RemapTo.Warning,
			RemapTo.Error,
		};
	}

	[StringFormatMethod("format")]
	protected override void FilterLog(Level level, int id, Object context, string format, params object[] parameters) {
		switch (m_Remap[(int)level]) {
			case RemapTo.Null:
				break;
			case RemapTo.Log:
				ParentLog(Level.Log,id,context,format,parameters);
				break;
			case RemapTo.Warning:
				ParentLog(Level.Warning, id, context, format, parameters);
				break;
			case RemapTo.Error:
				ParentLog(Level.Error, id, context, format, parameters);
				break;
		}
	}


}
