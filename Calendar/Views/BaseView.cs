using Calendar.ViewModels;

namespace Calendar.Views;

public abstract class BaseView<TViewModel> : BaseView where TViewModel : BaseViewModel
{
    public BaseView(TViewModel viewModel) : base(viewModel)
    {
    }

    public new TViewModel BindingContext => (TViewModel)base.BindingContext;
}

public abstract class BaseView : ContentView
{
    protected BaseView(object viewModel = null)
    {
        BindingContext = viewModel;
    }
}
