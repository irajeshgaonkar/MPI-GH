using HCA.Data.Entities;
using HCA.Data.Repository.Core;
using Microsoft.EntityFrameworkCore;

namespace HCA.Data.Repository.Impl
{
    public class AppRoleMappingRepository : RepositoryBase<AppRoleMappingEntity>, IAppRoleMappingRepository
    {
        private readonly HcaDbContext _hcaDbContext;
        public AppRoleMappingRepository(HcaDbContext dbContext) : base(dbContext)
        {
            _hcaDbContext = dbContext;
        }

        /// <summary>
        /// Get App Role Mappings for groups
        /// </summary>
        /// <param name="groups"></param>
        /// <returns></returns>
        public async Task<List<AppRoleMappingEntity>> GetAppRoleMappingsAsync(List<string> groups)
        {
            var results = await _hcaDbContext.AppRoleMappings.Where(arm => groups.Any(group => arm.RoleGroupName == group))
            .Include(mapping => mapping.AppRole).Include(s => s.System).ToListAsync();
            return results;
        }
    }
}
