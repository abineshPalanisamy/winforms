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

    [WinFormsTheory]
    [InlineData(Orientation.Horizontal, RightToLeft.No)]
    [InlineData(Orientation.Vertical, RightToLeft.No)]
    [InlineData(Orientation.Vertical, RightToLeft.Yes)]
    public void SplitContainer_MouseMove_IncrementSnappingAndBoundariesPreserveLayout(
        Orientation orientation,
        RightToLeft rightToLeft)
    {
        using SubSplitContainer control = new()
        {
            Orientation = orientation,
            RightToLeft = rightToLeft,
            Size = new Size(300, 300),
            VisualStylesMode = VisualStylesMode.Net11,
            Panel1MinSize = 50,
            Panel2MinSize = 50,
            SplitterWidth = 10,
            SplitterIncrement = 10,
            SplitterDistance = 100
        };
        Point anchor = GetSplitterCenter(control);
        int movedCallCount = 0;
        control.SplitterMoved += (_, _) => movedCallCount++;
        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, anchor.X, anchor.Y, 0));

        foreach ((int offset, int expectedDistance) in new (int, int)[]
        {
            (3, 100), (17, 110), (17, 110), (-17, 90),
            (-1000, 50), (140, 240), (1000, 240), (20, 120)
        })
        {
            Point position = GetDragPosition(control, anchor, offset);
            control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, position.X, position.Y, 0));

            Assert.Equal(expectedDistance, control.SplitterDistance);
            Assert.Equal(
                expectedDistance,
                orientation == Orientation.Vertical ? control.Panel1.Width : control.Panel1.Height);
            Assert.Equal(
                300 - expectedDistance - control.SplitterWidth,
                orientation == Orientation.Vertical ? control.Panel2.Width : control.Panel2.Height);
            Assert.Equal(0, movedCallCount);
        }

        Point finalPosition = GetDragPosition(control, anchor, 20);
        control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, finalPosition.X, finalPosition.Y, 0));

        Assert.False(control.Capture);
        Assert.Equal(120, control.SplitterDistance);
        Assert.Equal(1, movedCallCount);
    }

    [WinFormsTheory]
    [InlineData(Orientation.Horizontal, false)]
    [InlineData(Orientation.Horizontal, true)]
    [InlineData(Orientation.Vertical, false)]
    [InlineData(Orientation.Vertical, true)]
    public void SplitContainer_MouseMove_NoNetDistanceChangeDoesNotRaiseSplitterMoved(
        Orientation orientation,
        bool moveAwayAndReturn)
    {
        using SubSplitContainer control = new()
        {
            Orientation = orientation,
            Size = new Size(300, 300),
            VisualStylesMode = VisualStylesMode.Latest
        };
        int initialDistance = control.SplitterDistance;
        Rectangle initialPanel1Bounds = control.Panel1.Bounds;
        Rectangle initialPanel2Bounds = control.Panel2.Bounds;
        int movedCallCount = 0;
        control.SplitterMoved += (_, _) => movedCallCount++;
        Point anchor = GetSplitterCenter(control);
        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, anchor.X, anchor.Y, 0));

        if (moveAwayAndReturn)
        {
            Point position = GetDragPosition(control, anchor, 10);
            control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, position.X, position.Y, 0));
            Assert.Equal(initialDistance + 10, control.SplitterDistance);
            control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, anchor.X, anchor.Y, 0));
        }

        control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, anchor.X, anchor.Y, 0));

        Assert.Equal(initialDistance, control.SplitterDistance);
        Assert.Equal(initialPanel1Bounds, control.Panel1.Bounds);
        Assert.Equal(initialPanel2Bounds, control.Panel2.Bounds);
        Assert.Equal(0, movedCallCount);
        Assert.False(control.Capture);
    }

    [WinFormsTheory]
    [InlineData(Orientation.Horizontal, 0x0100, Keys.Escape)]
    [InlineData(Orientation.Vertical, 0x0100, Keys.Escape)]
    [InlineData(Orientation.Horizontal, 0x0104, Keys.Menu)]
    [InlineData(Orientation.Vertical, 0x0104, Keys.Menu)]
    public void SplitContainer_MouseMove_CancelingMessageRestoresLayoutAndAllowsNewDrag(
        Orientation orientation,
        int messageId,
        Keys key)
    {
        using SubSplitContainer control = new()
        {
            Orientation = orientation,
            Size = new Size(300, 300),
            VisualStylesMode = VisualStylesMode.Net11
        };
        int initialDistance = control.SplitterDistance;
        Rectangle initialPanel1Bounds = control.Panel1.Bounds;
        Rectangle initialPanel2Bounds = control.Panel2.Bounds;
        int movedCallCount = 0;
        control.SplitterMoved += (_, _) => movedCallCount++;
        Point anchor = GetSplitterCenter(control);
        Point position = GetDragPosition(control, anchor, 20);
        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, anchor.X, anchor.Y, 0));
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, position.X, position.Y, 0));
        Assert.Equal(initialDistance + 20, control.SplitterDistance);
        IMessageFilter filter = (IMessageFilter)control.TestAccessor.Dynamic._splitContainerMessageFilter;
        Message message = Message.Create(control.Handle, messageId, (nint)key, 0);

        Assert.True(filter.PreFilterMessage(ref message));
        Assert.Equal(initialDistance, control.SplitterDistance);
        Assert.Equal(initialPanel1Bounds, control.Panel1.Bounds);
        Assert.Equal(initialPanel2Bounds, control.Panel2.Bounds);
        Assert.False(control.Capture);
        Assert.Equal(0, movedCallCount);

        control.OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, position.X, position.Y, 0));
        control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, position.X, position.Y, 0));
        Assert.Equal(initialDistance, control.SplitterDistance);
        Assert.Equal(0, movedCallCount);

        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, anchor.X, anchor.Y, 0));
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, position.X, position.Y, 0));
        control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, position.X, position.Y, 0));
        Assert.Equal(initialDistance + 20, control.SplitterDistance);
        Assert.Equal(1, movedCallCount);
        Assert.False(control.Capture);
    }

    [WinFormsTheory]
    [InlineData(Orientation.Horizontal, VisualStylesMode.Net11)]
    [InlineData(Orientation.Vertical, VisualStylesMode.Latest)]
    public void SplitContainer_KeyMove_EscapeAfterRepeatedMovementRestoresLayoutAndAllowsNewInteraction(
        Orientation orientation,
        VisualStylesMode visualStylesMode)
    {
        using SubSplitContainer control = new()
        {
            Orientation = orientation,
            Size = new Size(300, 300),
            VisualStylesMode = visualStylesMode,
            SplitterIncrement = 10
        };
        control.TestAccessor.Dynamic._splitterFocused = true;
        int initialDistance = control.SplitterDistance;
        Rectangle initialPanel1Bounds = control.Panel1.Bounds;
        Rectangle initialPanel2Bounds = control.Panel2.Bounds;
        int movingCallCount = 0;
        int movedCallCount = 0;
        control.SplitterMoving += (_, _) => movingCallCount++;
        control.SplitterMoved += (_, _) => movedCallCount++;
        Keys key = orientation == Orientation.Vertical ? Keys.Right : Keys.Down;

        control.OnKeyDown(new KeyEventArgs(key));
        control.OnKeyDown(new KeyEventArgs(key));
        Assert.Equal(initialDistance + 20, control.SplitterDistance);
        Assert.False(control.Capture);
        Assert.Equal(0, movedCallCount);
        control.OnKeyDown(new KeyEventArgs(Keys.Escape));
        control.OnKeyUp(new KeyEventArgs(Keys.Escape));
        control.OnKeyUp(new KeyEventArgs(key));

        Assert.Equal(initialDistance, control.SplitterDistance);
        Assert.Equal(initialPanel1Bounds, control.Panel1.Bounds);
        Assert.Equal(initialPanel2Bounds, control.Panel2.Bounds);
        Assert.Equal(2, movingCallCount);
        Assert.Equal(0, movedCallCount);

        control.OnKeyDown(new KeyEventArgs(key));
        control.OnKeyUp(new KeyEventArgs(key));

        Assert.Equal(initialDistance + 10, control.SplitterDistance);
        Assert.Equal(3, movingCallCount);
        Assert.Equal(1, movedCallCount);
    }

    [WinFormsTheory]
    [InlineData(Orientation.Horizontal)]
    [InlineData(Orientation.Vertical)]
    public void SplitContainer_KeyMove_BoundariesAndReversalPreserveCompletionNotifications(Orientation orientation)
    {
        using SubSplitContainer control = new()
        {
            Orientation = orientation,
            Size = new Size(300, 300),
            VisualStylesMode = VisualStylesMode.Net11,
            Panel1MinSize = 50,
            Panel2MinSize = 50,
            SplitterWidth = 10,
            SplitterIncrement = 10,
            SplitterDistance = 50
        };
        control.TestAccessor.Dynamic._splitterFocused = true;
        int movedCallCount = 0;
        control.SplitterMoved += (_, _) => movedCallCount++;
        Keys decrease = orientation == Orientation.Vertical ? Keys.Left : Keys.Up;
        Keys increase = orientation == Orientation.Vertical ? Keys.Right : Keys.Down;

        control.OnKeyDown(new KeyEventArgs(decrease));
        control.OnKeyUp(new KeyEventArgs(decrease));
        Assert.Equal(50, control.SplitterDistance);
        Assert.Equal(0, movedCallCount);

        for (int index = 0; index < 20; index++)
        {
            control.OnKeyDown(new KeyEventArgs(increase));
        }

        Assert.Equal(240, control.SplitterDistance);
        Assert.Equal(50, orientation == Orientation.Vertical ? control.Panel2.Width : control.Panel2.Height);
        Assert.Equal(0, movedCallCount);
        control.OnKeyUp(new KeyEventArgs(increase));
        Assert.Equal(1, movedCallCount);

        control.OnKeyDown(new KeyEventArgs(increase));
        control.OnKeyUp(new KeyEventArgs(increase));
        Assert.Equal(240, control.SplitterDistance);
        Assert.Equal(1, movedCallCount);

        control.OnKeyDown(new KeyEventArgs(decrease));
        Assert.Equal(230, control.SplitterDistance);
        control.OnKeyDown(new KeyEventArgs(increase));
        control.OnKeyUp(new KeyEventArgs(increase));
        Assert.Equal(240, control.SplitterDistance);
        Assert.Equal(1, movedCallCount);
    }

    [WinFormsTheory]
    [InlineData(VisualStylesMode.Net11, VisualStylesMode.Classic, true)]
    [InlineData(VisualStylesMode.Classic, VisualStylesMode.Net11, false)]
    public void SplitContainer_KeyMove_ModeChangeAppliesToNextInteraction(
        VisualStylesMode initialMode,
        VisualStylesMode changedMode,
        bool initiallyLive)
    {
        using SubSplitContainer control = new()
        {
            Size = new Size(300, 100),
            VisualStylesMode = initialMode
        };
        control.TestAccessor.Dynamic._splitterFocused = true;
        int initialDistance = control.SplitterDistance;
        int movedCallCount = 0;
        control.SplitterMoved += (_, _) => movedCallCount++;

        control.OnKeyDown(new KeyEventArgs(Keys.Right));
        control.VisualStylesMode = changedMode;
        control.OnKeyDown(new KeyEventArgs(Keys.Right));
        Assert.Equal(initiallyLive ? initialDistance + 2 : initialDistance, control.SplitterDistance);
        control.OnKeyUp(new KeyEventArgs(Keys.Right));
        Assert.Equal(initialDistance + 2, control.SplitterDistance);
        Assert.Equal(1, movedCallCount);

        control.OnKeyDown(new KeyEventArgs(Keys.Right));
        Assert.Equal(initiallyLive ? initialDistance + 2 : initialDistance + 3, control.SplitterDistance);
        control.OnKeyUp(new KeyEventArgs(Keys.Right));
        Assert.Equal(initialDistance + 3, control.SplitterDistance);
        Assert.Equal(2, movedCallCount);
    }

    [WinFormsTheory]
    [InlineData(VisualStylesMode.Net11, true)]
    [InlineData(VisualStylesMode.Latest, true)]
    [InlineData(VisualStylesMode.Classic, false)]
    [InlineData(VisualStylesMode.Disabled, false)]
    public void SplitContainer_MouseMove_InheritedModeControlsLiveLayout(
        VisualStylesMode parentMode,
        bool expectedLive)
    {
        using Panel parent = new() { VisualStylesMode = parentMode };
        using SubSplitContainer control = new() { Size = new Size(300, 100) };
        parent.Controls.Add(control);
        Assert.Equal(VisualStylesMode.Inherit, control.VisualStylesMode);
        int initialDistance = control.SplitterDistance;
        Point anchor = GetSplitterCenter(control);

        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, anchor.X, anchor.Y, 0));
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, anchor.X + 10, anchor.Y, 0));
        Assert.Equal(expectedLive ? initialDistance + 10 : initialDistance, control.SplitterDistance);
        control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, anchor.X + 10, anchor.Y, 0));
        Assert.Equal(initialDistance + 10, control.SplitterDistance);
        Assert.False(control.Capture);
    }

    [WinFormsTheory]
    [InlineData(Orientation.Horizontal)]
    [InlineData(Orientation.Vertical)]
    public void SplitContainer_MouseAndKeyMove_FixedSplitterBlocksAndThenAllowsInteraction(Orientation orientation)
    {
        using SubSplitContainer control = new()
        {
            Orientation = orientation,
            Size = new Size(300, 300),
            VisualStylesMode = VisualStylesMode.Net11,
            IsSplitterFixed = true
        };
        control.TestAccessor.Dynamic._splitterFocused = true;
        int initialDistance = control.SplitterDistance;
        int movingCallCount = 0;
        int movedCallCount = 0;
        control.SplitterMoving += (_, _) => movingCallCount++;
        control.SplitterMoved += (_, _) => movedCallCount++;
        Point anchor = GetSplitterCenter(control);
        Point position = GetDragPosition(control, anchor, 10);
        Keys key = orientation == Orientation.Vertical ? Keys.Right : Keys.Down;

        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, anchor.X, anchor.Y, 0));
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, position.X, position.Y, 0));
        control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, position.X, position.Y, 0));
        control.OnKeyDown(new KeyEventArgs(key));
        control.OnKeyUp(new KeyEventArgs(key));
        Assert.Equal(initialDistance, control.SplitterDistance);
        Assert.Equal(0, movingCallCount);
        Assert.Equal(0, movedCallCount);
        Assert.False(control.Capture);

        control.IsSplitterFixed = false;
        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, anchor.X, anchor.Y, 0));
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, position.X, position.Y, 0));
        control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, position.X, position.Y, 0));
        control.OnKeyDown(new KeyEventArgs(key));
        control.OnKeyUp(new KeyEventArgs(key));
        Assert.Equal(initialDistance + 10 + control.SplitterIncrement, control.SplitterDistance);
        Assert.Equal(2, movingCallCount);
        Assert.Equal(2, movedCallCount);
    }

    [WinFormsTheory]
    [InlineData(MouseButtons.Right, 1, true)]
    [InlineData(MouseButtons.Left, 2, true)]
    [InlineData(MouseButtons.Left, 1, false)]
    public void SplitContainer_MouseDown_InvalidActivationDoesNotStartLiveResize(
        MouseButtons button,
        int clicks,
        bool enabled)
    {
        using SubSplitContainer control = new()
        {
            Size = new Size(300, 100),
            VisualStylesMode = VisualStylesMode.Net11,
            Enabled = enabled
        };
        int initialDistance = control.SplitterDistance;
        int movingCallCount = 0;
        int movedCallCount = 0;
        control.SplitterMoving += (_, _) => movingCallCount++;
        control.SplitterMoved += (_, _) => movedCallCount++;
        Point anchor = GetSplitterCenter(control);

        control.OnMouseDown(new MouseEventArgs(button, clicks, anchor.X, anchor.Y, 0));
        control.OnMouseMove(new MouseEventArgs(button, 0, anchor.X + 10, anchor.Y, 0));
        control.OnMouseUp(new MouseEventArgs(button, 1, anchor.X + 10, anchor.Y, 0));

        Assert.Equal(initialDistance, control.SplitterDistance);
        Assert.Equal(0, movingCallCount);
        Assert.Equal(0, movedCallCount);
        Assert.False(control.Capture);
        Assert.False(control.IsHandleCreated);
    }

    [WinFormsTheory]
    [InlineData(Orientation.Horizontal)]
    [InlineData(Orientation.Vertical)]
    public void SplitContainer_MouseMove_EventsDescribePendingAndFinalLayout(Orientation orientation)
    {
        using SubSplitContainer control = new()
        {
            Orientation = orientation,
            Size = new Size(300, 300),
            VisualStylesMode = VisualStylesMode.Net11
        };
        int initialDistance = control.SplitterDistance;
        Point anchor = GetSplitterCenter(control);
        Point position = GetDragPosition(control, anchor, 10);
        List<string> eventOrder = [];
        control.SplitterMoving += (_, e) =>
        {
            eventOrder.Add("Moving");
            Assert.Equal(initialDistance, control.SplitterDistance);
            Assert.Equal(position.X, e.MouseCursorX);
            Assert.Equal(position.Y, e.MouseCursorY);
            Assert.Equal(
                initialDistance + 10,
                orientation == Orientation.Vertical ? e.SplitX : e.SplitY);
        };
        control.SplitterMoved += (_, e) =>
        {
            eventOrder.Add("Moved");
            Rectangle rectangle = control.SplitterRectangle;
            Assert.Equal(initialDistance + 10, control.SplitterDistance);
            Assert.Equal(rectangle.X, e.SplitX);
            Assert.Equal(rectangle.Y, e.SplitY);
            Assert.Equal(rectangle.X + rectangle.Width / 2, e.X);
            Assert.Equal(rectangle.Y + rectangle.Height / 2, e.Y);
            Assert.False(control.Capture);
        };

        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, anchor.X, anchor.Y, 0));
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, position.X, position.Y, 0));
        Assert.Equal(["Moving"], eventOrder);
        control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, position.X, position.Y, 0));
        Assert.Equal(["Moving", "Moved"], eventOrder);
    }

    [WinFormsTheory]
    [InlineData(Orientation.Horizontal, FixedPanel.Panel1)]
    [InlineData(Orientation.Vertical, FixedPanel.Panel1)]
    [InlineData(Orientation.Horizontal, FixedPanel.Panel2)]
    [InlineData(Orientation.Vertical, FixedPanel.Panel2)]
    public void SplitContainer_MouseMove_FixedPanelRetainsCommittedSizeOnContainerResize(
        Orientation orientation,
        FixedPanel fixedPanel)
    {
        using SubSplitContainer control = new()
        {
            Orientation = orientation,
            Size = new Size(300, 300),
            VisualStylesMode = VisualStylesMode.Net11,
            FixedPanel = fixedPanel
        };
        Point anchor = GetSplitterCenter(control);
        Point position = GetDragPosition(control, anchor, 10);
        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, anchor.X, anchor.Y, 0));
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, position.X, position.Y, 0));
        control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, position.X, position.Y, 0));
        SplitterPanel panel = fixedPanel == FixedPanel.Panel1 ? control.Panel1 : control.Panel2;
        int committedSize = orientation == Orientation.Vertical ? panel.Width : panel.Height;

        control.Size = new Size(400, 400);

        Assert.Equal(committedSize, orientation == Orientation.Vertical ? panel.Width : panel.Height);
        Assert.Equal(
            400 - control.SplitterWidth,
            orientation == Orientation.Vertical
                ? control.Panel1.Width + control.Panel2.Width
                : control.Panel1.Height + control.Panel2.Height);
    }

    [WinFormsTheory]
    [InlineData(Orientation.Horizontal, true)]
    [InlineData(Orientation.Horizontal, false)]
    [InlineData(Orientation.Vertical, true)]
    [InlineData(Orientation.Vertical, false)]
    public void SplitContainer_MouseMove_AfterExpandingCollapsedPanelUpdatesRestoredLayout(
        Orientation orientation,
        bool collapsePanel1)
    {
        using SubSplitContainer control = new()
        {
            Orientation = orientation,
            Size = new Size(300, 300),
            VisualStylesMode = VisualStylesMode.Net11
        };
        int movingCallCount = 0;
        int movedCallCount = 0;
        control.SplitterMoving += (_, _) => movingCallCount++;
        control.SplitterMoved += (_, _) => movedCallCount++;
        control.Panel1Collapsed = collapsePanel1;
        control.Panel2Collapsed = !collapsePanel1;

        control.Size = new Size(400, 400);
        Assert.Equal(control.Size, collapsePanel1 ? control.Panel2.Size : control.Panel1.Size);
        Assert.False(control.Capture);
        Assert.Equal(0, movingCallCount);
        Assert.Equal(0, movedCallCount);

        control.Panel1Collapsed = false;
        control.Panel2Collapsed = false;
        movingCallCount = 0;
        movedCallCount = 0;
        Point anchor = GetSplitterCenter(control);
        Point position = GetDragPosition(control, anchor, 10);
        int initialDistance = control.SplitterDistance;
        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, anchor.X, anchor.Y, 0));
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, position.X, position.Y, 0));
        Assert.Equal(initialDistance + 10, control.SplitterDistance);
        control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, position.X, position.Y, 0));
        Assert.Equal(1, movingCallCount);
        Assert.Equal(1, movedCallCount);
    }

    [WinFormsTheory]
    [InlineData(Orientation.Horizontal)]
    [InlineData(Orientation.Vertical)]
    public void SplitContainer_MouseAndKeyMove_InsufficientSpaceDoesNotActivate(Orientation orientation)
    {
        using SubSplitContainer control = new()
        {
            Orientation = orientation,
            Size = new Size(100, 100),
            VisualStylesMode = VisualStylesMode.Net11,
            Panel1MinSize = 50,
            Panel2MinSize = 50
        };
        control.TestAccessor.Dynamic._splitterFocused = true;
        int initialDistance = control.SplitterDistance;
        Rectangle initialPanel1Bounds = control.Panel1.Bounds;
        Rectangle initialPanel2Bounds = control.Panel2.Bounds;
        int movingCallCount = 0;
        int movedCallCount = 0;
        control.SplitterMoving += (_, _) => movingCallCount++;
        control.SplitterMoved += (_, _) => movedCallCount++;
        Point anchor = GetSplitterCenter(control);
        Point position = GetDragPosition(control, anchor, 10);
        Keys key = orientation == Orientation.Vertical ? Keys.Right : Keys.Down;

        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, anchor.X, anchor.Y, 0));
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, position.X, position.Y, 0));
        control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, position.X, position.Y, 0));
        control.OnKeyDown(new KeyEventArgs(key));
        control.OnKeyUp(new KeyEventArgs(key));

        Assert.Equal(initialDistance, control.SplitterDistance);
        Assert.Equal(initialPanel1Bounds, control.Panel1.Bounds);
        Assert.Equal(initialPanel2Bounds, control.Panel2.Bounds);
        Assert.Equal(0, movingCallCount);
        Assert.Equal(0, movedCallCount);
        Assert.False(control.Capture);
    }

    [WinFormsTheory]
    [InlineData(Orientation.Horizontal)]
    [InlineData(Orientation.Vertical)]
    public void SplitContainer_KeyMove_CancelAfterAppliedMovementRollsBackAndCanRestart(Orientation orientation)
    {
        using SubSplitContainer control = new()
        {
            Orientation = orientation,
            Size = new Size(300, 300),
            VisualStylesMode = VisualStylesMode.Net11,
            SplitterIncrement = 10
        };
        control.TestAccessor.Dynamic._splitterFocused = true;
        int initialDistance = control.SplitterDistance;
        Rectangle initialPanel1Bounds = control.Panel1.Bounds;
        Rectangle initialPanel2Bounds = control.Panel2.Bounds;
        int movingCallCount = 0;
        int movedCallCount = 0;
        control.SplitterMoving += (_, e) => e.Cancel = ++movingCallCount == 2;
        control.SplitterMoved += (_, _) => movedCallCount++;
        Keys key = orientation == Orientation.Vertical ? Keys.Right : Keys.Down;

        control.OnKeyDown(new KeyEventArgs(key));
        Assert.Equal(initialDistance + 10, control.SplitterDistance);
        control.OnKeyDown(new KeyEventArgs(key));
        control.OnKeyUp(new KeyEventArgs(key));

        Assert.Equal(initialDistance, control.SplitterDistance);
        Assert.Equal(initialPanel1Bounds, control.Panel1.Bounds);
        Assert.Equal(initialPanel2Bounds, control.Panel2.Bounds);
        Assert.Equal(0, movedCallCount);
        Assert.False(control.TestAccessor.Dynamic._splitBegin);
        Assert.False(control.TestAccessor.Dynamic._splitMove);

        control.OnKeyDown(new KeyEventArgs(key));
        control.OnKeyUp(new KeyEventArgs(key));

        Assert.Equal(initialDistance + 10, control.SplitterDistance);
        Assert.Equal(3, movingCallCount);
        Assert.Equal(1, movedCallCount);
    }

    [WinFormsTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void SplitContainer_KeyMove_ReentrantDistancePreservesNotificationContract(bool cancel)
    {
        using SubSplitContainer control = new()
        {
            Size = new Size(300, 100),
            VisualStylesMode = VisualStylesMode.Net11
        };
        control.TestAccessor.Dynamic._splitterFocused = true;
        int initialDistance = control.SplitterDistance;
        List<int> notifiedDistances = [];
        control.SplitterMoved += (_, _) => notifiedDistances.Add(control.SplitterDistance);
        control.SplitterMoving += (_, e) =>
        {
            control.SplitterDistance = 70;
            e.Cancel = cancel;
        };

        control.OnKeyDown(new KeyEventArgs(Keys.Right));
        control.OnKeyUp(new KeyEventArgs(Keys.Right));

        Assert.Equal(cancel ? initialDistance : 70, control.SplitterDistance);
        Assert.Equal(cancel ? new[] { 70, initialDistance } : new[] { 70 }, notifiedDistances);
        Assert.Equal(control.SplitterDistance, control.Panel1.Width);
        Assert.False(control.TestAccessor.Dynamic._splitBegin);
        Assert.False(control.TestAccessor.Dynamic._splitMove);
    }

    [WinFormsFact]
    public void SplitContainer_MouseMove_ReentrantNotificationFollowedByLiveMovementNotifiesFinalDistance()
    {
        using SubSplitContainer control = new()
        {
            Size = new Size(300, 100),
            VisualStylesMode = VisualStylesMode.Net11
        };
        Point anchor = GetSplitterCenter(control);
        int initialDistance = control.SplitterDistance;
        int movingCallCount = 0;
        List<int> notifiedDistances = [];
        control.SplitterMoved += (_, _) => notifiedDistances.Add(control.SplitterDistance);
        control.SplitterMoving += (_, _) =>
        {
            if (++movingCallCount == 1)
            {
                control.SplitterDistance = 70;
            }
        };

        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, anchor.X, anchor.Y, 0));
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, anchor.X + 10, anchor.Y, 0));
        Assert.Equal([70], notifiedDistances);
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, anchor.X + 20, anchor.Y, 0));
        Assert.Equal(initialDistance + 20, control.SplitterDistance);
        Assert.Equal([70], notifiedDistances);
        control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, anchor.X + 20, anchor.Y, 0));

        Assert.Equal([70, initialDistance + 20], notifiedDistances);
        Assert.Equal(initialDistance + 20, control.Panel1.Width);
        Assert.False(control.Capture);
    }

    [WinFormsTheory]
    [InlineData(0x0100, Keys.A, true)]
    [InlineData(0x0101, Keys.Escape, true)]
    [InlineData(0x0200, Keys.None, false)]
    public void SplitContainer_MouseMove_NonCancelingMessagesPreserveInteraction(
        int messageId,
        Keys key,
        bool expectedFiltered)
    {
        using SubSplitContainer control = new()
        {
            Size = new Size(300, 100),
            VisualStylesMode = VisualStylesMode.Net11
        };
        int initialDistance = control.SplitterDistance;
        Point anchor = GetSplitterCenter(control);
        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, anchor.X, anchor.Y, 0));
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, anchor.X + 10, anchor.Y, 0));
        IMessageFilter filter = (IMessageFilter)control.TestAccessor.Dynamic._splitContainerMessageFilter;
        Message message = Message.Create(control.Handle, messageId, (nint)key, 0);

        Assert.Equal(expectedFiltered, filter.PreFilterMessage(ref message));
        Assert.True(control.Capture);
        Assert.Equal(initialDistance + 10, control.SplitterDistance);

        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, anchor.X + 20, anchor.Y, 0));
        control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, anchor.X + 20, anchor.Y, 0));
        Assert.Equal(initialDistance + 20, control.SplitterDistance);
        Assert.False(control.Capture);
    }

    [WinFormsTheory]
    [InlineData(Orientation.Horizontal)]
    [InlineData(Orientation.Vertical)]
    public void SplitContainer_SplitterDistance_ProgrammaticChangeRetainsImmediateLayoutAndNotification(
        Orientation orientation)
    {
        using SubSplitContainer control = new()
        {
            Orientation = orientation,
            Size = new Size(300, 300),
            VisualStylesMode = VisualStylesMode.Net11
        };
        int movingCallCount = 0;
        List<int> notifiedDistances = [];
        control.SplitterMoving += (_, _) => movingCallCount++;
        control.SplitterMoved += (_, _) => notifiedDistances.Add(control.SplitterDistance);

        control.SplitterDistance = 70;
        control.SplitterDistance = 70;
        control.SplitterDistance = 90;

        Assert.Equal([70, 90], notifiedDistances);
        Assert.Equal(0, movingCallCount);
        Assert.Equal(90, orientation == Orientation.Vertical ? control.Panel1.Width : control.Panel1.Height);
        Assert.Equal(300 - 90 - control.SplitterWidth,
            orientation == Orientation.Vertical ? control.Panel2.Width : control.Panel2.Height);
        Assert.False(control.Capture);
        Assert.False(control.IsHandleCreated);
    }

    [WinFormsTheory]
    [InlineData(Orientation.Horizontal, BorderStyle.FixedSingle, false)]
    [InlineData(Orientation.Vertical, BorderStyle.FixedSingle, true)]
    [InlineData(Orientation.Horizontal, BorderStyle.Fixed3D, true)]
    [InlineData(Orientation.Vertical, BorderStyle.Fixed3D, false)]
    public void SplitContainer_MouseMove_CancellationRestoresAndRepaintsBorderedNestedContainers(
        Orientation orientation,
        BorderStyle borderStyle,
        bool transparent)
    {
        using Form form = new();
        using SubSplitContainer control = new()
        {
            Dock = DockStyle.Fill,
            Orientation = orientation,
            BorderStyle = borderStyle,
            BackColor = transparent ? Color.Transparent : SystemColors.Control,
            VisualStylesMode = VisualStylesMode.Net11
        };
        using SplitContainer nested = new()
        {
            Dock = DockStyle.Fill,
            Orientation = orientation,
            BorderStyle = borderStyle
        };
        control.Panel1.Controls.Add(nested);
        using PaintTrackingControl panel1Content = CreateNestedPaintTrackingControl(nested.Panel1);
        using PaintTrackingControl panel2Content = CreateNestedPaintTrackingControl(control.Panel2);
        form.Controls.Add(control);
        form.Show();
        Application.DoEvents();
        Rectangle initialPanel1Bounds = control.Panel1.Bounds;
        Rectangle initialPanel2Bounds = control.Panel2.Bounds;
        Rectangle initialNestedBounds = nested.Bounds;
        Rectangle initialNestedPanel1Bounds = nested.Panel1.Bounds;
        Rectangle initialNestedPanel2Bounds = nested.Panel2.Bounds;
        Size initialContentSize = panel1Content.Size;
        int initialDistance = control.SplitterDistance;
        int movedCallCount = 0;
        control.SplitterMoved += (_, _) => movedCallCount++;
        Point anchor = GetSplitterCenter(control);
        Point position = GetDragPosition(control, anchor, 10);

        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, anchor.X, anchor.Y, 0));
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, position.X, position.Y, 0));
        Assert.Equal(initialDistance + 10, control.SplitterDistance);
        Assert.NotEqual(initialNestedBounds, nested.Bounds);
        panel1Content.ResetPaintCallCount();
        panel2Content.ResetPaintCallCount();
        control.SplitterMoving += (_, e) => e.Cancel = true;
        position = GetDragPosition(control, anchor, 20);
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, position.X, position.Y, 0));

        Assert.Equal(initialDistance, control.SplitterDistance);
        Assert.Equal(initialPanel1Bounds, control.Panel1.Bounds);
        Assert.Equal(initialPanel2Bounds, control.Panel2.Bounds);
        Assert.Equal(initialNestedBounds, nested.Bounds);
        Assert.Equal(initialNestedPanel1Bounds, nested.Panel1.Bounds);
        Assert.Equal(initialNestedPanel2Bounds, nested.Panel2.Bounds);
        Assert.Equal(initialContentSize, panel1Content.Size);
        Assert.True(panel1Content.PaintCallCount > 0);
        Assert.True(panel2Content.PaintCallCount > 0);
        Assert.Equal(0, movedCallCount);
        Assert.False(control.Capture);
    }

    [WinFormsTheory]
    [InlineData(Orientation.Horizontal)]
    [InlineData(Orientation.Vertical)]
    public void SplitContainer_KeyMove_WithHandleRepaintsBothPanelTrees(Orientation orientation)
    {
        using Form form = new();
        using SubSplitContainer control = new()
        {
            Dock = DockStyle.Fill,
            Orientation = orientation,
            VisualStylesMode = VisualStylesMode.Net11
        };
        using PaintTrackingControl panel1Content = CreateNestedPaintTrackingControl(control.Panel1);
        using PaintTrackingControl panel2Content = CreateNestedPaintTrackingControl(control.Panel2);
        form.Controls.Add(control);
        form.Show();
        Application.DoEvents();
        control.TestAccessor.Dynamic._splitterFocused = true;
        int initialDistance = control.SplitterDistance;
        Size initialPanel1ContentSize = panel1Content.Size;
        Size initialPanel2ContentSize = panel2Content.Size;
        panel1Content.ResetPaintCallCount();
        panel2Content.ResetPaintCallCount();
        Keys key = orientation == Orientation.Vertical ? Keys.Right : Keys.Down;

        control.OnKeyDown(new KeyEventArgs(key));

        Assert.Equal(initialDistance + control.SplitterIncrement, control.SplitterDistance);
        Assert.NotEqual(initialPanel1ContentSize, panel1Content.Size);
        Assert.NotEqual(initialPanel2ContentSize, panel2Content.Size);
        Assert.True(panel1Content.PaintCallCount > 0);
        Assert.True(panel2Content.PaintCallCount > 0);
        Assert.False(control.IsUpdating());
        control.OnKeyUp(new KeyEventArgs(key));
        Assert.Equal(initialDistance + control.SplitterIncrement, control.SplitterDistance);
    }

    [WinFormsFact]
    public void SplitContainer_MouseMove_LayoutExceptionResumesRedrawAndPropagatesException()
    {
        using SubSplitContainer control = new()
        {
            Size = new Size(300, 100),
            VisualStylesMode = VisualStylesMode.Net11
        };
        Point anchor = GetSplitterCenter(control);
        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, anchor.X, anchor.Y, 0));
        Assert.True(control.IsHandleCreated);
        InvalidOperationException expectedException = new("Layout failed.");
        LayoutEventHandler handler = (_, _) => throw expectedException;
        control.Panel2.Layout += handler;

        try
        {
            InvalidOperationException actualException = Assert.Throws<InvalidOperationException>(
                () => control.OnMouseMove(
                    new MouseEventArgs(MouseButtons.Left, 0, anchor.X + 10, anchor.Y, 0)));

            Assert.Same(expectedException, actualException);
            Assert.False(control.IsUpdating());
            Assert.False(control.TestAccessor.Dynamic._setSplitterDistance);
        }
        finally
        {
            control.Panel2.Layout -= handler;
            control.Capture = false;
        }
    }

    [WinFormsTheory]
    [InlineData(Orientation.Horizontal)]
    [InlineData(Orientation.Vertical)]
    public void SplitContainer_ClassicKeyMove_RepeatedKeysDeferLayoutAndPreserveLegacyEventTiming(
        Orientation orientation)
    {
        using SubSplitContainer control = new()
        {
            Orientation = orientation,
            Size = new Size(300, 300),
            VisualStylesMode = VisualStylesMode.Classic,
            SplitterIncrement = 10
        };
        control.TestAccessor.Dynamic._splitterFocused = true;
        int initialDistance = control.SplitterDistance;
        Rectangle initialPanel1Bounds = control.Panel1.Bounds;
        Rectangle initialPanel2Bounds = control.Panel2.Bounds;
        int movingCallCount = 0;
        List<int> notifiedDistances = [];
        control.SplitterMoving += (_, _) => movingCallCount++;
        control.SplitterMoved += (_, _) => notifiedDistances.Add(control.SplitterDistance);
        Keys increase = orientation == Orientation.Vertical ? Keys.Right : Keys.Down;
        Keys decrease = orientation == Orientation.Vertical ? Keys.Left : Keys.Up;

        control.OnKeyDown(new KeyEventArgs(increase));

        Assert.Equal(0, movingCallCount);
        Assert.Equal(initialDistance, control.SplitterDistance);
        Assert.Equal(initialPanel1Bounds, control.Panel1.Bounds);
        Assert.Equal(initialPanel2Bounds, control.Panel2.Bounds);
        Assert.Equal(initialDistance + 10, (int)control.TestAccessor.Dynamic._lastDrawSplit);
        Assert.Empty(notifiedDistances);

        control.OnKeyDown(new KeyEventArgs(increase));
        control.OnKeyDown(new KeyEventArgs(decrease));

        Assert.Equal(2, movingCallCount);
        Assert.Equal(initialDistance, control.SplitterDistance);
        Assert.Equal(initialPanel1Bounds, control.Panel1.Bounds);
        Assert.Equal(initialPanel2Bounds, control.Panel2.Bounds);
        Assert.Equal(initialDistance + 10, (int)control.TestAccessor.Dynamic._lastDrawSplit);
        Assert.Empty(notifiedDistances);

        control.OnKeyUp(new KeyEventArgs(decrease));

        Assert.Equal(initialDistance + 10, control.SplitterDistance);
        Assert.Equal(initialDistance + 10,
            orientation == Orientation.Vertical ? control.Panel1.Width : control.Panel1.Height);
        Assert.Equal([initialDistance + 10], notifiedDistances);
        Assert.Equal(-1, (int)control.TestAccessor.Dynamic._lastDrawSplit);
        Assert.False(control.TestAccessor.Dynamic._splitBegin);
        Assert.False(control.TestAccessor.Dynamic._splitMove);

        control.OnKeyUp(new KeyEventArgs(increase));
        Assert.Equal([initialDistance + 10], notifiedDistances);
    }

    [WinFormsTheory]
    [InlineData(Orientation.Horizontal, 0)]
    [InlineData(Orientation.Vertical, 0)]
    [InlineData(Orientation.Horizontal, 0x0100)]
    [InlineData(Orientation.Vertical, 0x0100)]
    [InlineData(Orientation.Horizontal, 0x0104)]
    [InlineData(Orientation.Vertical, 0x0104)]
    public void SplitContainer_ClassicMouseMove_CancellationDiscardsPendingLayoutAndClearsReversibleBar(
        Orientation orientation,
        int cancelMessageId)
    {
        using SubSplitContainer control = new()
        {
            Orientation = orientation,
            Size = new Size(300, 300),
            VisualStylesMode = VisualStylesMode.Classic
        };
        int initialDistance = control.SplitterDistance;
        Rectangle initialPanel1Bounds = control.Panel1.Bounds;
        Rectangle initialPanel2Bounds = control.Panel2.Bounds;
        int movedCallCount = 0;
        control.SplitterMoved += (_, _) => movedCallCount++;
        Point anchor = GetSplitterCenter(control);
        Point position = GetDragPosition(control, anchor, 10);
        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, anchor.X, anchor.Y, 0));
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, position.X, position.Y, 0));
        Assert.True(control.Capture);
        Assert.Equal(initialDistance + 10, (int)control.TestAccessor.Dynamic._lastDrawSplit);

        if (cancelMessageId == 0)
        {
            SplitterCancelEventHandler cancelHandler = (_, e) => e.Cancel = true;
            control.SplitterMoving += cancelHandler;
            try
            {
                position = GetDragPosition(control, anchor, 20);
                control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, position.X, position.Y, 0));
            }
            finally
            {
                control.SplitterMoving -= cancelHandler;
            }
        }
        else
        {
            IMessageFilter filter = (IMessageFilter)control.TestAccessor.Dynamic._splitContainerMessageFilter;
            Keys key = cancelMessageId == 0x0100 ? Keys.Escape : Keys.Menu;
            Message message = Message.Create(control.Handle, cancelMessageId, (nint)key, 0);
            Assert.True(filter.PreFilterMessage(ref message));
        }

        Assert.Equal(initialDistance, control.SplitterDistance);
        Assert.Equal(initialPanel1Bounds, control.Panel1.Bounds);
        Assert.Equal(initialPanel2Bounds, control.Panel2.Bounds);
        Assert.Equal(0, movedCallCount);
        Assert.False(control.Capture);
        Assert.Equal(-1, (int)control.TestAccessor.Dynamic._lastDrawSplit);
        Assert.Null((object?)control.TestAccessor.Dynamic._splitContainerMessageFilter);

        control.OnMouseMove(new MouseEventArgs(MouseButtons.None, 0, position.X, position.Y, 0));
        control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, position.X, position.Y, 0));
        Assert.Equal(initialDistance, control.SplitterDistance);
        Assert.Equal(0, movedCallCount);
        Assert.Equal(-1, (int)control.TestAccessor.Dynamic._lastDrawSplit);

        position = GetDragPosition(control, anchor, 10);
        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, anchor.X, anchor.Y, 0));
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, position.X, position.Y, 0));
        Assert.Equal(initialDistance, control.SplitterDistance);
        control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, position.X, position.Y, 0));
        Assert.Equal(initialDistance + 10, control.SplitterDistance);
        Assert.Equal(1, movedCallCount);
        Assert.False(control.Capture);
        Assert.Equal(-1, (int)control.TestAccessor.Dynamic._lastDrawSplit);
    }

    [WinFormsTheory]
    [InlineData(Orientation.Horizontal)]
    [InlineData(Orientation.Vertical)]
    public void SplitContainer_ClassicMouseMove_AfterLiveCompletionRestoresDeferredLayoutAndReversibleBar(
        Orientation orientation)
    {
        using SubSplitContainer control = new()
        {
            Orientation = orientation,
            Size = new Size(300, 300),
            VisualStylesMode = VisualStylesMode.Net11
        };
        Point anchor = GetSplitterCenter(control);
        Point position = GetDragPosition(control, anchor, 10);
        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, anchor.X, anchor.Y, 0));
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, position.X, position.Y, 0));
        control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, position.X, position.Y, 0));

        control.VisualStylesMode = VisualStylesMode.Classic;
        int initialDistance = control.SplitterDistance;
        Rectangle initialPanel1Bounds = control.Panel1.Bounds;
        Rectangle initialPanel2Bounds = control.Panel2.Bounds;
        int movedCallCount = 0;
        control.SplitterMoved += (_, _) => movedCallCount++;
        anchor = GetSplitterCenter(control);
        position = GetDragPosition(control, anchor, 10);
        control.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, anchor.X, anchor.Y, 0));
        control.OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, position.X, position.Y, 0));

        Assert.Equal(initialDistance, control.SplitterDistance);
        Assert.Equal(initialPanel1Bounds, control.Panel1.Bounds);
        Assert.Equal(initialPanel2Bounds, control.Panel2.Bounds);
        Assert.Equal(initialDistance + 10, (int)control.TestAccessor.Dynamic._lastDrawSplit);
        Assert.Equal(0, movedCallCount);

        control.OnMouseUp(new MouseEventArgs(MouseButtons.Left, 1, position.X, position.Y, 0));

        Assert.Equal(initialDistance + 10, control.SplitterDistance);
        Assert.Equal(initialDistance + 10,
            orientation == Orientation.Vertical ? control.Panel1.Width : control.Panel1.Height);
        Assert.Equal(1, movedCallCount);
        Assert.False(control.Capture);
        Assert.Equal(-1, (int)control.TestAccessor.Dynamic._lastDrawSplit);
    }

    private static Point GetSplitterCenter(SplitContainer control)
    {
        Rectangle rectangle = control.SplitterRectangle;

        return new Point(rectangle.Left + rectangle.Width / 2, rectangle.Top + rectangle.Height / 2);
    }

    private static Point GetDragPosition(SplitContainer control, Point anchor, int offset)
        => control.Orientation == Orientation.Horizontal
            ? new Point(anchor.X, anchor.Y + offset)
            : new Point(anchor.X + (control.RightToLeft == RightToLeft.Yes ? -offset : offset), anchor.Y);

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
