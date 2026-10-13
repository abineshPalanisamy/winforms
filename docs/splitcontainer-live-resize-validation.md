# SplitContainer live-resize validation

Tracks the regression and evidence requirements of
[dotnet/winforms#14931](https://github.com/dotnet/winforms/issues/14931).

## Automated regression coverage

`src\test\unit\System.Windows.Forms\System\Windows\Forms\SplitContainerTests.cs`
covers:

| Scenario | Coverage |
| --- | --- |
| Live mouse movement and completion | Net11/Latest, horizontal/vertical, LTR/RTL; two moves before completion |
| Keyboard movement and completion | Net11/Latest, horizontal/vertical |
| External capture loss | Net11/Latest, horizontal/vertical, LTR/RTL; rollback, button-free movement, and restarting |
| Reentrant capture loss | Capture stolen inside `SplitterMoving` |
| Nested repaint and handle lifetime | Three nested `TableLayoutPanel` levels in each panel; three drags with five moves each, reversals, and return to the initial distance |
| Per-move evidence | Splitter distance, panel size, deepest content size and painting, original HWND identity, and no handle creation/destruction during the tracked interval |
| Legacy behavior and cancellation | Existing Classic deferred-layout and cancellation/notification tests retained |

Timing and process-wide memory/handle thresholds are deliberately excluded from
unit tests. They depend on runtime caches, other windows, and machine/session
conditions; deterministic control-level handle lifetime belongs in unit tests,
while process resource trends belong in separate stress evidence.

## Repeatable stress evidence

The standalone harness is located outside this repository at the sibling
`..\SplitContainerStress-14931`. Retain its `Program.cs`, project, and README
alongside the raw measurements when sharing evidence. Its README specifies the
fixed trajectory, payload sizes, warm-up, repetitions, resource snapshots, build
inputs, and console/RDP comparison protocol.

No new stress or performance execution is authorized for this revision. The
harness is unbuilt and unexecuted. **Actual measurements are pending.** No
before/after performance, leak, RDP, or hardware acceptance claim can be made from
the existence of tests or the harness.

For each measured row, publish the exact commands, before/after source revisions,
assembly hashes, SDK/runtime, build configuration, OS/process architecture,
logical processor count, DPI, color mode, session type, payload, orientation, RTL,
all three repetitions, and raw JSON. Report paint activity, wall/CPU/move latency,
memory trends, GDI/USER/process handles, control handle churn, and final layout.

| Required evidence row | Current status |
| --- | --- |
| Matched before/after light, medium, and heavy workloads | Pending execution and publication of raw measurements |
| Net11/Latest and Classic comparison | Pending stress execution; Classic is not a substitute for a pre-change binary |
| Console light and dark rendering | Pending exploratory and measured runs |
| Real Terminal Services/RDP session | Pending connected-session execution; console results do not satisfy this row |
| Deep heavy payload and longer-duration resource trends | Harness supplied; measurements and visual inspection pending |
| Windows on ARM and genuinely slow hardware | Pending execution on real hardware |

Run the same workload in a real RDP session and include client resolution,
scaling, codec, bandwidth, and latency. Synchronous server-side paint timing does
not measure client-visible frame delivery. Visual flicker, accessibility,
long-duration leaks, and real-device responsiveness still require their own
exploratory evidence.
