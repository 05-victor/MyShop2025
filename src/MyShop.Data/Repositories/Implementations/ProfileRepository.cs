using Microsoft.EntityFrameworkCore;
using MyShop.Data.Entities;
using MyShop.Data.Repositories.Interfaces;

namespace MyShop.Data.Repositories.Implementations
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly ShopContext _context;

        public ProfileRepository(ShopContext context)
        {
            _context = context;
        }

        public async Task<Profile?> GetByUserIdAsync(Guid userId)
        {
            return await _context.Profiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<Profile> CreateAsync(Profile profile)
        {
            _context.Profiles.Add(profile);
            await _context.SaveChangesAsync();
            return profile;
        }

        public async Task<Profile> UpdateAsync(Profile profile)
        {
            _context.Profiles.Update(profile);
            await _context.SaveChangesAsync();
            return profile;
        }

        public async Task DeleteAsync(Guid userId)
        {
            var profile = await GetByUserIdAsync(userId);
            if (profile != null)
            {
                _context.Profiles.Remove(profile);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid userId)
        {
            return await _context.Profiles.AnyAsync(p => p.UserId == userId);
        }
    }
}