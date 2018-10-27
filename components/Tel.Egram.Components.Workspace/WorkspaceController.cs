using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Avalonia.Threading;
using ReactiveUI;
using Tel.Egram.Components.Messenger;
using Tel.Egram.Components.Messenger.Catalog;
using Tel.Egram.Components.Settings;
using Tel.Egram.Components.Workspace.Navigation;
using Tel.Egram.Gui.Views.Messenger;
using Tel.Egram.Gui.Views.Settings;
using Tel.Egram.Gui.Views.Workspace;
using Tel.Egram.Messaging.Chats;
using Tel.Egram.Utils;

namespace Tel.Egram.Components.Workspace
{
    public class WorkspaceController : BaseController<WorkspacePageModel>
    {
        private readonly IActivator<NavigationControlModel> _navigationActivator;
        private IController<NavigationControlModel> _navigationController;
        
        private readonly IActivator<Section, MessengerControlModel> _messengerActivator;
        private IController<MessengerControlModel> _messengerController;
        
        private readonly IActivator<SettingsControlModel> _settingsActivator;
        private IController<SettingsControlModel> _settingsController;

        public WorkspaceController(
            IActivator<NavigationControlModel> navigationActivator,
            IActivator<Section, MessengerControlModel> messengerActivator,
            IActivator<SettingsControlModel> settingsActivator)
        {
            _navigationActivator = navigationActivator;
            _messengerActivator = messengerActivator;
            _settingsActivator = settingsActivator;

            BindNavigation()
                .DisposeWith(this);
        }

        private IDisposable BindNavigation()
        {
            var model = _navigationActivator.Activate(ref _navigationController);
            Model.NavigationControlModel = model;
            
            return Model.NavigationControlModel.WhenAnyValue(m => m.SelectedTabIndex)
                .Select(index => (ContentKind)index)
                .SubscribeOn(TaskPoolScheduler.Default)
                .ObserveOn(AvaloniaScheduler.Instance)
                .Subscribe(kind =>
                {
                    switch (kind)
                    {
                        case ContentKind.Settings:
                            Model.ContentIndex = 1;
                            InitSettings();
                            break;
                        
                        default:
                            Model.ContentIndex = 0;
                            InitMessenger(kind);
                            break;
                    }
                });
        }

        private void InitSettings()
        {
            if (Model.SettingsControlModel == null)
            {   
                _messengerController?.Dispose();
                _messengerController = null;
                Model.MessengerControlModel = null;
                
                var model = _settingsActivator.Activate(ref _settingsController);
                Model.SettingsControlModel = model;
            }
        }

        private void InitMessenger(ContentKind kind)
        {
            var section = (Section) kind;
            
            if (Model.MessengerControlModel == null)
            {
                _settingsController?.Dispose();
                _settingsController = null;
                Model.SettingsControlModel = null;
                
                var model = _messengerActivator.Activate(section, ref _messengerController);
                Model.MessengerControlModel = model;
            }
        }

        public override void Dispose()
        {
            _navigationController?.Dispose();
            _messengerController?.Dispose();
            _settingsController?.Dispose();
            
            base.Dispose();
        }
    }
}