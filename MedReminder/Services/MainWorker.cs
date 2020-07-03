using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MedReminder.Services
{
    public interface IMainWorker
    {
        Task Run();
    }
    public class MainWorker : IMainWorker
    {
        private IYamlConfigService _yamlConfigService;
        public MainWorker(IYamlConfigService yamlConfigService)
        {
            _yamlConfigService = yamlConfigService;
        }
        public async Task Run()
        {
            throw new NotImplementedException();
        }
    }
}
