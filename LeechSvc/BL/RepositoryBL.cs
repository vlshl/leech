using Common.Interfaces;
using Storage;
using System.Xml.Linq;

namespace BL
{
    /// <summary>
    /// Repository subsystem interface
    /// </summary>
    public interface IRepositoryBL
    {
        int PutXmlObject<T>(T obj, string key = "") where T : IXmlSerializable, new();
        int PutStrObject<T>(T obj, string key = "") where T : IStrSerializable, new();
        T GetXmlObject<T>(int id) where T : IXmlSerializable, new();
        T GetXmlObject<T>(string key) where T : IXmlSerializable, new();
        T GetStrObject<T>(int id) where T : IStrSerializable, new();
        T GetStrObject<T>(string key) where T : IStrSerializable, new();
        int? GetIntParam(string key);
        void SetIntParam(string key, int n);
        string GetStrParam(string key);
        void SetStrParam(string key, string data);
    }

    /// <summary>
    /// Используется для хранения в постоянной памяти различных объектов по ключу.
    /// </summary>
    public class RepositoryBL : IRepositoryBL
    {
        private readonly IRepositoryDA _reposDA;

        public RepositoryBL(IRepositoryDA da)
        {
            _reposDA = da;
        }

        /// <summary>
        /// Сохранить в репозитории xml-сериализуемый объект
        /// </summary>
        /// <typeparam name="T">Тип сериализуемого объекта</typeparam>
        /// <param name="obj">Сериализуемый объект</param>
        /// <param name="key">Ключ. Если с таким ключом объект уже существует, он будет перезаписан новым объектом. Идентификатор записи при этом не измеится.</param>
        /// <returns>Идентификатор записи (позволяет однозначно идентифицировать запись)</returns>
        public int PutXmlObject<T>(T obj, string key = "") where T : IXmlSerializable, new()
        {
            int id = 0;
            var ro = _reposDA.Select(0, key);
            string data = obj.Serialize().ToString();
            if (ro == null)
            {
                id = _reposDA.Insert(key, data);
            }
            else
            {
                ro.Data = data;
                _reposDA.Update(ro);
                id = ro.ReposID;
            }

            return id;
        }

        /// <summary>
        /// Сохранить в репозитории string-сериализуемый объект
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="obj">Сериализуемый объект</param>
        /// <param name="key">Ключ. Если с таким ключом объект уже существует, он будет перезаписан новым объектом. Идентификатор записи при этом не измеится.</param>
        /// <returns>Идентификатор записи (позволяет однозначно идентифицировать запись)</returns>
        public int PutStrObject<T>(T obj, string key = "") where T : IStrSerializable, new()
        {
            int id = 0;
            var ro = _reposDA.Select(0, key);
            string data = obj.Serialize();
            if (ro == null)
            {
                id = _reposDA.Insert(key, data);
            }
            else
            {
                ro.Data = data;
                _reposDA.Update(ro);
                id = ro.ReposID;
            }

            return id;
        }

        /// <summary>
        /// Извлечь xml-сериализуемый объект из хранилища и удалить его в хранилище
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="id">Идентификатор записи (однозначно идентифицирует запись)</param>
        /// <returns>Извлеченный объект или объект по умолчанию нужного типа</returns>
        public T GetXmlObject<T>(int id) where T : IXmlSerializable, new()
        {
            return GetXmlObject<T>(id, "");
        }

        /// <summary>
        /// Извлечь xml-сериализуемый объект из хранилища и удалить его в хранилище
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="key">Ключ. С одним ключом хранится не более одного объекта.</param>
        /// <returns>Извлеченный объект или объект по умолчанию нужного типа</returns>
        public T GetXmlObject<T>(string key) where T : IXmlSerializable, new()
        {
            return GetXmlObject<T>(0, key);
        }

        private T GetXmlObject<T>(int id, string key) where T : IXmlSerializable, new()
        {
            var ro = _reposDA.Select(id, key);
            if (ro == null) return default(T);

            _reposDA.Delete(ro.ReposID);

            var xDoc = XDocument.Parse(ro.Data);
            var t = new T();
            t.Initialize(xDoc);

            return t;
        }

        /// <summary>
        /// Извлечь string-сериализуемый объект из хранилища и удалить его в хранилище
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="id">Идентификатор записи (однозначно идертифицирует запись)</param>
        /// <returns>Извлеченный объект или объект по умолчанию нужного типа</returns>
        public T GetStrObject<T>(int id) where T : IStrSerializable, new()
        {
            return GetStrObject<T>(id, "");
        }

        /// <summary>
        /// Извлечь string-сериализуемый объект из хранилища и удалить его в хранилище
        /// </summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="key">Ключ. С одним ключом хранится не более одного объекта.</param>
        /// <returns>Извлеченный объект или объект по умолчанию нужного типа</returns>
        public T GetStrObject<T>(string key) where T : IStrSerializable, new()
        {
            return GetStrObject<T>(0, key);
        }

        private T GetStrObject<T>(int id, string key) where T : IStrSerializable, new()
        {
            var ro = _reposDA.Select(id, key);
            if (ro == null) return default(T);

            _reposDA.Delete(ro.ReposID);
            var t = new T();
            t.Initialize(ro.Data);

            return t;
        }

        /// <summary>
        /// Получить числовое значение по ключу
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns>Значение или null</returns>
        public int? GetIntParam(string key)
        {
            var ro = _reposDA.Select(0, key);
            if (ro == null) return null;

            int res = 0;
            if (!int.TryParse(ro.Data, out res)) return null;

            return res;
        }

        /// <summary>
        /// Сохранить числовое значение по ключу.
        /// При сохранении другого значения с тем же ключом, старое значение будет заменено новым.
        /// </summary>
        /// <param name="key">Ключ. Обеспечивает уникальность.</param>
        /// <param name="n">Значение</param>
        public void SetIntParam(string key, int n)
        {
            var ro = _reposDA.Select(0, key);
            if (ro == null)
            {
                _reposDA.Insert(key, n.ToString());
            }
            else
            {
                ro.Data = n.ToString();
                _reposDA.Update(ro);
            }
        }

        /// <summary>
        /// Получить строковое значение по ключу
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns>Значение или пустая строка</returns>
        public string GetStrParam(string key)
        {
            var ro = _reposDA.Select(0, key);
            if (ro == null) return "";

            return ro.Data;
        }

        /// <summary>
        /// Сохранить строковое значение по ключу.
        /// При сохранении другого значения с тем же ключом, старое значение будет заменено новым.
        /// </summary>
        /// <param name="key">Ключ (обеспечивает уникальность)</param>
        /// <param name="data">Значение</param>
        public void SetStrParam(string key, string data)
        {
            var ro = _reposDA.Select(0, key);
            if (ro == null)
            {
                _reposDA.Insert(key, data);
            }
            else
            {
                ro.Data = data;
                _reposDA.Update(ro);
            }
        }
    }
}
