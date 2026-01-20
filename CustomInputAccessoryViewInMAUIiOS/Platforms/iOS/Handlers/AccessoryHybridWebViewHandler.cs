using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Foundation;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;
using WebKit;

namespace CustomInputAccessoryViewInMAUIiOS.Platforms.iOS.Handlers;

public sealed class AccessoryHybridWebViewHandler : HybridWebViewHandler
{
	private const int AccessoryHeight = 44;
	private static readonly NSObject AccessoryViewKey = new();
	private static readonly object SwizzleLock = new();
	private static bool _isSwizzled;

	protected override void ConnectHandler(WKWebView platformView)
	{
		base.ConnectHandler(platformView);

		platformView.InputAssistantItem.LeadingBarButtonGroups = Array.Empty<UIBarButtonItemGroup>();
		platformView.InputAssistantItem.TrailingBarButtonGroups = Array.Empty<UIBarButtonItemGroup>();

		ConfigureInputAccessoryView(platformView);
	}

	protected override void DisconnectHandler(WKWebView platformView)
	{
		ClearAccessoryView(platformView);
		base.DisconnectHandler(platformView);
	}

	private void ConfigureInputAccessoryView(WKWebView webView)
	{
		if (!TrySwizzleInputAccessoryView())
		{
			return;
		}

		AssignAccessoryView(webView);
		ClearInputAssistantItems(webView);
		webView.ReloadInputViews();
	}

	private static void AssignAccessoryView(WKWebView webView)
	{
		var contentView = FindContentView(webView);
		if (contentView is null)
		{
			return;
		}

		var existing = objc_getAssociatedObject(contentView.Handle, AccessoryViewKey.Handle);
		if (existing != IntPtr.Zero)
		{
			return;
		}

		var accessoryView = CreateAccessoryView(webView);
		objc_setAssociatedObject(contentView.Handle, AccessoryViewKey.Handle, accessoryView.Handle, OBJC_ASSOCIATION_RETAIN_NONATOMIC);
	}

	private static UIView CreateAccessoryView(WKWebView webView)
	{
		var container = new UIView(new CoreGraphics.CGRect(0, 0, 0, AccessoryHeight))
		{
			BackgroundColor = UIColor.SystemBackground,
			AutoresizingMask = UIViewAutoresizing.FlexibleWidth
		};

		var stack = new UIStackView
		{
			Axis = UILayoutConstraintAxis.Horizontal,
			Alignment = UIStackViewAlignment.Center,
			Distribution = UIStackViewDistribution.EqualSpacing,
			Spacing = 12,
			TranslatesAutoresizingMaskIntoConstraints = false
		};

		stack.AddArrangedSubview(CreateTextButton("B", _ => Evaluate(webView, "applyFormat('bold')")));
		stack.AddArrangedSubview(CreateTextButton("I", _ => Evaluate(webView, "applyFormat('italic')")));
		stack.AddArrangedSubview(CreateTextButton("U", _ => Evaluate(webView, "applyFormat('underline')")));
		stack.AddArrangedSubview(CreateTextButton("⌄", _ => webView.EndEditing(true)));

		container.AddSubview(stack);

		NSLayoutConstraint.ActivateConstraints(new[]
		{
			stack.LeadingAnchor.ConstraintEqualTo(container.LeadingAnchor, 16),
			stack.TrailingAnchor.ConstraintLessThanOrEqualTo(container.TrailingAnchor, -16),
			stack.CenterYAnchor.ConstraintEqualTo(container.CenterYAnchor)
		});

		return container;
	}

	private static UIButton CreateTextButton(string title, Action<UIButton> onClick)
	{
		var button = new UIButton(UIButtonType.System);
		button.SetTitle(title, UIControlState.Normal);
		button.TitleLabel.Font = UIFont.SystemFontOfSize(17, UIFontWeight.Semibold);
		button.TouchUpInside += (_, __) => onClick(button);
		return button;
	}

	private static void Evaluate(WKWebView webView, string js)
	{
		webView.EvaluateJavaScript($"window.{js}", (_, __) => { });
	}

	private static void ClearInputAssistantItems(WKWebView webView)
	{
		webView.InputAssistantItem.LeadingBarButtonGroups = Array.Empty<UIBarButtonItemGroup>();
		webView.InputAssistantItem.TrailingBarButtonGroups = Array.Empty<UIBarButtonItemGroup>();

		webView.ScrollView.InputAssistantItem.LeadingBarButtonGroups = Array.Empty<UIBarButtonItemGroup>();
		webView.ScrollView.InputAssistantItem.TrailingBarButtonGroups = Array.Empty<UIBarButtonItemGroup>();

		var contentView = FindContentView(webView);
		if (contentView is not null)
		{
			contentView.InputAssistantItem.LeadingBarButtonGroups = Array.Empty<UIBarButtonItemGroup>();
			contentView.InputAssistantItem.TrailingBarButtonGroups = Array.Empty<UIBarButtonItemGroup>();
		}
	}

	private static UIView? FindContentView(WKWebView webView)
	{
		foreach (var subview in webView.ScrollView.Subviews)
		{
			if (subview.Class.Name == "WKContentView")
			{
				return subview;
			}
		}

		return null;
	}

	private static unsafe bool TrySwizzleInputAccessoryView()
	{
		if (_isSwizzled)
		{
			return true;
		}

		lock (SwizzleLock)
		{
			if (_isSwizzled)
			{
				return true;
			}

			var selector = sel_registerName("inputAccessoryView");
			var imp = (IntPtr)(delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr>)&ReturnAccessoryView;

			var targets = new[]
			{
				"WKContentView",
				"_WKContentView",
				"WKScrollView"
			};

			var didSwizzle = false;
			foreach (var targetName in targets)
			{
				var targetClass = objc_getClass(targetName);
				if (targetClass == IntPtr.Zero)
				{
					continue;
				}

				var targetMethod = class_getInstanceMethod(targetClass, selector);
				if (targetMethod == IntPtr.Zero)
				{
					continue;
				}

				method_setImplementation(targetMethod, imp);
				didSwizzle = true;
			}

			if (!didSwizzle)
			{
				return false;
			}

			_isSwizzled = true;
			return true;
		}
	}

	private static void ClearAccessoryView(WKWebView webView)
	{
		var contentView = FindContentView(webView);
		if (contentView is null)
		{
			return;
		}

		objc_setAssociatedObject(contentView.Handle, AccessoryViewKey.Handle, IntPtr.Zero, OBJC_ASSOCIATION_RETAIN_NONATOMIC);
	}

	[DllImport("/usr/lib/libobjc.dylib")]
	private static extern IntPtr objc_getClass(string name);

	[DllImport("/usr/lib/libobjc.dylib")]
	private static extern IntPtr sel_registerName(string name);

	[DllImport("/usr/lib/libobjc.dylib")]
	private static extern IntPtr class_getInstanceMethod(IntPtr cls, IntPtr sel);

	[DllImport("/usr/lib/libobjc.dylib")]
	private static extern IntPtr method_setImplementation(IntPtr method, IntPtr imp);

	[UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
	private static IntPtr ReturnAccessoryView(IntPtr self, IntPtr cmd)
	{
		return objc_getAssociatedObject(self, AccessoryViewKey.Handle);
	}

	[DllImport("/usr/lib/libobjc.dylib")]
	private static extern IntPtr objc_getAssociatedObject(IntPtr obj, IntPtr key);

	[DllImport("/usr/lib/libobjc.dylib")]
	private static extern void objc_setAssociatedObject(IntPtr obj, IntPtr key, IntPtr value, nint policy);

	private const int OBJC_ASSOCIATION_RETAIN_NONATOMIC = 1;
}
