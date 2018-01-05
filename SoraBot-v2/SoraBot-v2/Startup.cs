using Microsoft.AspNetCore.Builder;

namespace SoraBot_v2
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseStaticFiles();
            app.UseCors(builder => builder.WithOrigins("localhost"));
            app.UseMvc();
        }
    }
}