// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Drawing;

namespace System.Windows.Forms.Tests;

/// <summary>
///  Tests the behavior of <see cref="SplitContainer"/>.
/// </summary>
public class SplitContainerTests
{
    [WinFormsTheory]
    [InlineData(Orientation.Horizontal)]
    [InlineData(Orientation.Vertical)]
    public void SplitContainer_Net11MouseMove_UpdatesLayoutAndRaisesSplitterMovedOnCompletion(Orientation orientation)
    {
        using SubSplitContainer control = new()
        {
            Orientation = orientation,
            Size = new Size(300, 300),
            VisualStylesMode = VisualStylesMode.Net11
        };

        int splitterMovedCallCount = 0;
        control.SplitterMoved += (_, _) => splitterMovedCallCount++;

        Rectangle splitterRectangle = control.SplitterRectangle;
        int splitterX = splitterRectangle.Left + (splitterRectangle.Width / 2);
        int splitterY = splitterRectangle.Top + (splitterRectangle.Height / 2);
        int initialSplitterDistance = control.SplitterDistance;
        int firstX = orientation == Orientation.Vertical ? splitterX + 10 : splitterX;
        int firstY = orientation == Orientation.Horizontal ? splitterY + 10 : splitterY;
        int secondX = orientation == Orientation.Vertical ? splitterX + 20 : splitterX;
        int secondY = orientation == Orientation.Horizontal ? splitterY + 20 : splitterY;

        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, splitterX, splitterY, 0));
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, firstX, firstY, 0));

        Assert.Equal(initialSplitterDistance + 10, control.SplitterDistance);
        Assert.Equal(
            initialSplitterDistance + 10,
            orientation == Orientation.Vertical ? control.Panel1.Width : control.Panel1.Height);
        Assert.Equal(0, splitterMovedCallCount);

        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, secondX, secondY, 0));

        Assert.Equal(initialSplitterDistance + 20, control.SplitterDistance);
        Assert.Equal(
            initialSplitterDistance + 20,
            orientation == Orientation.Vertical ? control.Panel1.Width : control.Panel1.Height);
        Assert.Equal(0, splitterMovedCallCount);

        control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, secondX, secondY, 0));

        Assert.Equal(1, splitterMovedCallCount);
    }

    [WinFormsTheory]
    [InlineData(Orientation.Horizontal)]
    [InlineData(Orientation.Vertical)]
    public void SplitContainer_ClassicMouseMove_DefersLayoutUntilCompletion(Orientation orientation)
    {
        using SubSplitContainer control = new()
        {
            Orientation = orientation,
            Size = new Size(300, 300),
            VisualStylesMode = VisualStylesMode.Classic
        };

        int splitterMovedCallCount = 0;
        control.SplitterMoved += (_, _) => splitterMovedCallCount++;

        Rectangle splitterRectangle = control.SplitterRectangle;
        int splitterX = splitterRectangle.Left + (splitterRectangle.Width / 2);
        int splitterY = splitterRectangle.Top + (splitterRectangle.Height / 2);
        int initialSplitterDistance = control.SplitterDistance;
        int movedX = orientation == Orientation.Vertical ? splitterX + 10 : splitterX;
        int movedY = orientation == Orientation.Horizontal ? splitterY + 10 : splitterY;

        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, splitterX, splitterY, 0));
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, movedX, movedY, 0));

        Assert.Equal(initialSplitterDistance, control.SplitterDistance);
        Assert.Equal(0, splitterMovedCallCount);

        control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, movedX, movedY, 0));

        Assert.Equal(initialSplitterDistance + 10, control.SplitterDistance);
        Assert.Equal(1, splitterMovedCallCount);
    }

    [WinFormsFact]
    public void SplitContainer_Net11KeyMove_UpdatesLayoutAndRaisesSplitterMovedOnCompletion()
    {
        using SubSplitContainer control = new()
        {
            Size = new Size(300, 100),
            VisualStylesMode = VisualStylesMode.Net11
        };

        int splitterMovedCallCount = 0;
        control.SplitterMoved += (_, _) => splitterMovedCallCount++;
        control.TestAccessor.Dynamic._splitterFocused = true;
        int initialSplitterDistance = control.SplitterDistance;

        control.OnKeyDown(new KeyEventArgs(Keys.Right));

        Assert.Equal(initialSplitterDistance + control.SplitterIncrement, control.SplitterDistance);
        Assert.Equal(0, splitterMovedCallCount);

        control.OnKeyUp(new KeyEventArgs(Keys.Right));

        Assert.Equal(1, splitterMovedCallCount);
    }

    [WinFormsFact]
    public void SplitContainer_Net11MouseMove_CanceledMovementRestoresInitialLayoutWithoutSplitterMoved()
    {
        using SubSplitContainer control = new()
        {
            Size = new Size(300, 100),
            VisualStylesMode = VisualStylesMode.Net11
        };

        int splitterMovedCallCount = 0;
        control.SplitterMoved += (_, _) => splitterMovedCallCount++;

        Rectangle splitterRectangle = control.SplitterRectangle;
        int splitterX = splitterRectangle.Left + (splitterRectangle.Width / 2);
        int splitterY = splitterRectangle.Top + (splitterRectangle.Height / 2);
        int initialSplitterDistance = control.SplitterDistance;

        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, splitterX, splitterY, 0));
        Assert.True(control.Capture);

        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, splitterX + 10, splitterY, 0));
        control.SplitterMoving += (_, e) => e.Cancel = true;
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, splitterX + 20, splitterY, 0));

        Assert.Equal(initialSplitterDistance, control.SplitterDistance);
        Assert.Equal(initialSplitterDistance, control.Panel1.Width);
        Assert.Equal(0, splitterMovedCallCount);
        Assert.False(control.Capture);
    }

    [WinFormsFact]
    public void SplitContainer_Net11MouseMove_CanceledAtInitialDistanceRestoresAppliedLayout()
    {
        using SubSplitContainer control = new()
        {
            Size = new Size(300, 100),
            VisualStylesMode = VisualStylesMode.Net11
        };

        Rectangle splitterRectangle = control.SplitterRectangle;
        int splitterX = splitterRectangle.Left + (splitterRectangle.Width / 2);
        int splitterY = splitterRectangle.Top + (splitterRectangle.Height / 2);
        int initialSplitterDistance = control.SplitterDistance;

        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, splitterX, splitterY, 0));
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, splitterX + 10, splitterY, 0));
        control.SplitterMoving += (_, e) => e.Cancel = true;
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, splitterX, splitterY, 0));

        Assert.Equal(initialSplitterDistance, control.SplitterDistance);
        Assert.Equal(initialSplitterDistance, control.Panel1.Width);
        Assert.False(control.Capture);
    }

    [WinFormsTheory]
    [InlineData(VisualStylesMode.Disabled)]
    [InlineData(VisualStylesMode.Classic)]
    [InlineData(VisualStylesMode.Net11)]
    public void SplitContainer_MouseMove_ReentrantSplitterDistanceAndCancellationNotifiesRestoredDistance(
        VisualStylesMode visualStylesMode)
    {
        using SubSplitContainer control = new()
        {
            Size = new Size(300, 100),
            VisualStylesMode = visualStylesMode
        };

        List<int> notifiedDistances = [];
        control.SplitterMoved += (_, _) => notifiedDistances.Add(control.SplitterDistance);
        control.SplitterMoving += (_, e) =>
        {
            control.SplitterDistance = 70;
            e.Cancel = true;
        };

        Rectangle splitterRectangle = control.SplitterRectangle;
        int splitterX = splitterRectangle.Left + (splitterRectangle.Width / 2);
        int splitterY = splitterRectangle.Top + (splitterRectangle.Height / 2);
        int initialSplitterDistance = control.SplitterDistance;

        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, splitterX, splitterY, 0));
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, splitterX + 10, splitterY, 0));

        Assert.Equal(initialSplitterDistance, control.SplitterDistance);
        Assert.Equal([70, initialSplitterDistance], notifiedDistances);
    }

    [WinFormsFact]
    public void SplitContainer_Net11KeyMove_CanceledMovementTerminatesInteraction()
    {
        using SubSplitContainer control = new()
        {
            Size = new Size(300, 100),
            VisualStylesMode = VisualStylesMode.Net11
        };

        int splitterMovedCallCount = 0;
        control.SplitterMoved += (_, _) => splitterMovedCallCount++;
        control.SplitterMoving += (_, e) => e.Cancel = true;
        control.TestAccessor.Dynamic._splitterFocused = true;
        int initialSplitterDistance = control.SplitterDistance;

        control.OnKeyDown(new KeyEventArgs(Keys.Right));

        Assert.Equal(initialSplitterDistance, control.SplitterDistance);
        Assert.False(control.TestAccessor.Dynamic._splitBegin);
        Assert.False(control.TestAccessor.Dynamic._splitMove);
        Assert.Equal(0, splitterMovedCallCount);

        control.OnKeyUp(new KeyEventArgs(Keys.Right));

        Assert.Equal(initialSplitterDistance, control.SplitterDistance);
        Assert.Equal(0, splitterMovedCallCount);
    }

    [WinFormsFact]
    public void SplitContainer_Net11MouseMove_ReentrantSplitterDistanceDoesNotDuplicateCompletionNotification()
    {
        using SubSplitContainer control = new()
        {
            Size = new Size(300, 100),
            VisualStylesMode = VisualStylesMode.Net11
        };

        int splitterMovedCallCount = 0;
        control.SplitterMoved += (_, _) => splitterMovedCallCount++;
        control.SplitterMoving += (_, _) => control.SplitterDistance = 70;

        Rectangle splitterRectangle = control.SplitterRectangle;
        int splitterX = splitterRectangle.Left + (splitterRectangle.Width / 2);
        int splitterY = splitterRectangle.Top + (splitterRectangle.Height / 2);

        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, splitterX, splitterY, 0));
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, splitterX + 10, splitterY, 0));
        control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, splitterX + 10, splitterY, 0));

        Assert.Equal(70, control.SplitterDistance);
        Assert.Equal(1, splitterMovedCallCount);
    }

    [WinFormsTheory]
    [InlineData(VisualStylesMode.Net11, VisualStylesMode.Classic, true)]
    [InlineData(VisualStylesMode.Classic, VisualStylesMode.Net11, false)]
    public void SplitContainer_MouseMove_VisualStylesModeChangeUsesModeFromMouseDown(
        VisualStylesMode initialMode,
        VisualStylesMode changedMode,
        bool expectedLiveResize)
    {
        using SubSplitContainer control = new()
        {
            Size = new Size(300, 100),
            VisualStylesMode = initialMode
        };

        Rectangle splitterRectangle = control.SplitterRectangle;
        int splitterX = splitterRectangle.Left + (splitterRectangle.Width / 2);
        int splitterY = splitterRectangle.Top + (splitterRectangle.Height / 2);
        int initialSplitterDistance = control.SplitterDistance;

        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, splitterX, splitterY, 0));
        control.VisualStylesMode = changedMode;
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, splitterX + 10, splitterY, 0));

        Assert.Equal(
            expectedLiveResize ? initialSplitterDistance + 10 : initialSplitterDistance,
            control.SplitterDistance);

        control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, splitterX + 10, splitterY, 0));
        Assert.Equal(initialSplitterDistance + 10, control.SplitterDistance);
    }

    [WinFormsTheory]
    [InlineData(Orientation.Horizontal)]
    [InlineData(Orientation.Vertical)]
    public void SplitContainer_Net11MouseMove_WithHandleRepaintsNestedPanelContent(Orientation orientation)
    {
        using Form form = new();
        using SubSplitContainer control = new()
        {
            Dock = DockStyle.Fill,
            Orientation = orientation,
            Size = new Size(300, 300),
            VisualStylesMode = VisualStylesMode.Net11
        };
        using PaintTrackingControl panel1Content = CreateNestedPaintTrackingControl(control.Panel1);
        using PaintTrackingControl panel2Content = CreateNestedPaintTrackingControl(control.Panel2);
        form.Controls.Add(control);
        form.Show();
        Application.DoEvents();
        panel1Content.ResetPaintCallCount();
        panel2Content.ResetPaintCallCount();

        Rectangle splitterRectangle = control.SplitterRectangle;
        int splitterX = splitterRectangle.Left + (splitterRectangle.Width / 2);
        int splitterY = splitterRectangle.Top + (splitterRectangle.Height / 2);
        int movedX = orientation == Orientation.Vertical ? splitterX + 10 : splitterX;
        int movedY = orientation == Orientation.Horizontal ? splitterY + 10 : splitterY;

        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, splitterX, splitterY, 0));
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, movedX, movedY, 0));

        Assert.True(control.IsHandleCreated);
        Assert.True(panel1Content.PaintCallCount > 0);
        Assert.True(panel2Content.PaintCallCount > 0);
    }

    private static PaintTrackingControl CreateNestedPaintTrackingControl(Control parent)
    {
        Panel nestedParent = new()
        {
            Dock = DockStyle.Fill
        };
        PaintTrackingControl content = new()
        {
            Dock = DockStyle.Fill
        };
        nestedParent.Controls.Add(content);
        parent.Controls.Add(nestedParent);

        return content;
    }

    private class SubSplitContainer : SplitContainer
    {
        public new void OnKeyDown(KeyEventArgs e) => base.OnKeyDown(e);

        public new void OnKeyUp(KeyEventArgs e) => base.OnKeyUp(e);

        public new void OnMouseDown(MouseEventArgs e) => base.OnMouseDown(e);

        public new void OnMouseMove(MouseEventArgs e) => base.OnMouseMove(e);

        public new void OnMouseUp(MouseEventArgs e) => base.OnMouseUp(e);
    }

    private class PaintTrackingControl : Control
    {
        public int PaintCallCount { get; private set; }

        public void ResetPaintCallCount() => PaintCallCount = 0;

        protected override void OnPaint(PaintEventArgs e)
        {
            PaintCallCount++;
            base.OnPaint(e);
        }
    }
}
