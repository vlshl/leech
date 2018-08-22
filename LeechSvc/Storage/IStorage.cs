using SQLite.Net;
using System.Threading.Tasks;

namespace Storage
{
    /// <summary>
    /// Storage object base interface
    /// </summary>
    public interface IStorage
    {
        SQLiteConnection DbContext { get; }
        void CloseDbContext();
        bool BeginTransaction();
        void Commit(bool isNewTran);
        void Rollback(bool isNewTran);
    }
}
