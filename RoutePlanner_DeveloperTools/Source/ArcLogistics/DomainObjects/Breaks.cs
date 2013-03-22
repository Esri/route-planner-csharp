/*
 | Version 10.1.84
 | Copyright 2013 Esri
 |
 | Licensed under the Apache License, Version 2.0 (the "License");
 | you may not use this file except in compliance with the License.
 | You may obtain a copy of the License at
 |
 |    http://www.apache.org/licenses/LICENSE-2.0
 |
 | Unless required by applicable law or agreed to in writing, software
 | distributed under the License is distributed on an "AS IS" BASIS,
 | WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 | See the License for the specific language governing permissions and
 | limitations under the License.
 */

using System;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using ESRI.ArcLogistics.DomainObjects.Validation;
using ESRI.ArcLogistics.BreaksHelpers;

namespace ESRI.ArcLogistics.DomainObjects
{
    /// <summary>
    /// Class that represents a set of breaks.
    /// </summary>
    public class Breaks : ICollection<Break>, ICloneable, INotifyPropertyChanged, INotifyCollectionChanged
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <c>Breaks</c> class.
        /// </summary>
        public Breaks()
        {
            _breaks.CollectionChanged +=
                new NotifyCollectionChangedEventHandler(_BreaksCollectionChanged);
        }

        #endregion // Constructors

        #region Public static properties
   
        /// <summary>
        /// Gets name of the Breaks property.
        /// </summary>
        public static string PropertyNameBreaks
        {
            get { return PROP_NAME_BREAKS; }
        }

        /// <summary>
        /// Gets maximum count of <c>Break</c>.
        /// </summary>
        public static int MaximumBreakCount
        {
            get { return SOLVER_BREAK_COUNT; }
        }

        #endregion // Public static properties

        #region Static methods

        /// <summary>
        /// Puts all breaks to string.
        /// </summary>
        /// <param name="breaks">Breaks values.</param>
        internal static string AssemblyDBString(Breaks breaks)
        {
            var result = new StringBuilder();
            for (int index = 0; index < breaks.Count; ++index)
            {
                Break currBreak = breaks[index];
                result.AppendFormat("{0}{1}{2}",
                                    currBreak.GetType().ToString(),
                                    PART_SPLITTER,
                                    currBreak.ConvertToString());

                if (index < breaks.Count - 1)
                    result.Append(CommonHelpers.SEPARATOR_ALIAS); // NOTE: after last not needed
            }

            return result.ToString();
        }

        /// <summary>
        /// Parses string and splits it to properties values.
        /// </summary>
        /// <param name="value">DB order custom properties string.</param>
        /// <returns>Parsed capacities.</returns>
        internal static Breaks CreateFromDBString(string value)
        {
            var breaks = new Breaks();
            if (null != value)
            {
                var valuesSeparator = new string[1] { CommonHelpers.SEPARATOR_ALIAS };
                string[] values =
                    value.Split(valuesSeparator, StringSplitOptions.RemoveEmptyEntries);

                var separator = new string[1] { PART_SPLITTER };
                for (int index = 0; index < values.Length; ++index)
                {
                    string currValue = values[index];

                    Break instBreak = null;
                    if (-1 == currValue.IndexOf(PART_SPLITTER))
                    {   // support old version
                        instBreak = new TimeWindowBreak();
                        instBreak.InitFromString(currValue);
                    }
                    else
                    {   // current version
                        string[] breakValues =
                            currValue.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                        Debug.Assert(2 == breakValues.Length);

                        Type type = Type.GetType(breakValues[0]);
                        instBreak = (Break)Activator.CreateInstance(type);
                        instBreak.InitFromString(breakValues[1]); // init state
                    }

                    Debug.Assert(null != instBreak);
                    breaks.Add(instBreak);
                }
            }

            return breaks;
        }

        #endregion // Static methods

        #region Public members
 
        /// <summary>
        /// Returns a string representation of the break information.
        /// </summary>
        /// <returns>Break's string.</returns>
        public override string ToString()
        {
            if (Count == 0)
                return string.Format(Properties.Resources.BreaksIsEmpty);
            else if (Count == 1)
                return this[0].ToString();
            else
                return string.Format(Properties.Resources.BreaksFormat, Count);
        }

        /// <summary>
        /// Sorting Breaks.
        /// Bubble sort.
        /// Sort is ascending. TimeWindowBreaks sorted by From property, TimeInterval Breaks - by TimeInterval property.
        /// </summary>
        public void Sort()
        {
            if (_breaks != null)
            {
                for (int i = _breaks.Count - 1; i > 0; i--)
                    for (int j = 0; j < i; j++)
                        if (BreaksHelper.Compare(_breaks[j], _breaks[j + 1]) == 1)
                        {
                            Break tmp = _breaks[j];
                            _breaks[j] = _breaks[j + 1];
                            _breaks[j + 1] = tmp;
                        }
            }
        }

        #endregion 

        #region ICollection<Break> members
      
        /// <summary>
        /// Gets <c>Break</c> from <c>Breaks</c> by index.
        /// </summary>
        /// <param name="index">Collection index.</param>
        /// <returns>Founded object.</returns>
        public Break this[int index]
        {
            get { return _breaks[index]; }
        }

        /// <summary>
        /// Gets the number of elements contained in the <c>Breaks</c>.
        /// </summary>
        public int Count
        {
            get { return _breaks.Count; }
        }

        /// <summary>
        /// Adds an break to the <c>Breaks</c>.
        /// </summary>
        /// <param name="obj">Break to add.</param>
        /// <exception cref="NotSupportedException">Exception is thrown if you try to add a break
        /// when the collection already cotnains maxumim breaks count.
        /// This is the limitation of current version.</exception>
        public virtual void Add(Break obj)
        {
            if (null == obj)
                throw new ArgumentNullException(); // exception
            if (SOLVER_BREAK_COUNT == Count)
                throw new NotSupportedException(); // exception
            obj.Breaks = this;
            _breaks.Add(obj);
            obj.PropertyChanged += new PropertyChangedEventHandler(_BreakPropertyChanged);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <c>Breaks</c>.
        /// Set object's Breaks property to this.
        /// </summary>
        /// <param name="obj">Object to remove.</param>
        /// <returns>TRUE if item was successfully removed from Breaks; otherwise, false.
        /// This method also returns false if item is not found in the original <c>Breaks</c>.</returns>
        public bool Remove(Break obj)
        {
            obj.PropertyChanged -= _BreakPropertyChanged;
            obj.Breaks = null;
            return _breaks.Remove(obj);
        }

        /// <summary>
        /// Removes all items from the <c>Breaks</c>.
        /// </summary>
        public void Clear()
        {
            foreach (Break item in _breaks)
            {
                item.Breaks = null;
                item.PropertyChanged -= _BreakPropertyChanged;
            }
            _breaks.Clear();
        }

        /// <summary>
        /// Checks <c>Breaks</c> contains a specific value.
        /// </summary>
        /// <param name="obj">The object to locate in the <c>Breaks</c>.</param>
        /// <returns>TRUE if item is found in the <c>Breaks</c>; otherwise, false.</returns>
        public bool Contains(Break obj)
        {
            return _breaks.Contains(obj);
        }

        /// <summary>
        /// Returns index of Break in breaks collection.
        /// </summary>
        /// <param name="obj">The object to locate in the <c>Breaks</c>.</param>
        /// <returns>Index of break if break is in Breaks and -1 if not.</returns>
        public int IndexOf(Break obj)
        {
            return BreaksHelper.IndexOf(this,obj);
        }

        /// <summary>
        /// Copies the elements of the <c>Breaks</c> to an System.Array, starting at a particular
        /// System.Array index.
        /// </summary>
        /// <param name="objects">The one-dimensional System.Array that is the destination of the
        /// elements copied from <c>Breaks</c>. The System.Array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        public void CopyTo(Break[] objects, int index)
        {
            _breaks.CopyTo(objects, index);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>A System.Collections.Generic.IEnumerator that can be used to iterate through
        /// the collection.</returns>
        IEnumerator<Break> IEnumerable<Break>.GetEnumerator()
        {
            return _breaks.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An System.Collections.IEnumerator object that can be used to iterate through
        /// the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _breaks.GetEnumerator();
        }

        /// <summary>
        /// Gets a value indicating whether the <c>Breaks</c> is read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion // ICollection members

        #region ICloneable members

        /// <summary>
        /// Clones the breaks object.
        /// </summary>
        /// <returns>Cloned object.</returns>
        public object Clone()
        {
            var obj = new Breaks();
            foreach (Break item in _breaks)
                obj.Add(item.Clone() as Break);

            return obj;
        }

        #endregion // ICloneable members

        #region INotifyCollectionChanged members

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion INotifyCollectionChanged members

        #region INotifyPropertyChanged members

        /// <summary>
        /// Event which is invoked when any of the object's properties change.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion 

        #region Internal methods

        /// <summary>
        /// Compare two breaks collection that their breaks have equal values.
        /// </summary>
        /// <param name="breaks">Breaks collection to compare with this.</param>
        /// <returns>"True" if they contain breaks with same values and false otherwise.</returns>
        internal bool EqualsByValue(Breaks breaks)
        {
            Breaks breaksToCompare = breaks.Clone() as Breaks;

            // Collections must have same breaks count.
            if (this.Count != breaksToCompare.Count)
                return false;

            // If they both dont have breaks - return true.
            if (this.Count == 0)
                return true;

            // Sort breaks.
            Breaks sortedBreaks = this.Clone() as Breaks;
            sortedBreaks.Sort();
            breaksToCompare.Sort();

            // Compare each breaks in one collection with corresponding break in another collection.
            for (int i = 0; i < sortedBreaks.Count; i++)
                if (!sortedBreaks[i].EqualsByValue(breaksToCompare[i]))
                    return false;

            // Collection have breaks with same values.
            return true;
        }

        /// <summary>
        /// Returns a string representation of the break information which is used for exporting.
        /// </summary>
        /// <returns>Break's string.</returns>
        internal string AssemblyExportString()
        {
            var result = new StringBuilder();
            for (int index = 0; index < Count; index++)
            {
                if (0 < index)
                    result.Append(CommonHelpers.SEPARATOR_ALIAS);

                Break currBreak = this[index];
                string typeName = null;
                if (currBreak.GetType() == typeof(TimeWindowBreak))
                    typeName = TIME_WINDOW_SHORTENED_NAME;
                else if (currBreak.GetType() == typeof(DriveTimeBreak))
                    typeName = DRIVE_TIME_SHORTENED_NAME;
                else if (currBreak.GetType() == typeof(WorkTimeBreak))
                    typeName = WORK_TIME_SHORTENED_NAME;

                result.AppendFormat(EXPORT_STRING_FORMAT,
                    typeName, CommonHelpers.SEPARATOR, currBreak.ConvertToString());
            }
            return result.ToString();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Notifies about change of the break from breaks collection.
        /// </summary>
        /// <param name="sender">Ignored.</param>
        /// <param name="e">Property changed event arguments.</param>
        private void _BreakPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _NotifyPropertyChanged(e.PropertyName);
        }

        /// <summary>
        /// Notifies about change of the breaks collection.
        /// </summary>
        /// <param name="sender">This collection.</param>
        /// <param name="e">NotifyCollectionChangedEventArgs.</param>
        private void _BreaksCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _NotifyPropertyChanged(PROP_NAME_BREAKS);
            if (CollectionChanged != null)
                CollectionChanged(this,e);
        }

        /// <summary>
        /// Notifies about change of the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        private void _NotifyPropertyChanged(string propertyName)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // Private methods

        #region Private constants
    
        /// <summary>
        /// Name of the Breaks property.
        /// </summary>
        private const string PROP_NAME_BREAKS = "Breaks";

        /// <summary>
        /// Solver supported break count.
        /// </summary>
        private const int SOLVER_BREAK_COUNT = 5;

        /// <summary>
        /// Context part splitter.
        /// </summary>
        private const string PART_SPLITTER = "&semicolon";

        #endregion // Private constants

        #region Private members

        /// <summary>
        /// Shortened names of breaks
        /// </summary>
        private const string TIME_WINDOW_SHORTENED_NAME = "t";
        private const string DRIVE_TIME_SHORTENED_NAME = "d";
        private const string WORK_TIME_SHORTENED_NAME = "w";

        private const string EXPORT_STRING_FORMAT = "{0}{1}{2}";

        /// <summary>
        /// Breaks collection.
        /// </summary>
        private ObservableCollection<Break> _breaks = new ObservableCollection<Break>();

        #endregion // Private members
    }
}
