using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HangFireApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            //Configura��o do HangFire
            services.AddHangfire( op => {
                op.UseMemoryStorage();
            });

            services.AddHangfireServer();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "HangFireApi", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HangFireApi v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseHangfireDashboard();
            RegistrarJob();
        }

        public void RegistrarJob()
        {
            // Fire-and-forget- Jobs - S�o executados apenas uma �nica vez e quase que
            // imediatamente ap�s a cria��o
            BackgroundJob.Enqueue(() => MeuPrimeiroJobFireAndForget());


            // Recurring Jobs - Faz o agendamento recorrente do processo a ser executado.
            // S�o tarefas que executam de tempos em tempos
            RecurringJob.AddOrUpdate(() => Console.WriteLine("Recurring Job"), Cron.Hourly );


            // Delayed Jobs - Faz o agendamento do processo a ser executado.
            BackgroundJob.Schedule(() => Console.WriteLine("Delayed Job"), TimeSpan.FromDays(2));


            // Continuations - S�o "tarefas filhas" cujas execuc��es acontecem ap�s a tarefa pai
            // ser processada. Faz com que um processo seja executado novamente, pelo Id da execu��o
            string jobId = BackgroundJob.Enqueue(() => Console.WriteLine("Tarefa Pai"));
            BackgroundJob.ContinueJobWith(jobId, () => Console.WriteLine("Tarefa Filha"));

            BackgroundJob.Enqueue(() => MeuJob(null));
        }

        public void MeuJob(PerformContext performContext)
        {
            //var context = CreateContext();
            //var contextCopy = new PerformContext(context);
            string jobId = performContext.BackgroundJob.Id;
            Console.WriteLine(jobId);
        }

        public async Task MeuPrimeiroJobFireAndForget()
        {
            await Task.Run(() =>
            {

                Console.WriteLine("Bem-Vindo ao HangFire!");
            });
        }
    }
}
