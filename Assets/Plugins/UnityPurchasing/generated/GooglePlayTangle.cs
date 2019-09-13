#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("kSOgg5Gsp6iLJ+knVqygoKCkoaJuBWp9J4j5EzDaDnhuqX5nsinDWyOgrqGRI6CroyOgoKEcdjY3002/mBnn5dYZ8SK8eSMg7icYPn9jVfI/Q0kDHIfQtw6v06PTJKPB4TkcjRx2Pw3az0YHOx/cTvO9sCd/vz0GwSyxX58u6p6QlXddm5Oj9dYQZv7U+PiRlWJ6XLyMPh5pGvIE9gTVkIWBAkb5syB/XEeUQEjKS8CiRwWZhi8Bt2/Om8vmDn/qtG7VnqRz7FlX9tPJ/kQvThnGRH+SgI5g9jIeKUh5RyM1XCLsWXX43vNuSuGQ5oXnlOkYZD4JecoaUOl4AJKe0idlsUoWzHwRCPa2Fdb8NZ4eoGXFJEoQtN1q0B8/UYnU2KOioKGg");
        private static int[] order = new int[] { 0,6,6,10,5,10,9,8,8,9,11,13,12,13,14 };
        private static int key = 161;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
#endif
