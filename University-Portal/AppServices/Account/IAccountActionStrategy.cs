using System.Threading.Tasks;
using University_Portal.ViewModels;

namespace University_Portal.AppServices.Account
{
    public interface IAccountActionStrategy<TModel>
    {
        Task<(bool Success, string Message)> ExecuteAsync(TModel model);
    }
}