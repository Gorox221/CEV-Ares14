// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface;
using Robust.Shared.Maths;
using Robust.Shared.Utility;

namespace Content.IntegrationTests.Tests
{
    [TestFixture]
    public sealed class RoleTooltipSpacingTest
    {
        [Test]
        public async Task Spacing()
        {
            await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true, DummyTicker = true });
            var client = pair.Client;

            await client.WaitAssertion(() =>
            {
                float Height(int newlines)
                {
                    var msg = new FormattedMessage();
                    msg.AddText("desc");
                    for (var i = 0; i < newlines; i++)
                        msg.PushNewline();
                    msg.PushColor(Color.White);
                    msg.AddText("Header");
                    msg.Pop();
                    msg.PushNewline();
                    msg.AddText("item");

                    var tooltip = new Tooltip();
                    tooltip.SetMessage(msg);
                    tooltip.Measure(Vector2Helpers.Infinity);
                    return tooltip.DesiredSize.Y;
                }

                var hSingle = Height(0);
                var hBreak = Height(1);
                var hBlank = Height(2);

                Assert.Fail($"single={hSingle} break={hBreak} blank={hBlank} breakDelta={hBreak - hSingle} blankDelta={hBlank - hBreak}");
            });

            await pair.CleanReturnAsync();
        }
    }
}