/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:PowerliminalsPlayer"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using System;
using System.Windows;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CommonServiceLocator;
using CommonServiceLocator.WindsorAdapter;
using HanumanInstitute.FFmpegExampleApplication.Business;
using HanumanInstitute.FFmpeg;
using MvvmDialogs;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;

namespace HanumanInstitute.FFmpegExampleApplication.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable
    public class ViewModelLocator
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
        //private static ViewModelLocator s_instance;
        //public static ViewModelLocator Instance => s_instance ?? (s_instance = (ViewModelLocator)Application.Current.FindResource("Locator"));

        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class¸with Windsor Castle IoC.
        /// </summary>
        public ViewModelLocator()
        {
            var container = new WindsorContainer();
            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));

            container.AddFFmpeg();
            container.Register(Component.For<IDialogService>().ImplementedBy<DialogService>()
                .DependsOn(Dependency.OnValue("dialogTypeLocator", new AppDialogTypeLocator())).LifeStyle.Transient);

            container.Register(Component.For<MainViewModel>().ImplementedBy<MainViewModel>().LifeStyle.Transient);
            container.Register(Component.For<IFFmpegErrorViewModel>().ImplementedBy<FFmpegErrorViewModel>().LifeStyle.Transient);
            container.Register(Component.For<IFFmpegUiViewModel>().ImplementedBy<FFmpegUiViewModel>().LifeStyle.Transient);
            container.Register(Component.For<IUserInterfaceManager>().ImplementedBy<FFmpegUserInterfaceManager>().LifeStyle.Singleton);
            container.Register(Component.For<IFFmpegUserInterfaceFactory>().ImplementedBy<FFmpegUserInterfaceFactory>().LifeStyle.Singleton);

            container.Dispose();
        }

        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class with SimpleIoc.
        /// </summary>
        //public ViewModelLocator()
        //{
        //    ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
        //    SimpleIoc.Default.Reset();

        //    SimpleIoc.Default.AddFFmpeg();
        //    SimpleIoc.Default.Register<IDialogService>(() => new DialogService(dialogTypeLocator: new AppDialogTypeLocator()));

        //    SimpleIoc.Default.Register<MainViewModel>();
        //    SimpleIoc.Default.Register<IFFmpegErrorViewModel, FFmpegErrorViewModel>();
        //    SimpleIoc.Default.Register<IFFmpegUiViewModel, FFmpegUiViewModel>();
        //    SimpleIoc.Default.Register<IUserInterfaceManager, FFmpegUserInterfaceManager>();
        //    SimpleIoc.Default.Register<IFFmpegUserInterfaceFactory, FFmpegUserInterfaceFactory>();
        //}

        public static MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
        public static FFmpegErrorViewModel FFmpegError => GetDesignViewModel(() => ServiceLocator.Current.GetInstance<IFFmpegErrorViewModel>() as FFmpegErrorViewModel);
        public static FFmpegUiViewModel FFmpegUi => GetDesignViewModel(() => ServiceLocator.Current.GetInstance<IFFmpegUiViewModel>() as FFmpegUiViewModel);

        /// <summary>
        /// Returns a ViewModel only in design-time. DataContext on Views will be null at runtime until initialized by MvvmDialog.
        /// </summary>
        private static T GetDesignViewModel<T>(Func<T> viewModel) 
            where T : class
        {
            return ViewModelBase.IsInDesignModeStatic ? viewModel() : (T)null;
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}
