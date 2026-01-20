using CoreGraphics;
using Foundation;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;

namespace CustomInputAccessoryViewInMAUIiOS.Platforms.iOS.Handlers;

public sealed class AccessoryEditorHandler : EditorHandler
{
	private const int AccessoryHeight = 44;

	protected override MauiTextView CreatePlatformView() => new AccessoryTextView();

	protected override void ConnectHandler(MauiTextView platformView)
	{
		base.ConnectHandler(platformView);

		platformView.InputAssistantItem.LeadingBarButtonGroups = Array.Empty<UIBarButtonItemGroup>();
		platformView.InputAssistantItem.TrailingBarButtonGroups = Array.Empty<UIBarButtonItemGroup>();

		if (platformView is AccessoryTextView accessoryTextView)
		{
			accessoryTextView.CustomAccessoryView = CreateAccessoryView(accessoryTextView);
			accessoryTextView.ReloadInputViews();
		}
	}

	private static UIView CreateAccessoryView(UITextView textView)
	{
		var container = new UIView(new CGRect(0, 0, 0, AccessoryHeight))
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

		stack.AddArrangedSubview(CreateTextButton("B", _ => WrapSelection(textView, "<b>", "</b>")));
		stack.AddArrangedSubview(CreateTextButton("I", _ => WrapSelection(textView, "<i>", "</i>")));
		stack.AddArrangedSubview(CreateTextButton("U", _ => WrapSelection(textView, "<u>", "</u>")));
		stack.AddArrangedSubview(CreateTextButton("⌄", _ => textView.EndEditing(true)));

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

	private static void WrapSelection(UITextView textView, string prefix, string suffix)
	{
		if (textView is null)
		{
			return;
		}

		var range = textView.SelectedRange;
		var existing = textView.Text ?? string.Empty;
		var start = (int)range.Location;
		var length = (int)range.Length;
		if (start < 0 || start > existing.Length)
		{
			return;
		}

		var selected = existing.Substring(start, length);
		var updated = existing.Substring(0, start) + prefix + selected + suffix + existing.Substring(start + length);
		textView.Text = updated;

		var newStart = start + prefix.Length;
		textView.SelectedRange = new NSRange(newStart, selected.Length);
	}

	private sealed class AccessoryTextView : MauiTextView
	{
		public AccessoryTextView() : base(CGRect.Empty)
		{
		}

		public UIView? CustomAccessoryView { get; set; }

		public override UIView? InputAccessoryView => CustomAccessoryView;
	}
}
