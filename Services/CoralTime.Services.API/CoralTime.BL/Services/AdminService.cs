using CoralTime.BL.Interfaces;
using CoralTime.DAL.Repositories;

namespace CoralTime.BL.Services
{
    public class AdminService: BaseService, IAdminService
    {
        public AdminService(UnitOfWork uow)
            : base(uow) { }

        public void ResetCache()
        {
            Uow.UserRepository.LinkedCacheClear();
            Uow.UserRepository.LinkedCacheGetList();
        }
    }
}
