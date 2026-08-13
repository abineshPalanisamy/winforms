// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.WinForms;
using System.Windows.Forms;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
///  Provides extension methods for configuring Windows Forms hosting in an
///  <see cref="IHostBuilder"/> or <see cref="IHostApplicationBuilder"/>.
/// </summary>
public static class WinFormsHostingExtensions
{
    /// <summary>
    ///  Adds the Windows Forms hosted service infrastructure to <paramref name="services"/>,
    ///  using <typeparamref name="TForm"/> as the application main form.
    /// </summary>
    /// <typeparam name="TForm">
    ///  The <see cref="Form"/>-derived type to use as the application's main form.
    /// </typeparam>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">
    ///  <paramref name="services"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddWinFormsMainForm<TForm>(this IServiceCollection services)
        where TForm : Form
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<TForm>();

        // Register the main form type in the registration helper.
        services.AddSingleton<GuiContextRegistration>(_ => new GuiContextRegistration
        {
            MainFormType = typeof(TForm)
        });

        return services;
    }

    /// <summary>
    ///  Adds the Windows Forms hosted service to the service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">
    ///  <paramref name="services"/> is <see langword="null"/>.
    /// </exception>
    public static IServiceCollection AddWinFormsHostedService(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IGuiContext>(sp =>
            sp.GetRequiredService<GuiContextRegistration>().WaitForGuiContextAsync().GetAwaiter().GetResult());

        services.AddSingleton<IFormProvider, FormProvider>();
        services.AddHostedService<WinFormsHostedService>();

        return services;
    }

    /// <summary>
    ///  Adds Windows Forms hosting to the host, configuring the main form and applying the
    ///  provided <paramref name="configureOptions"/>.
    /// </summary>
    /// <typeparam name="TForm">
    ///  The <see cref="Form"/>-derived type to use as the application's main form.
    /// </typeparam>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configureOptions">
    ///  An optional delegate to configure <see cref="WinFormsHostingOptions"/>.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddWindowsFormsLifetime<TForm>(
        this IServiceCollection services,
        Action<WinFormsHostingOptions>? configureOptions = null)
        where TForm : Form
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<WinFormsHostingOptions>();

        if (configureOptions is not null)
        {
            services.Configure(configureOptions);
        }

        services.AddWinFormsMainForm<TForm>();
        services.AddWinFormsHostedService();

        return services;
    }
}
