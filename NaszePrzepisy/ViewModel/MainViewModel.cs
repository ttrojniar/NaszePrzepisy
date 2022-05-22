using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using NaszePrzepisy.Model;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace NaszePrzepisy.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IDataService _dataService;
        private TabItem selectedTab;
        public TabItem SelectedTab
        {
            get { return selectedTab; }
            set
            {
                Set(ref selectedTab,value);
                var t = SelectedTab.Header.ToString();
                Messenger.Default.Send(SelectedTab.Header.ToString(),"wybranyposilek");
            }
        }

        public MainViewModel(IDataService dataService)
        {
            Model.Statics.Methods.CheckIfXMLExists();
            Model.Statics.Methods.slownikPrzepisow = Model.Statics.Methods.GetRecipes();
           
            _dataService = dataService;
            _dataService.GetData(
                (item, error) =>
                {
                    if (error != null)
                    {
                        // Report error here
                        return;
                    }

                    
                });
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}