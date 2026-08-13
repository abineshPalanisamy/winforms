// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.ComponentModel;

/// <summary>
///  Represents a component that can receive a service provider after construction.
/// </summary>
/// <remarks>
///  <para>
///   Components that require access to services but are created before the DI container is
///   fully built (e.g., <see cref="System.Windows.Forms.Form"/> instances) implement this
///   interface so the builder infrastructure can inject the service provider once it is
///   available.
///  </para>
/// </remarks>
public interface IServiceProviderAssignable : IServiceProvider
{
    /// <summary>
    ///  Sets the resolved <see cref="IServiceProvider"/> for this component.
    /// </summary>
    /// <param name="serviceProvider">The resolved service provider from the DI container.</param>
    /// <returns>The assigned service provider.</returns>
    IServiceProvider SetServiceProvider(object serviceProvider);
}
