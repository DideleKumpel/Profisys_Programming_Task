using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Profisys_Programming_Task.Model;
using Profisys_Programming_Task.Service.DbService;
using Profisys_Programming_Task.Service.Export;
using Profisys_Programming_Task.Service.Import;
using Profisys_Programming_Task.ViewModel;
using Profisys_Programming_Task.ViewModel.DialogViewModel;
using System.Configuration;
using System.Windows;

namespace Profisys_Programming_Task
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }
        public App()
        {
            ServiceCollection services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(options => 
            {
                string connection = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                options.UseSqlServer(connection);
            });

            services.AddScoped<IDbService<Documents>, DocumentsDbService>();
            services.AddScoped<IDocumentItemsDbService, DocumentItemsDbService>();
            services.AddScoped<IImportService<Documents>, DocuemntsImportService>();
            services.AddScoped<IImportService<DocumentItems>, DocumentItemsImportService>();
            services.AddScoped<IExportService<Documents>, ExportServiceBase<Documents>>();
            services.AddScoped<IExportService<DocumentItems>, DocumentItemsExportService>();
            services.AddScoped<DocumentPdfExportService>();

            services.AddTransient<MainMenuViewModel>();
            services.AddTransient<DataViewModel>();
            services.AddTransient<ImportViewModel>();
            services.AddTransient<DocumentsItemDialogViewModel>();
            services.AddTransient<ExportViewModel>();

            ServiceProvider = services.BuildServiceProvider();
        }
    }

}
