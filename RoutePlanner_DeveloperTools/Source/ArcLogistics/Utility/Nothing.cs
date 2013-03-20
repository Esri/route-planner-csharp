using System;

namespace ESRI.ArcLogistics.Utility
{
    /// <summary>
    /// Represents type of return value for methods that do not return values.
    /// </summary>
    /// <remarks>
    /// This type serves as a substitute for the <see cref="System.Void"/> type.
    /// In contrast to <see cref="System.Void"/> this class could have instances
    /// which makes it particularly useful for generic code. In type theory
    /// it is usually called Unit (see http://en.wikipedia.org/wiki/Unit_type),
    /// here we call it Nothing since Unit could be confused with a unit of measure.
    /// </remarks>
    /// <example>
    /// See how we need to duplicate code in order to provide similar processing
    /// for Action and Func instances.
    /// <![CDATA[
    /// static class VoidSample
    /// {
    ///     void LogAndCall(Action f)
    ///     {
    ///         try
    ///         {
    ///             f();
    ///         }
    ///         catch (Exception e)
    ///         {
    ///             Logger.Log(e);
    ///             throw;
    ///         }
    ///     }
    ///     T LogAndCall<T>(Func<T> f)
    ///     {
    ///         try
    ///         {
    ///             return f();
    ///         }
    ///         catch (Exception e)
    ///         {
    ///             Logger.Log(e);
    ///             throw;
    ///         }
    ///     }
    /// }
    /// ]]>
    /// Using Nothing type we can avoid this duplication by turning Action into
    /// Func&lt;Nothing&gt;
    /// <![CDATA[
    /// static class NothingSample
    /// {
    ///     void LogAndCall(Action f)
    ///     {
    ///         LogAndCall<Nothing>(() =>
    ///         {
    ///             f();
    ///             return new Nothing();
    ///         });
    ///     }
    ///     T LogAndCall<T>(Func<T> f)
    ///     {
    ///         try
    ///         {
    ///             return f();
    ///         }
    ///         catch (Exception e)
    ///         {
    ///             Logger.Log(e);
    ///             throw;
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </example>
    internal sealed class Nothing :
        IComparable,
        IComparable<Nothing>,
        IEquatable<Nothing>
    {
        #region public methods
        /// <summary>
        /// Checks whether the current instance is equal to other object.
        /// </summary>
        /// <param name="obj">An object to compare this instance with.</param>
        /// <returns>true if and only if the current instance is equal to
        /// the <paramref name="obj"/>.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as Nothing;
            if (object.ReferenceEquals(other, null))
            {
                return false;
            }

            return this.Equals(other);
        }

        /// <summary>
        /// Calculates hash code value for the current instance.
        /// </summary>
        /// <returns>A hash code for the current instance.</returns>
        public override int GetHashCode()
        {
            return 0;
        }
        #endregion

        #region IComparable Members
        /// <summary>
        /// Compares the current instance of <see cref="Nothing"/> type with another one.
        /// </summary>
        /// <param name="obj">An instance to compare this one with.</param>
        /// <returns>Zero always, since all instances of <see cref="Nothing"/>
        /// are considered equal.</returns>
        /// <exception cref="System.ArgumentException"><paramref name="obj"/>
        /// is not of type <see cref="Nothing"/>.</exception>
        public int CompareTo(object obj)
        {
            if (!(obj is Nothing))
            {
                var message = string.Format(
                    Properties.Messages.Error_ArgumentMustBeOfType,
                    typeof(Nothing).FullName);
                throw new ArgumentException(message, "obj");
            }

            return 0;
        }
        #endregion

        #region IComparable<Nothing> Members
        /// <summary>
        /// Compares the current instance of <see cref="Nothing"/> type with another one.
        /// </summary>
        /// <param name="other">An instance to compare this one with.</param>
        /// <returns>Zero always, since all instances of <see cref="Nothing"/>
        /// are considered equal.</returns>
        public int CompareTo(Nothing other)
        {
            return 0;
        }
        #endregion

        #region IEquatable<Nothing> Members
        /// <summary>
        /// Checks whether the current instance is equal to other one.
        /// </summary>
        /// <param name="other">An instance to compare this one with.</param>
        /// <returns>true always, since all instances of <see cref="Nothing"/>
        /// are considered equal.</returns>
        public bool Equals(Nothing other)
        {
            return true;
        }
        #endregion
    }
}
