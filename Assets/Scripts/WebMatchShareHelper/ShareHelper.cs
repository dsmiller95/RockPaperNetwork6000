using System.Runtime.InteropServices;
using UnityEngine;

namespace WebMatchShareHelper
{
    public class ShareHelper
    {
        
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void JS_ShareHelper_Share(string matchCode);

        [DllImport("__Internal")]
        private static extern string JS_ShareHelper_GetShared();
#else
        private static void JS_ShareHelper_Share(string matchCode)
        {
            GUIUtility.systemCopyBuffer = matchCode;
        }
        
        private static string JS_ShareHelper_GetShared()
        {
            return null;
        }
#endif
        
        public static void Share(string matchCode)
        {
            Debug.Log("Sharing match code: " + matchCode);
            JS_ShareHelper_Share(matchCode);
        }

        private static string m_getSharedMemo = null;
        private static int m_getSharedMemoFrame = int.MinValue;
        
        
        /// <summary>
        /// Get a match code that was shared by opening this page. memoized across multiple frames.
        /// </summary>
        /// <returns></returns>
        public static string GetShared()
        {
            if(Time.frameCount >= m_getSharedMemoFrame + 60)
            {
                m_getSharedMemo = JS_ShareHelper_GetShared();
                m_getSharedMemoFrame = Time.frameCount;
            }

            return m_getSharedMemo;
        }
    }
}