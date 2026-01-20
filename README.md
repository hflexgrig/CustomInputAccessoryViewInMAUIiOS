How to customize InputAccessoryView on iOS?

Here's how it looks by default, as you see for native Editor control when focused, above the soft keyboard there's blue round button with white chekmark appears, tapping which will close the keyboard. Similar thing, but even worse you can see, when you have WebView (in our case HybridWebView) control with html textarea, it shows big semitransparent white panel with up and sown arrows at left and checkmark at right, which again closes the keyboard when tapping.



https://github.com/user-attachments/assets/09acf5a7-6842-4d39-9aff-bec103d4d904

Our goal is to customize them by using Custom Handlers. In case of native editor it is pretty straightforward, but in case of HybridWebView I've spent few intense days to make it work in debug mode, then another few days of nightmare to make it work in TestFlight release version and make my real app distributable. This is how it looks after our customizations, for sample project we added just 3 butttons simulating adding bold, italic and underline buttons, and the custom close keyboard button. 



https://github.com/user-attachments/assets/43e73ca6-107b-4bfc-9771-4029d20e03fb


You can also completely remove it by overriding inputAccessoryView to return null in MauiTextView child class

```
private sealed class AccessoryTextView : MauiTextView
	{
		public AccessoryTextView() : base(CGRect.Empty)
		{
		}

		public UIView? CustomAccessoryView { get; set; }

		public override UIView? InputAccessoryView => Null;
	}
```
