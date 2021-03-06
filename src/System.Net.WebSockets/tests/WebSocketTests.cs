// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Net.WebSockets.Tests
{
    public sealed class WebSocketTests
    {
        [Fact]
        public static void DefaultKeepAliveInterval_ValidValue()
        {
            Assert.True(WebSocket.DefaultKeepAliveInterval > TimeSpan.Zero);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public static void CreateClientBuffer_InvalidSendValues(int size)
        {
            Assert.Throws<ArgumentOutOfRangeException>("sendBufferSize", () => WebSocket.CreateClientBuffer(256, size));
        }

        [Theory]
        [InlineData(16)]
        [InlineData(64 * 1024)]
        public static void CreateClientBuffer_ValidSendValues(int size)
        {
            ArraySegment<byte> buffer = WebSocket.CreateClientBuffer(256, size);
            Assert.InRange(buffer.Count, size, int.MaxValue);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public static void CreateClientBuffer_InvalidReceiveValues(int size)
        {
            Assert.Throws<ArgumentOutOfRangeException>("receiveBufferSize", () => WebSocket.CreateClientBuffer(size, 16));
        }

        [Theory]
        [InlineData(256)]
        [InlineData(64 * 1024)]
        public static void CreateClientBuffer_ValidReceiveValues(int size)
        {
            ArraySegment<byte> buffer = WebSocket.CreateClientBuffer(size, 16);
            Assert.InRange(buffer.Count, size, int.MaxValue);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public static void CreateServerBuffer_InvalidReceiveValues(int size)
        {
            Assert.Throws<ArgumentOutOfRangeException>("receiveBufferSize", () => WebSocket.CreateServerBuffer(size));
        }

        [Theory]
        [InlineData(256)]
        [InlineData(64 * 1024)]
        public static void CreateServerBuffer_ValidReceiveValues(int size)
        {
            ArraySegment<byte> buffer = WebSocket.CreateServerBuffer(size);
            Assert.InRange(buffer.Count, size, int.MaxValue);
        }

        [SkipOnTargetFramework(TargetFrameworkMonikers.NetFramework)]
        [Fact]
        public static void CreateClientWebSocket_Unsupported()
        {
            Assert.Throws<PlatformNotSupportedException>(() =>
                WebSocket.CreateClientWebSocket(new MemoryStream(), "", 256, 16, TimeSpan.FromSeconds(30), false, new ArraySegment<byte>(new byte[64 * 1024])));
        }

        [SkipOnTargetFramework(TargetFrameworkMonikers.NetFramework)]
        [Fact]
        public static void RegisterPrefixes_Unsupported()
        {
            Assert.Throws<PlatformNotSupportedException>(() => WebSocket.RegisterPrefixes());
        }

        [Fact]
        public static void IsApplicationTargeting45_AlwaysTrue()
        {
#pragma warning disable 0618 // Obsolete API
            Assert.True(WebSocket.IsApplicationTargeting45());
#pragma warning restore 0618
        }

        [Theory]
        [InlineData(WebSocketState.None)]
        [InlineData(WebSocketState.Connecting)]
        [InlineData(WebSocketState.Open)]
        [InlineData(WebSocketState.CloseSent)]
        [InlineData(WebSocketState.CloseReceived)]
        [InlineData((WebSocketState)(-1))]
        [InlineData((WebSocketState)(7))]
        public static void IsStateTerminal_NonTerminalReturnsFalse(WebSocketState state)
        {
            Assert.False(ExposeProtectedWebSocket.IsStateTerminal(state));
        }

        [Theory]
        [InlineData(WebSocketState.Closed)]
        [InlineData(WebSocketState.Aborted)]
        public static void IsStateTerminal_TerminalReturnsTrue(WebSocketState state)
        {
            Assert.True(ExposeProtectedWebSocket.IsStateTerminal(state));
        }

        [Theory]
        [InlineData(WebSocketState.Closed, new WebSocketState[] { })]
        [InlineData(WebSocketState.Closed, new WebSocketState[] { WebSocketState.Open })]
        [InlineData(WebSocketState.Open, new WebSocketState[] { WebSocketState.Aborted, WebSocketState.CloseSent })]
        public static void ThrowOnInvalidState_ThrowsIfNotInValidList(WebSocketState state, WebSocketState[] validStates)
        {
            Assert.Throws<WebSocketException>(() => ExposeProtectedWebSocket.ThrowOnInvalidState(state, validStates));
        }

        [Theory]
        [InlineData(WebSocketState.Open, new WebSocketState[] { WebSocketState.Open })]
        [InlineData(WebSocketState.Open, new WebSocketState[] { WebSocketState.Open, WebSocketState.Aborted, WebSocketState.Closed })]
        [InlineData(WebSocketState.Open, new WebSocketState[] { WebSocketState.Aborted, WebSocketState.Open, WebSocketState.Closed })]
        [InlineData(WebSocketState.Open, new WebSocketState[] { WebSocketState.Aborted, WebSocketState.CloseSent, WebSocketState.Open })]
        public static void ThrowOnInvalidState_SuccessIfInList(WebSocketState state, WebSocketState[] validStates)
        {
            ExposeProtectedWebSocket.ThrowOnInvalidState(state, validStates);
        }

        public abstract class ExposeProtectedWebSocket : WebSocket
        {
            public new static bool IsStateTerminal(WebSocketState state) => 
                WebSocket.IsStateTerminal(state);
            public new static void ThrowOnInvalidState(WebSocketState state, params WebSocketState[] validStates) =>
                WebSocket.ThrowOnInvalidState(state, validStates);
        }
    }
}
