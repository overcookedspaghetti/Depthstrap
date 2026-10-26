using System.Windows.Input;
using Depthstrap.UI.ViewModels;

namespace Depthstrap.Models
{
    internal class ServerEntry : NotifyPropertyChangedViewModel
    {
        public int Number { get; set; }
        public string ServerId { get; set; } = null!;
        public string Region { get; set; } = null!;
        public int? DataCenterId { get; set; }
        public string Uptime { get; set; } = "Loading...";
        public ICommand? JoinCommand { get; set; }
    }
}