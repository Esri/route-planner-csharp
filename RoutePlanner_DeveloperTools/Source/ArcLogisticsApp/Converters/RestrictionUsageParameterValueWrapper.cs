using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ESRI.ArcLogistics.App.GridHelpers;

namespace ESRI.ArcLogistics.App.Converters
{
    /// <summary>
    /// Wrapper for presenting restriction usage parameter value in combobox.
    /// </summary>
    internal class RestrictionUsageParameterValueWrapper
    {
        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="label">String representing parameter, which will be shown in UI.</param>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="parameterValue">Parameter value.</param>
        private RestrictionUsageParameterValueWrapper(string label, string parameterName,
            double parameterValue)
            : this(label, parameterName, parameterValue.ToString())
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="key">String, which will be shown in UI.</param>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="parameterValue">Parameter value.</param>
        private RestrictionUsageParameterValueWrapper(string key, string parameterName,
            string parameterValue)
        {
            Debug.Assert(parameterName != null);
            Debug.Assert(parameterValue != null);
            Debug.Assert(key != null);

            Parameter = new Parameter(parameterName, parameterValue);
            Label = key;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parameter">Paramete for which wrapper is created</param>
        private RestrictionUsageParameterValueWrapper(Parameter parameter)
            : this(parameter.Value, parameter.Name, parameter.Value)
        {
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Restriction usage parameter.
        /// </summary>
        public Parameter Parameter { get; private set; }

        /// <summary>
        /// String representing parameter, which will be shown in UI.
        /// </summary>
        public string Label { get; private set; }

        #endregion

        #region Public members

        /// <summary>
        /// Check that two parameters are equal.
        /// </summary>
        /// <param name="x">First parameter to compare.</param>
        /// <param name="y">Second parameter to compare.</param>
        /// <returns>'True' if both x and y are null or if their names and values are equal, 
        /// 'false' otherwise.</returns>
        public static bool operator ==(RestrictionUsageParameterValueWrapper x, 
            RestrictionUsageParameterValueWrapper y)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(x, y))
                return true;

            // If one is null, but not both, return false.
            if ((object)x == null || (object)y == null)
                return false;

            // Check that label and parameters of objects are equal.
            return x.Label == y.Label && x.Parameter == y.Parameter;
        }

        /// <summary>
        /// Check that two parameters are different.
        /// </summary>
        /// <param name="x">First parameter to compare.</param>
        /// <param name="y">Second parameter to compare.</param>
        /// <returns>'False' if both x and y are null or if their names and values are equal, 
        /// 'true' otherwise.</returns>
        public static bool operator !=(RestrictionUsageParameterValueWrapper x, 
            RestrictionUsageParameterValueWrapper y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">Object to compare with current.</param>
        /// <returns>'True' if object is RestrictionUsageParameterWrapper and its Label 
        /// and Parameter properties is the same as current object's.</returns>
        public override bool Equals(object obj)
        {
            var wrapper = obj as RestrictionUsageParameterValueWrapper;

            if (wrapper == null)
                return false;
            else
                return wrapper == this;
        }

        /// <summary>
        /// Get hash code for parameter.
        /// </summary>
        /// <returns>Hash code of current parameter.</returns>
        public override int GetHashCode()
        {
            return (Label != null ? Label.GetHashCode() : 0) ^ 
                (Parameter != null ? Parameter.GetHashCode() : 0);
        }


        /// <summary>
        /// String representation.
        /// </summary>
        /// <returns>Object's string representation.</returns>
        public override string ToString()
        {
            return Label;
        }

        #endregion

        #region Public static members

        /// <summary>
        /// Wrapper for specified parameter value.
        /// </summary>
        /// <param name="parameter">Restriction usage parameter.</param>
        /// <returns>Wrapper for specified parameter value.</returns>
        public static RestrictionUsageParameterValueWrapper GetValueWrapper(Parameter parameter)
        {
            // Try to get wrapper which value will be the same as parameter value.
            var defaultWrapper = GetSelectedItemWrapper(parameter);

            // If there is no default wrapper for current 
            // parameter value - create new wrapper for it.
            defaultWrapper = defaultWrapper ?? new RestrictionUsageParameterValueWrapper(parameter);

            return defaultWrapper;
        }

        /// <summary>
        /// Wrapper for specified parameter value, which is selected in editor.
        /// </summary>
        /// <param name="parameter">Restriction usage parameter.</param>
        /// <returns>Wrapper for specified parameter value.</returns>
        internal static RestrictionUsageParameterValueWrapper GetSelectedItemWrapper(
            Parameter parameter)
        {
            // If there is no restriction usage parameter - 
            // return wrapper for prohibited parameter value.
            if (parameter == null)
                return _GetProhibitedParameterWrapper(null);

            // Get wrapper which value will be the same as parameter value.
            var defaultWrappers = GetDefaultWrappers(parameter);
            return defaultWrappers.FirstOrDefault(x => x.Parameter.Value == parameter.Value);
        }

        /// <summary>
        /// Get default collection of RestrictionUsageParameterWrapper.
        /// </summary>
        /// <param name="parameter">Restriction usage parameter.</param>
        /// <returns>Collection with default RestrictionUsageParameterWrapper.</returns>
        public static IEnumerable<RestrictionUsageParameterValueWrapper> GetDefaultWrappers(
            Parameter parameter)
        {
            // If there is no parameter - return collection with only prohibited parameter wrapper.
            if (parameter == null)
                return new RestrictionUsageParameterValueWrapper[] { _GetProhibitedParameterWrapper(null) };

            var list = new List<RestrictionUsageParameterValueWrapper>();

            list.Add(_GetProhibitedParameterWrapper(parameter.Name));
            list.Add(new RestrictionUsageParameterValueWrapper(
                Properties.Resources.AvoidHighUsageParameterLabel, parameter.Name, 5.0));
            list.Add(new RestrictionUsageParameterValueWrapper(
                Properties.Resources.AvoidMediumUsageParameterLabel, parameter.Name, 2.0));
            list.Add(new RestrictionUsageParameterValueWrapper(
                Properties.Resources.AvoidLowUsageParameterLabel, parameter.Name, 1.3));
            list.Add(new RestrictionUsageParameterValueWrapper(
                Properties.Resources.PrefererLowUsageParameterLabel, parameter.Name, 0.8));
            list.Add(new RestrictionUsageParameterValueWrapper(
                Properties.Resources.PrefererMediumUsageParameterLabel, parameter.Name, 0.5));
            list.Add(new RestrictionUsageParameterValueWrapper(
                Properties.Resources.PrefererHighUsageParameterLabel, parameter.Name, 0.2));

            return list;
        }

        #endregion

        #region Private members

        /// <summary>
        /// Get wrapper for "prohibited" parameter value.
        /// </summary>
        /// <param name="parameterName">Name of the parameter.</param>
        /// <returns>"Prohibited" wrapper.</returns>
        private static RestrictionUsageParameterValueWrapper _GetProhibitedParameterWrapper(
            string parameterName)
        {
            return new RestrictionUsageParameterValueWrapper(
                Properties.Resources.ProhibitedUsageParameterLabel, parameterName ?? string.Empty, -1.0);
        }

        #endregion
    }
}
