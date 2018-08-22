using System.Threading.Tasks;

namespace Common.Interfaces
{
    /// <summary>
    /// All DataStorage objects base interface
    /// </summary>
    public interface IDataStorage
    {
        void Initialize();
        void Close();
    }
}
