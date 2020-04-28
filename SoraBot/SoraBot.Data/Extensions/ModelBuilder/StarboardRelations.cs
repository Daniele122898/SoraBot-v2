using Microsoft.EntityFrameworkCore;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Extensions.ModelBuilder
{
    public static class StarboardRelations
    {
        public static Microsoft.EntityFrameworkCore.ModelBuilder AddStarboardRelations(
            this Microsoft.EntityFrameworkCore.ModelBuilder mb)
        {
            mb.Entity<Guild>()
                .Property(p => p.StarboardThreshold)
                .HasDefaultValue(1);

            return mb;
        }
    }
}