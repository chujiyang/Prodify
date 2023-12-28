#if IOS
using Microsoft.Maui.Platform;
using Calendar.Platforms.iOS.Utils;
using UIKit;
using CoreGraphics;
#endif

namespace Calendar.Views;

public class FixedKeyboardView : Grid
{
    public FixedKeyboardView()
    {
#if IOS
        UIKeyboard.Notifications.ObserveWillShow(OnKeyboardShowing);
        UIKeyboard.Notifications.ObserveWillHide(OnKeyboardHiding);
#endif
    }

#if IOS
    private void OnKeyboardShowing(object sender, UIKeyboardEventArgs args)
    {
        if (Shell.Current.CurrentPage is ContentPage page)
        {
            UIView control = this.ToPlatform(Handler.MauiContext).FindFirstResponder();
            if (control == null)
            {
                return;
            }

            UIView rootUiView = page.Content.ToPlatform(Handler.MauiContext);
            CGRect kbFrame = UIKeyboard.FrameEndFromNotification(args.Notification);
            double distance = control.GetOverlapDistance(rootUiView, kbFrame);
            if (distance > 0)
            {
                Margin = new Thickness(Margin.Left, Margin.Top, Margin.Right, distance);
            }
        }
    }

    private void OnKeyboardHiding(object sender, UIKeyboardEventArgs args)
    {
        Margin = new Thickness(Margin.Left, Margin.Top, Margin.Right, 0);
    }
#endif
}
