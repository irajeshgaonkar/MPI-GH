using HCA.Data.Entities;
using HCA.Data.Repository.Core;

namespace HCA.Data.Repository
{
    public interface IAppRoleMappingRepository : IRepositoryBase<AppRoleMappingEntity>
    {
        /// <summary>
        /// Get App Role Mappings for groups
        /// </summary>
        /// <param name="groups"></param>
        /// <returns></returns>
        Task<List<AppRoleMappingEntity>> GetAppRoleMappingsAsync(List<string> groups);
    }
}
