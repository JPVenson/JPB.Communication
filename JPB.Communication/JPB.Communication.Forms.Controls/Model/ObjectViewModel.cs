using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace JPB.Communication.Forms.Controls.Model
{
    public class ObjectViewModel : INotifyPropertyChanged
    {
        private readonly PropertyInfo _info;
        private readonly object _object;
        private readonly ObjectViewModel _parent;
        private readonly Type _type;
        private ReadOnlyCollection<ObjectViewModel> _children;

        private bool _isExpanded;
        private bool _isSelected;

        public ObjectViewModel(object obj)
            : this(obj, null, null)
        {
        }

        private ObjectViewModel(object obj, PropertyInfo info, ObjectViewModel parent)
        {
            _object = obj;
            _info = info;
            if (_object != null)
            {
                _type = obj.GetType();
                if (!IsPrintableType(_type))
                {
                    // load the _children object with an empty collection to allow the + expander to be shown
                    _children =
                        new ReadOnlyCollection<ObjectViewModel>(new[] {new ObjectViewModel(null)});
                }
            }
            _parent = parent;
        }

        public ObjectViewModel Parent
        {
            get { return _parent; }
        }

        public PropertyInfo Info
        {
            get { return _info; }
        }

        public ReadOnlyCollection<ObjectViewModel> Children
        {
            get { return _children; }
        }

        public string Type
        {
            get
            {
                string type = string.Empty;
                if (_object != null)
                    type = string.Format("({0})", _type.Name);
                else
                {
                    if (_info != null)
                        type = string.Format("({0})", _info.PropertyType.Name);
                }
                return type;
            }
        }

        public string Name
        {
            get
            {
                string name = string.Empty;
                if (_info != null)
                    name = _info.Name;
                return name;
            }
        }

        public string Value
        {
            get
            {
                string value = string.Empty;
                if (_object != null)
                {
                    if (IsPrintableType(_type))
                        value = _object.ToString();
                }
                else
                    value = "<null>";
                return value;
            }
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    if (_isExpanded)
                        LoadChildren();
                    OnPropertyChanged("IsExpanded");
                }

                // Expand all the way up to the root.
                if (_isExpanded && _parent != null)
                    _parent.IsExpanded = true;
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void LoadChildren()
        {
            if (_object != null)
            {
                // exclude value types and strings from listing child members
                if (!IsPrintableType(_type))
                {
                    // the public properties of this object are its children
                    List<ObjectViewModel> children = _type.GetProperties()
                        .Where(p => !p.GetIndexParameters().Any()) // exclude indexed parameters for now
                        .Select(p => new ObjectViewModel(p.GetValue(_object, null), p, this))
                        .ToList();

                    // if this is a collection type, add the contained items to the children
                    var collection = _object as IEnumerable;
                    if (collection != null)
                    {
                        foreach (object item in collection)
                        {
                            children.Add(new ObjectViewModel(item, null, this));
                            // todo: add something to view the index value
                        }
                    }

                    _children = new ReadOnlyCollection<ObjectViewModel>(children);
                    OnPropertyChanged("Children");
                }
            }
        }

        /// <summary>
        ///     Gets a value indicating if the object graph can display this type without enumerating its children
        /// </summary>
        private static bool IsPrintableType(Type type)
        {
            return type != null && (
                type.IsPrimitive ||
                type.IsAssignableFrom(typeof (string)) ||
                type.IsEnum);
        }

        public bool NameContains(string text)
        {
            if (String.IsNullOrEmpty(text) || String.IsNullOrEmpty(Name))
                return false;

            return Name.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        public bool ValueContains(string text)
        {
            if (String.IsNullOrEmpty(text) || String.IsNullOrEmpty(Value))
                return false;

            return Value.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}