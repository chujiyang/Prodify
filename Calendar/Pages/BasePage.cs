using System.Diagnostics;
using Calendar.ViewModels;

namespace Calendar.Pages
{
    public abstract class BasePage<TViewModel> : BasePage where TViewModel : BaseViewModel
    {
        protected BasePage(TViewModel viewModel) : base(viewModel)
        {
        }

        public new TViewModel BindingContext => (TViewModel)base.BindingContext;
    }

    public abstract class BasePage : ContentPage
    {
        protected BasePage(object viewModel = null)
        {
            BindingContext = viewModel;
            Padding = 12;

            if (string.IsNullOrWhiteSpace(Title))
            {
                Title = GetType().Name;
            }
        }

        ~BasePage()
        {
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            Debug.WriteLine($"OnAppearing: {Title}");
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            Debug.WriteLine($"OnDisappearing: {Title}");
        }
    }
}
