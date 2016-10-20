using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using RapidPliant.App.Controls;
using RapidPliant.WPF.Mvx;
using MainWindow = RapidPliant.WPF.Controls.MainWindow;

namespace RapidPliant.App
{
    public abstract class RapidPliantAppliction
    {
        public Window MainWindow { get; protected set; }
        public Control MainContent { get; protected set; }

        public void Run()
        {
            var app = CreateApp();
            app.InitializeComponent();
            
            //Run the app with the main window!
            MainWindow = CreateMainWindow();

            InitializeMainWindow(MainWindow);
            
            MainWindow.Loaded += MainWindowOnLoaded;

            app.Run(MainWindow);
        }

        private void MainWindowOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (MainContent != null)
            {
                Mvx.LoadView(MainContent);
            }            
        }

        private void InitializeMainWindow(Window mainWindow)
        {
            MainContent = CreateMainContent();
            if (MainContent != null)
            {
                mainWindow.Content = MainContent;
            }
        }

        protected virtual Window CreateMainWindow()
        {
            return new MainWindow();
        }

        protected virtual Control CreateMainContent()
        {
            return null;
        }

        protected virtual App CreateApp()
        {
            return new App();
        }

        public class App : Application
        {
            public virtual void InitializeComponent()
            {
            }
        }
    }
    
}
