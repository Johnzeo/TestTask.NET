
using System.Collections.Concurrent;
using Microsoft.Playwright;

namespace TestTask.Recorder.Helpers;

public static class MouseHelper
{
    private static readonly ConcurrentDictionary<IPage, (float X, float Y)> Positions = new();

    public static async Task MoveSmoothlyAsync(
        IPage page,
        float targetX,
        float targetY)
    {
        await EnsureCursorAsync(page);

        var start = Positions.GetOrAdd(
            page,
            _ => (960f, 540f));

        var startX = start.X;
        var startY = start.Y;

        var distance = Math.Sqrt(
            Math.Pow(targetX - startX, 2) +
            Math.Pow(targetY - startY, 2));

        var duration = Math.Clamp(distance * 0.45, 350, 900);
        var steps = Math.Clamp((int)(duration / 16), 20, 55);

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        for (var i = 1; i <= steps; i++)
        {
            var t = (double)i / steps;
            var eased = t * t * (3 - 2 * t);

            var x = startX + (targetX - startX) * eased;
            var y = startY + (targetY - startY) * eased;

            await page.Mouse.MoveAsync((float)x, (float)y);

            var targetElapsed = duration * t;
            var remaining = targetElapsed - stopwatch.Elapsed.TotalMilliseconds;

            if (remaining > 0)
            {
                await Task.Delay((int)remaining);
            }
        }

        stopwatch.Stop();

        Positions[page] = (targetX, targetY);

        await Task.Delay(150);
    }

    public static async Task ClickSmoothlyAsync(
        IPage page,
        float x,
        float y)
    {
        await MoveSmoothlyAsync(page, x, y);

        await page.Mouse.ClickAsync(x, y);
    }

    private static async Task EnsureCursorAsync(IPage page)
    {
        var position = Positions.GetOrAdd(
            page,
            _ => (960f, 540f));

        await page.EvaluateAsync(
            """
            (position) => {
                if (document.getElementById('__recorder_cursor')) {
                    return;
                }

                const style = document.createElement('style');

                style.textContent = `
                    @keyframes recorder-ripple {
                        0% {
                            transform: translate(-50%, -50%) scale(0.2);
                            opacity: 0.8;
                        }
                        100% {
                            transform: translate(-50%, -50%) scale(1.5);
                            opacity: 0;
                        }
                    }

                    .recorder-ripple {
                        position: fixed;
                        width: 64px;
                        height: 64px;
                        border: 3px solid #ffd700;
                        border-radius: 50%;
                        pointer-events: none;
                        z-index: 2147483646;
                        animation: recorder-ripple 0.55s ease-out forwards;
                    }
                `;

                document.head.appendChild(style);

                const cursor = document.createElement('div');

                cursor.id = '__recorder_cursor';

                cursor.style.cssText = `
                    position: fixed;
                    left: ${position.x}px;
                    top: ${position.y}px;
                    width: 32px;
                    height: 32px;
                    pointer-events: none;
                    z-index: 2147483647;
                    will-change: left, top;
                `;

                cursor.innerHTML = `
                    <div style="
                        position: absolute;
                        left: -30px;
                        top: -31px;
                        width: 64px;
                        height: 64px;
                        border-radius: 50%;
                        background: rgba(255, 215, 0, 0.28);
                    "></div>

                    <svg
                        width="32"
                        height="32"
                        viewBox="0 0 32 32"
                        style="position:absolute;left:0;top:0"
                    >
                        <path
                            d="M2 1 L2 27 L9 20 L14 31 L19 29 L14 18 L24 18 Z"
                            fill="white"
                            stroke="black"
                            stroke-width="1.8"
                            stroke-linejoin="round"
                        />
                    </svg>
                `;

                document.body.appendChild(cursor);

                document.addEventListener('mousemove', event => {
                    cursor.style.left = event.clientX + 'px';
                    cursor.style.top = event.clientY + 'px';
                });

                document.addEventListener('click', event => {
                    const ripple = document.createElement('div');

                    ripple.className = 'recorder-ripple';
                    ripple.style.left = event.clientX + 'px';
                    ripple.style.top = event.clientY + 'px';

                    document.body.appendChild(ripple);

                    ripple.addEventListener('animationend', () => {
                        ripple.remove();
                    });
                });
            }
            """,
            new { x = position.X, y = position.Y });
    }
}
