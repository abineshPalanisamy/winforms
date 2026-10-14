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
    [InlineData(VisualStylesMode.Net11)]
    [InlineData(VisualStylesMode.Latest)]
    public void SplitContainer_MouseMove_CaptureLostAndCanceledDoesNotPerformReversibleDrawing(
        VisualStylesMode visualStylesMode)
    {
        using SubSplitContainer control = new()
        {
            Size = new Size(300, 100),
            VisualStylesMode = visualStylesMode
        };
        using Control captureOwner = new();

        int splitterMovingCallCount = 0;
        int splitterMovedCallCount = 0;
        control.SplitterMoving += (_, _) => splitterMovingCallCount++;
        control.SplitterMoved += (_, _) => splitterMovedCallCount++;
        int initialSplitterDistance = control.SplitterDistance;
        Rectangle initialPanel1Bounds = control.Panel1.Bounds;
        Rectangle initialPanel2Bounds = control.Panel2.Bounds;
        int initialLastDrawSplit = control.TestAccessor.Dynamic._lastDrawSplit;
        Rectangle splitterRectangle = control.SplitterRectangle;
        int splitterX = splitterRectangle.Left + (splitterRectangle.Width / 2);
        int splitterY = splitterRectangle.Top + (splitterRectangle.Height / 2);

        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, splitterX, splitterY, 0));
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, splitterX + 10, splitterY, 0));

        Assert.True(control.Capture);
        Assert.Equal(initialSplitterDistance + 10, control.SplitterDistance);

        control.SplitterMoving += (_, e) =>
        {
            captureOwner.Capture = true;
            e.Cancel = true;
        };
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, splitterX + 20, splitterY, 0));

        Assert.False(control.Capture);
        Assert.True(captureOwner.Capture);
        Assert.Equal(initialSplitterDistance, control.SplitterDistance);
        Assert.Equal(initialPanel1Bounds, control.Panel1.Bounds);
        Assert.Equal(initialPanel2Bounds, control.Panel2.Bounds);
        Assert.Equal(initialLastDrawSplit, (int)control.TestAccessor.Dynamic._lastDrawSplit);
        Assert.Equal(2, splitterMovingCallCount);
        Assert.Equal(0, splitterMovedCallCount);

        control.OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, splitterX + 30, splitterY, 0));
        control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, splitterX + 30, splitterY, 0));

        Assert.Equal(initialSplitterDistance, control.SplitterDistance);
        Assert.Equal(initialPanel1Bounds, control.Panel1.Bounds);
        Assert.Equal(initialPanel2Bounds, control.Panel2.Bounds);
        Assert.Equal(initialLastDrawSplit, (int)control.TestAccessor.Dynamic._lastDrawSplit);
        Assert.Equal(2, splitterMovingCallCount);
        Assert.Equal(0, splitterMovedCallCount);
    }

    [WinFormsTheory]
    [InlineData(VisualStylesMode.Net11)]
    [InlineData(VisualStylesMode.Latest)]
    public void SplitContainer_MouseMove_AfterExternalCaptureLossDoesNotResumeLiveResize(
        VisualStylesMode visualStylesMode)
    {
        using SubSplitContainer control = new()
        {
            Size = new Size(300, 100),
            VisualStylesMode = visualStylesMode
        };
        using Control captureOwner = new();

        int splitterMovingCallCount = 0;
        int splitterMovedCallCount = 0;
        control.SplitterMoving += (_, _) => splitterMovingCallCount++;
        control.SplitterMoved += (_, _) => splitterMovedCallCount++;
        int initialSplitterDistance = control.SplitterDistance;
        Rectangle initialPanel1Bounds = control.Panel1.Bounds;
        Rectangle initialPanel2Bounds = control.Panel2.Bounds;
        Rectangle splitterRectangle = control.SplitterRectangle;
        int splitterX = splitterRectangle.Left + (splitterRectangle.Width / 2);
        int splitterY = splitterRectangle.Top + (splitterRectangle.Height / 2);

        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, splitterX, splitterY, 0));
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, splitterX + 10, splitterY, 0));

        Assert.True(control.Capture);
        Assert.Equal(initialSplitterDistance + 10, control.SplitterDistance);
        Assert.Equal(1, splitterMovingCallCount);
        Assert.Equal(0, splitterMovedCallCount);

        captureOwner.Capture = true;
        Assert.False(control.Capture);
        Assert.True(captureOwner.Capture);

        control.OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, splitterX + 20, splitterY, 0));
        control.OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, splitterX + 30, splitterY, 0));

        Assert.Equal(initialSplitterDistance, control.SplitterDistance);
        Assert.Equal(initialPanel1Bounds, control.Panel1.Bounds);
        Assert.Equal(initialPanel2Bounds, control.Panel2.Bounds);
        Assert.Equal(1, splitterMovingCallCount);
        Assert.Equal(0, splitterMovedCallCount);
        Assert.False(control.Capture);
        Assert.True(captureOwner.Capture);

        control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, splitterX + 30, splitterY, 0));

        Assert.Equal(initialSplitterDistance, control.SplitterDistance);
        Assert.Equal(1, splitterMovingCallCount);
        Assert.Equal(0, splitterMovedCallCount);
    }

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
    [InlineData(Orientation.Horizontal, VisualStylesMode.Net11, RightToLeft.Yes)]
    [InlineData(Orientation.Vertical, VisualStylesMode.Net11, RightToLeft.Yes)]
    [InlineData(Orientation.Horizontal, VisualStylesMode.Latest, RightToLeft.No)]
    [InlineData(Orientation.Horizontal, VisualStylesMode.Latest, RightToLeft.Yes)]
    [InlineData(Orientation.Vertical, VisualStylesMode.Latest, RightToLeft.No)]
    [InlineData(Orientation.Vertical, VisualStylesMode.Latest, RightToLeft.Yes)]
    public void SplitContainer_MouseMove_UpdatesLayoutAndRaisesSplitterMovedOnCompletion(
        Orientation orientation,
        VisualStylesMode visualStylesMode,
        RightToLeft rightToLeft)
    {
        using SubSplitContainer control = new()
        {
            Orientation = orientation,
            RightToLeft = rightToLeft,
            Size = new Size(300, 300),
            VisualStylesMode = visualStylesMode
        };

        int splitterMovedCallCount = 0;
        control.SplitterMoved += (_, _) => splitterMovedCallCount++;

        Rectangle splitterRectangle = control.SplitterRectangle;
        int splitterX = splitterRectangle.Left + (splitterRectangle.Width / 2);
        int splitterY = splitterRectangle.Top + (splitterRectangle.Height / 2);
        int initialSplitterDistance = control.SplitterDistance;
        int direction = rightToLeft == RightToLeft.Yes ? -1 : 1;
        int firstX = orientation == Orientation.Vertical ? splitterX + (direction * 10) : splitterX;
        int firstY = orientation == Orientation.Horizontal ? splitterY + 10 : splitterY;
        int secondX = orientation == Orientation.Vertical ? splitterX + (direction * 20) : splitterX;
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

    [WinFormsTheory]
    [InlineData(Orientation.Horizontal, VisualStylesMode.Net11)]
    [InlineData(Orientation.Horizontal, VisualStylesMode.Latest)]
    [InlineData(Orientation.Vertical, VisualStylesMode.Latest)]
    public void SplitContainer_KeyMove_UpdatesLayoutAndRaisesSplitterMovedOnCompletion(
        Orientation orientation,
        VisualStylesMode visualStylesMode)
    {
        using SubSplitContainer control = new()
        {
            Orientation = orientation,
            Size = new Size(300, 300),
            VisualStylesMode = visualStylesMode
        };

        int splitterMovedCallCount = 0;
        control.SplitterMoved += (_, _) => splitterMovedCallCount++;
        control.TestAccessor.Dynamic._splitterFocused = true;
        int initialSplitterDistance = control.SplitterDistance;

        Keys key = orientation == Orientation.Vertical ? Keys.Right : Keys.Down;
        control.OnKeyDown(new KeyEventArgs(key));

        Assert.Equal(initialSplitterDistance + control.SplitterIncrement, control.SplitterDistance);
        Assert.Equal(0, splitterMovedCallCount);

        control.OnKeyUp(new KeyEventArgs(key));

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
    public void SplitContainer_Net11MouseMove_CaptureLostDuringSplitterMovingCancelsInteraction()
    {
        using SubSplitContainer control = new()
        {
            Size = new Size(300, 100),
            VisualStylesMode = VisualStylesMode.Net11
        };
        using Control captureOwner = new();

        int splitterMovedCallCount = 0;
        int splitterMovingCallCount = 0;
        control.SplitterMoved += (_, _) => splitterMovedCallCount++;
        control.SplitterMoving += (_, _) =>
        {
            splitterMovingCallCount++;
            captureOwner.Capture = true;
        };

        Rectangle splitterRectangle = control.SplitterRectangle;
        int splitterX = splitterRectangle.Left + (splitterRectangle.Width / 2);
        int splitterY = splitterRectangle.Top + (splitterRectangle.Height / 2);
        int initialSplitterDistance = control.SplitterDistance;

        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, splitterX, splitterY, 0));
        Assert.True(control.Capture);
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, splitterX + 10, splitterY, 0));

        Assert.False(control.Capture);
        Assert.True(captureOwner.Capture);
        Assert.Equal(initialSplitterDistance, control.SplitterDistance);
        Assert.Equal(initialSplitterDistance, control.Panel1.Width);

        control.OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, splitterX + 20, splitterY, 0));
        control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, splitterX + 20, splitterY, 0));

        Assert.Equal(initialSplitterDistance, control.SplitterDistance);
        Assert.Equal(1, splitterMovingCallCount);
        Assert.Equal(0, splitterMovedCallCount);
    }

    [WinFormsTheory]
    [InlineData(Orientation.Horizontal, VisualStylesMode.Net11, RightToLeft.No)]
    [InlineData(Orientation.Horizontal, VisualStylesMode.Net11, RightToLeft.Yes)]
    [InlineData(Orientation.Vertical, VisualStylesMode.Net11, RightToLeft.No)]
    [InlineData(Orientation.Vertical, VisualStylesMode.Net11, RightToLeft.Yes)]
    [InlineData(Orientation.Horizontal, VisualStylesMode.Latest, RightToLeft.No)]
    [InlineData(Orientation.Horizontal, VisualStylesMode.Latest, RightToLeft.Yes)]
    [InlineData(Orientation.Vertical, VisualStylesMode.Latest, RightToLeft.No)]
    [InlineData(Orientation.Vertical, VisualStylesMode.Latest, RightToLeft.Yes)]
    public void SplitContainer_MouseMove_CaptureLostRestoresInitialLayoutAndTerminatesInteraction(
        Orientation orientation,
        VisualStylesMode visualStylesMode,
        RightToLeft rightToLeft)
    {
        using SubSplitContainer control = new()
        {
            Orientation = orientation,
            RightToLeft = rightToLeft,
            Size = new Size(300, 300),
            VisualStylesMode = visualStylesMode
        };
        using Control captureOwner = new();

        int splitterMovedCallCount = 0;
        int splitterMovingCallCount = 0;
        control.SplitterMoved += (_, _) => splitterMovedCallCount++;
        control.SplitterMoving += (_, _) => splitterMovingCallCount++;

        Rectangle splitterRectangle = control.SplitterRectangle;
        int splitterX = splitterRectangle.Left + (splitterRectangle.Width / 2);
        int splitterY = splitterRectangle.Top + (splitterRectangle.Height / 2);
        int initialSplitterDistance = control.SplitterDistance;
        Rectangle initialPanel1Bounds = control.Panel1.Bounds;
        Rectangle initialPanel2Bounds = control.Panel2.Bounds;
        int direction = rightToLeft == RightToLeft.Yes ? -1 : 1;
        int movedX = orientation == Orientation.Vertical ? splitterX + (direction * 10) : splitterX;
        int movedY = orientation == Orientation.Horizontal ? splitterY + 10 : splitterY;

        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, splitterX, splitterY, 0));
        Assert.True(control.Capture);
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, movedX, movedY, 0));
        Assert.Equal(initialSplitterDistance + 10, control.SplitterDistance);

        captureOwner.Capture = true;

        Assert.False(control.Capture);
        Assert.True(captureOwner.Capture);
        Assert.Equal(initialSplitterDistance, control.SplitterDistance);
        Assert.Equal(initialPanel1Bounds, control.Panel1.Bounds);
        Assert.Equal(initialPanel2Bounds, control.Panel2.Bounds);
        Assert.Equal(0, splitterMovedCallCount);

        control.OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, movedX, movedY, 0));
        control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, movedX, movedY, 0));

        Assert.Equal(initialSplitterDistance, control.SplitterDistance);
        Assert.Equal(initialPanel1Bounds, control.Panel1.Bounds);
        Assert.Equal(initialPanel2Bounds, control.Panel2.Bounds);
        Assert.Equal(1, splitterMovingCallCount);
        Assert.Equal(0, splitterMovedCallCount);

        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, splitterX, splitterY, 0));
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, movedX, movedY, 0));
        control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, movedX, movedY, 0));

        Assert.False(control.Capture);
        Assert.Equal(initialSplitterDistance + 10, control.SplitterDistance);
        Assert.Equal(2, splitterMovingCallCount);
        Assert.Equal(1, splitterMovedCallCount);
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

    [WinFormsFact]
    public void SplitContainer_Net11MouseMove_ReentrantCaptureChangedDoesNotDuplicateCompletionNotification()
    {
        using SubSplitContainer control = new()
        {
            Size = new Size(300, 100),
            VisualStylesMode = VisualStylesMode.Net11
        };

        int splitterMovedCallCount = 0;
        control.SplitterMoved += (_, _) => splitterMovedCallCount++;
        control.MouseCaptureChanged += (_, _) =>
        {
            if (!control.Capture)
            {
                control.SplitterDistance = 70;
            }
        };

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

    [WinFormsTheory]
    [InlineData(Orientation.Horizontal, VisualStylesMode.Net11, RightToLeft.No)]
    [InlineData(Orientation.Horizontal, VisualStylesMode.Net11, RightToLeft.Yes)]
    [InlineData(Orientation.Vertical, VisualStylesMode.Net11, RightToLeft.No)]
    [InlineData(Orientation.Vertical, VisualStylesMode.Net11, RightToLeft.Yes)]
    [InlineData(Orientation.Horizontal, VisualStylesMode.Latest, RightToLeft.No)]
    [InlineData(Orientation.Horizontal, VisualStylesMode.Latest, RightToLeft.Yes)]
    [InlineData(Orientation.Vertical, VisualStylesMode.Latest, RightToLeft.No)]
    [InlineData(Orientation.Vertical, VisualStylesMode.Latest, RightToLeft.Yes)]
    public void SplitContainer_MouseMove_RepaintsDeeplyNestedContentWithoutRecreatingHandles(
        Orientation orientation,
        VisualStylesMode visualStylesMode,
        RightToLeft rightToLeft)
    {
        using Form form = new();
        using SubSplitContainer control = new()
        {
            Dock = DockStyle.Fill,
            Orientation = orientation,
            RightToLeft = rightToLeft,
            Size = new Size(300, 300),
            VisualStylesMode = visualStylesMode
        };
        using PaintTrackingControl panel1Content = CreateDeeplyNestedPaintTrackingControl(control.Panel1);
        using PaintTrackingControl panel2Content = CreateDeeplyNestedPaintTrackingControl(control.Panel2);
        form.Controls.Add(control);
        form.Show();
        Application.DoEvents();
        Dictionary<Control, nint> handles = [];
        int handleCreatedCallCount = 0;
        int handleDestroyedCallCount = 0;
        TrackHandles(form);
        int splitterMovedCallCount = 0;
        control.SplitterMoved += (_, _) => splitterMovedCallCount++;
        int direction = rightToLeft == RightToLeft.Yes ? -1 : 1;
        int[] offsets = [10, 20, -10, 0, 10];

        for (int interaction = 0; interaction < 3; interaction++)
        {
            Rectangle splitterRectangle = control.SplitterRectangle;
            int splitterX = splitterRectangle.Left + (splitterRectangle.Width / 2);
            int splitterY = splitterRectangle.Top + (splitterRectangle.Height / 2);
            int initialSplitterDistance = control.SplitterDistance;
            control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, splitterX, splitterY, 0));

            foreach (int offset in offsets)
            {
                panel1Content.ResetPaintCallCount();
                panel2Content.ResetPaintCallCount();
                Size previousPanel1ContentSize = panel1Content.Size;
                Size previousPanel2ContentSize = panel2Content.Size;
                int movedX = orientation == Orientation.Vertical ? splitterX + (direction * offset) : splitterX;
                int movedY = orientation == Orientation.Horizontal ? splitterY + offset : splitterY;

                control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, movedX, movedY, 0));

                Assert.Equal(initialSplitterDistance + offset, control.SplitterDistance);
                Assert.Equal(
                    initialSplitterDistance + offset,
                    orientation == Orientation.Vertical ? control.Panel1.Width : control.Panel1.Height);
                Assert.NotEqual(previousPanel1ContentSize, panel1Content.Size);
                Assert.NotEqual(previousPanel2ContentSize, panel2Content.Size);
                Assert.True(panel1Content.PaintCallCount > 0);
                Assert.True(panel2Content.PaintCallCount > 0);
                Assert.Equal(interaction, splitterMovedCallCount);
                foreach ((Control child, nint handle) in handles)
                {
                    Assert.True(child.IsHandleCreated);
                    Assert.Equal(handle, child.Handle);
                }
            }

            int finalX = orientation == Orientation.Vertical ? splitterX + (direction * offsets[^1]) : splitterX;
            int finalY = orientation == Orientation.Horizontal ? splitterY + offsets[^1] : splitterY;
            control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, finalX, finalY, 0));
            Assert.False(control.Capture);
            Assert.Equal(initialSplitterDistance + offsets[^1], control.SplitterDistance);
            Assert.Equal(interaction + 1, splitterMovedCallCount);
        }

        Assert.Equal(0, handleCreatedCallCount);
        Assert.Equal(0, handleDestroyedCallCount);

        void TrackHandles(Control parent)
        {
            Assert.True(parent.IsHandleCreated);
            handles.Add(parent, parent.Handle);
            parent.HandleCreated += (_, _) => handleCreatedCallCount++;
            parent.HandleDestroyed += (_, _) => handleDestroyedCallCount++;
            foreach (Control child in parent.Controls)
            {
                TrackHandles(child);
            }
        }
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

    private static PaintTrackingControl CreateDeeplyNestedPaintTrackingControl(Control parent)
    {
        for (int depth = 0; depth < 3; depth++)
        {
            TableLayoutPanel nestedParent = new()
            {
                ColumnCount = 1,
                RowCount = 1,
                Dock = DockStyle.Fill
            };
            nestedParent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            nestedParent.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            parent.Controls.Add(nestedParent);
            parent = nestedParent;
        }

        PaintTrackingControl content = new()
        {
            Dock = DockStyle.Fill
        };
        parent.Controls.Add(content);

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
