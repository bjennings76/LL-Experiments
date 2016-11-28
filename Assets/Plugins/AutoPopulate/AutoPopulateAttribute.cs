using System;
using UnityEngine;

namespace Utils {
  public class AutoPopulateAttribute : PropertyAttribute {
    public enum FindBehaviour {
      Self,
      Children,
      Parent
    }

    public enum EditBehaviour {
      ReadOnlyUntilUserAction,
      Never,
      Always
    }

    private readonly FindBehaviour m_Find;
    private readonly Type m_FindRelativeTo;
    private readonly EditBehaviour m_Editable;
    private readonly Type m_TypeOverride;

    public FindBehaviour Find {
      get {
        return m_Find;
      }
    }

    public Type FindRelativeTo {
      get {
        return m_FindRelativeTo;
      }
    }

    public EditBehaviour Editable {
      get {
        return m_Editable;
      }
    }

    public Type TypeOverride {
      get {
        return m_TypeOverride;
      }
    }


    public AutoPopulateAttribute(FindBehaviour find = FindBehaviour.Self, Type findRelativeTo = null, EditBehaviour editable = EditBehaviour.ReadOnlyUntilUserAction, Type typeOverride = null) {
      m_Find = find;
      m_FindRelativeTo = findRelativeTo;
      m_Editable = editable;
      m_TypeOverride = typeOverride;
    }
  }

  public class AutoPopulateFromChildrenAttribute : AutoPopulateAttribute {
    public AutoPopulateFromChildrenAttribute(Type findRelativeTo = null, EditBehaviour editable = EditBehaviour.ReadOnlyUntilUserAction, Type typeOverride = null) : base(FindBehaviour.Children, findRelativeTo, editable, typeOverride) {}
  }

  public class AutoPopulateFromParentAttribute : AutoPopulateAttribute {
    public AutoPopulateFromParentAttribute(Type findRelativeTo = null, EditBehaviour editable = EditBehaviour.ReadOnlyUntilUserAction, Type typeOverride = null) : base(FindBehaviour.Parent, findRelativeTo, editable, typeOverride) {}
  }
}