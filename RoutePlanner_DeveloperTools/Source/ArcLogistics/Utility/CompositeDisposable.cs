using System;
using System.Collections.Generic;
using System.Linq;

namespace ESRI.ArcLogistics.Utility
{
    /// <summary>
    /// Represents a collection of disposable objects to be disposed together.
    /// </summary>
    internal sealed class CompositeDisposable : IDisposable
    {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the CompositeDisposable class.
        /// </summary>
        /// <param name="disposables">The collection of disposable objects to
        /// be disposed together.</param>
        public CompositeDisposable(params IDisposable[] disposables)
        {
            if (disposables == null)
            {
                throw new ArgumentNullException("disposables");
            }

            if (disposables.Any(disposable => disposable == null))
            {
                throw new ArgumentOutOfRangeException("disposables");
            }

            _disposables = disposables.ToList();
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Disposes all contained objects.
        /// </summary>
        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }

            _disposables.Clear();
        }
        #endregion

        #region private fields
        /// <summary>
        /// The collection of all disposable objects.
        /// </summary>
        private List<IDisposable> _disposables = new List<IDisposable>();
        #endregion
    }
}
