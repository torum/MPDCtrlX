using MPDCtrlX.Models;
using System.Collections.ObjectModel;

namespace MPDCtrlX.Services.Contracts
{
    internal interface IConfigurationService
    {
        void Initialize(string configFilePath);
        Task<Models.Configuration> LoadConfigurationAsync();
        Task SaveConfigurationAsync();
        Models.Configuration CurrentConfiguration { get; }
    }
}