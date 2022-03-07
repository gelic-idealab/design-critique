using System.Runtime.InteropServices;

namespace Komodo.Runtime
{
    public static class SocketIOJSLib
    {
        public static int SUCCESS = 0;

        public static int FAILURE = 1;

        // import callable js functions
        // socket.io with webgl
        // https://www.gamedev.net/articles/programming/networking-and-multiplayer/integrating-socketio-with-unity-5-webgl-r4365/
        [DllImport("__Internal")]
        public static extern string SetSocketIOAdapterName(string name);

        [DllImport("__Internal")]
        public static extern int SetSyncEventListeners();

        [DllImport("__Internal")]
        public static extern int OpenSyncConnection();

        [DllImport("__Internal")]
        public static extern int OpenChatConnection();

        [DllImport("__Internal")]
        public static extern int JoinSyncSession();

        [DllImport("__Internal")]
        public static extern int JoinChatSession();

        [DllImport("__Internal")]
        public static extern int SendStateCatchUpRequest();

        [DllImport("__Internal")]
        public static extern int SetChatEventListeners();

        [DllImport("__Internal")]
        public static extern int GetClientIdFromBrowser();

        [DllImport("__Internal")]
        public static extern int GetSessionIdFromBrowser();

        [DllImport("__Internal")]
        public static extern int GetIsTeacherFlagFromBrowser();

        // [DllImport("__Internal")]
        // private static extern void InitSocketIOReceivePosition(float[] array, int size);

        // [DllImport("__Internal")]
        // private static extern void SocketIOSendPosition(float[] array, int size);

        // [DllImport("__Internal")]
        // private static extern void SocketIOSendInteraction(int[] array, int size);

        // [DllImport("__Internal")]
        // private static extern void InitSocketIOReceiveInteraction(int[] array, int size);

        // [DllImport("__Internal")]
        // private static extern void InitReceiveDraw(float[] array, int size);

        // [DllImport("__Internal")]
        // private static extern void SendDraw(float[] array, int size);

        [DllImport("__Internal")]
        public static extern int EnableVRButton();

        [DllImport("__Internal")]
        public static extern string GetSessionDetails();

        // TODO(rob): move this to GlobalMessageManager.cs
        [DllImport("__Internal")]
        public static extern void BrowserEmitMessage(string type, string message);

        [DllImport("__Internal")]
        public static extern int LeaveSyncSession();

        [DllImport("__Internal")]
        public static extern int LeaveChatSession();

        [DllImport("__Internal")]
        public static extern int CloseSyncConnection();

        [DllImport("__Internal")]
        public static extern int CloseChatConnection();
    }
}