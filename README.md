buymeacoffee.com/hakobgrigoryan

<img width="220" height="220" alt="qr-code" src="https://github.com/user-attachments/assets/6619eb0a-9d99-4dc0-9afa-3be56dbb6f46" />


How to Customize InputAccessoryView on iOS

By default, when a native editor control receives focus on iOS, a system-provided InputAccessoryView appears above the on-screen keyboard. As shown below, this typically includes a blue rounded button with a white checkmark, which dismisses the keyboard when tapped.

A similar—but even less desirable—behavior occurs when using a WebView (in our case, a HybridWebView) containing an HTML <textarea>. In this scenario, iOS displays a large semi-transparent white panel above the keyboard, featuring up and down arrow buttons on the left and a checkmark button on the right. Tapping the checkmark again dismisses the keyboard.

This default UI is often unnecessary, visually intrusive, and difficult to customize, especially when building rich text editors or custom input experiences.



https://github.com/user-attachments/assets/09acf5a7-6842-4d39-9aff-bec103d4d904

Our goal is to fully customize this accessory view using custom handlers. For the native editor, this is relatively straightforward. However, achieving the same result for a HybridWebView proved to be significantly more challenging.

While the solution initially worked in Debug mode, it took several intense days to make it function correctly in a TestFlight release build. Turning that into a stable, distributable solution for a real production app was, frankly, a nightmare—mostly due to subtle iOS behavior differences between debug and release configurations.

After overcoming these issues, this is how the customized accessory view looks. In the sample project, we added three toolbar buttons to simulate Bold, Italic, and Underline actions, along with a custom “Close Keyboard” button.



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
