using Unity;
using System.Web.Http;
using Unity.WebApi;
using FileBrowser.BLL;
using System.IO.Abstractions;
using System.Web.Configuration;

public static class UnityConfig
{
    public static void RegisterComponents()
    {
        var container = new UnityContainer();

        string homeDirectory = WebConfigurationManager.AppSettings["HomeDirectoryPath"];

        // e.g., container.RegisterType<IProductRepository, ProductRepository>();
        container.RegisterType<IFileBrowserService, FileBrowserService>();
        container.RegisterType<IFileSystem, FileSystem>();

        GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
    }
}
