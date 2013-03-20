using System;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using ESRI.ArcLogistics.Utility.Reflection;

namespace ESRI.ArcLogistics.App
{
    /// <summary>
    /// Implements <see cref="IGenericStorage&lt;T&gt;"/> using application settings storage.
    /// </summary>
    internal class ApplicationSettingsGenericStorage<T> : IGenericStorage<T>
        where T : new()
    {
        /// <summary>
        /// Initializes a new instance of the ApplicationSettingsGenericStorage&lt;T&gt; class.
        /// </summary>
        /// <param name="settingsProperty">The member expression specifying application
        /// settings property to be used for storing settings object.</param>
        public ApplicationSettingsGenericStorage(
            Expression<Func<Properties.Settings, string>> settingsProperty)
        {
            Debug.Assert(settingsProperty != null);

            _storageProperty = TypeInfoProvider<Properties.Settings>.GetPropertyInfo(
                settingsProperty);
        }

        #region IGenericStorage<T> Members
        /// <summary>
        /// Loads object of type <typeparamref name="T"/> from the application settings storage.
        /// </summary>
        /// <returns>Reference to the loaded object.</returns>
        public T Load()
        {
            var settings = Properties.Settings.Default;
            var serialized = (string)_storageProperty.GetValue(settings, null);

            if (string.IsNullOrEmpty(serialized))
            {
                return new T();
            }

            var obj = default(T);

            try
            {
                using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(serialized)))
                {
                    var serializer = new DataContractSerializer(typeof(T));
                    obj = (T)serializer.ReadObject(memoryStream);
                }
            }
            catch (SerializationException e)
            {
                Logger.Warning(e);
            }
            catch (IOException e)
            {
                Logger.Warning(e);
            }

            if (obj == null)
            {
                obj = new T();
            }

            return obj;
        }

        /// <summary>
        /// Saves object of type <typeparamref name="T"/> to the application settings storage.
        /// </summary>
        /// <param name="obj">The reference to the object to be saved.</param>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> is a null
        /// reference.</exception>
        public void Save(T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            var serializer = new DataContractSerializer(typeof(T));

            string serialized;
            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, obj);
                serialized = Encoding.UTF8.GetString(memoryStream.ToArray());
            }

            var settings = Properties.Settings.Default;
            _storageProperty.SetValue(settings, serialized, null);
            settings.Save();
        }
        #endregion

        #region private fields
        /// <summary>
        /// The reference to the descriptor of property to be used for storing settings object.
        /// </summary>
        private PropertyInfo _storageProperty;
        #endregion
    }
}
