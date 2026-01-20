using Microsoft.Extensions.Logging;

namespace CustomInputAccessoryViewInMAUIiOS;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			}).Services
			.AddHybridWebViewDeveloperTools();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
