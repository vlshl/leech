using System;
using System.Linq;
using Storage.Data;
using CommonData = Common.Data;

namespace Storage
{
    /// <summary>
    /// Repository da-layer interface
    /// </summary>
    public interface IRepositoryDA
    {
        int Insert(string key, string data);
        CommonData.ReposObject Select(int reposID = 0, string key = null);
        void Update(CommonData.ReposObject ro);
        void Delete(int reposID);
    }

    /// <summary>
    /// DA-layer for Repository
    /// </summary>
    public class RepositoryDA : IRepositoryDA
    {
        private IStorage _da;

        public RepositoryDA(IStorage da)
        {
            _da = da;
        }

        /// <summary>
        /// Insert repository object into db
        /// </summary>
        /// <param name="key">Unique string key</param>
        /// <param name="data">Serialized data</param>
        /// <returns>Id</returns>
        public int Insert(string key, string data)
        {
            int id = 0;
            var repos = new Repository()
            {
                ReposID = 0,
                Key = key != null ? key : "",
                Data = data != null ? data : ""
            };
            _da.DbContext.Insert(repos);
            id = repos.ReposID;

            return id;
        }

        /// <summary>
        /// Get repository object by Id or Key
        /// </summary>
        /// <param name="reposID">Id or 0 (0 - ignore Id)</param>
        /// <param name="key">Key or null (null - ignore Key)</param>
        /// <returns></returns>
        public CommonData.ReposObject Select(int reposID = 0, string key = null)
        {
            Repository repos = null;
            repos = _da.DbContext.Table<Repository>().FirstOrDefault(s =>
                (s.ReposID == (reposID == 0 ? s.ReposID : reposID))
                && (s.Key == (key == null ? s.Key : key)));
            if (repos == null) return null;

            return new CommonData.ReposObject()
            {
                ReposID = repos.ReposID,
                Key = repos.Key,
                Data = repos.Data
            };
        }

        /// <summary>
        /// Update repository object
        /// </summary>
        /// <param name="ro"></param>
        public void Update(CommonData.ReposObject ro)
        {
            Repository db_repos = new Repository()
            {
                ReposID = ro.ReposID,
                Key = ro.Key != null ? ro.Key : "",
                Data = ro.Data != null ? ro.Data : ""
            };

            _da.DbContext.Update(db_repos);
        }

        /// <summary>
        /// Delete from repository by Id
        /// </summary>
        /// <param name="reposID"></param>
        public void Delete(int reposID)
        {
            try
            {
                _da.DbContext.Execute("DELETE FROM Repository WHERE ReposID = ?", reposID);
            }
            catch (Exception ex)
            {
                throw new Exception("Delete from repository error", ex);
            }
        }
    }
}
